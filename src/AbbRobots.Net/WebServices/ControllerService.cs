/*
                       ┌─────────────────────────────────┐
                       │        CreateBackupAsync         │
                       └────────────────┬────────────────┘
                                        │
                              ¿_isVirtualController?
                                        │
                       ┌────────────────┴────────────────┐
                       │                                 │
                    [ FALSE ]                          [ TRUE ]
                  (controlador real)               (controlador virtual)
                       │                                 │
                       ▼                                 ▼
         ┌───────────────────────────┐     ┌───────────────────────────┐
         │ Suscripción a eventos RWS │     │     Polling / Sondeo      │
         │  (WebSocket /ctrl/backup) │     │  (GET /ctrl/backup cada 1s)│
         └───────────────────────────┘     └───────────────────────────┘

    La elección es transparente para quien llama a CreateBackupAsync: el
    mecanismo se decide internamente según el valor que AbbRobotClient
    detectó en ConnectAsync() (RobotWareService.IsVirtualController).
*/

using System.Diagnostics;
using System.Text.Json;
using AbbRobots.Net.WebServices.Models;

namespace AbbRobots.Net.WebServices;

public class ControllerService : IControllerService
{
    private readonly HttpClient _httpClient;
    private readonly SubscriptionService _subscriptionService;
    private readonly bool _isVirtualController;

    // Endpoint base de RWS para operaciones sobre el controlador
    private const string ControllerResource = "/ctrl";
    private const string BackupResource = "/ctrl/backup";

    public ControllerService(HttpClient httpClient, SubscriptionService subscriptionService, bool isVirtualController)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
        _isVirtualController = isVirtualController;
    }

    /// <inheritdoc />
    public async Task<IDisposable> SubscribeToStateAsync(
        Action<ControllerStateChangeEventArgs> onStateChanged,
        SubscriptionPriority priority = SubscriptionPriority.Medium)
    {
        ArgumentNullException.ThrowIfNull(onStateChanged);

        // 1. Definimos el handler local para filtrar y transformar eventos del WebSocket
        EventHandler<ControllerStateChangeEventArgs> handler = (sender, args) =>
        {
            onStateChanged(args);
        };

        // 2. Nos enganchamos a la tubería interna del SubscriptionService
        _subscriptionService.OnControllerStateChanged += handler;

        // 3. Suscribimos la ruta física del controlador en ABB RWS
        string resourceUri = $"{ControllerResource}/state;state";
        await _subscriptionService.SubscribeToResourceInternalAsync(resourceUri, priority);

        // 4. Retornamos la desuscripción para que el cliente la libere mediante Dispose()
        return new SubscriptionDisposer(() =>
        {
            _subscriptionService.OnControllerStateChanged -= handler;
        });
    }

    /// <inheritdoc />
    public async Task<BackupResult> CreateBackupAsync(
        string backupName,
        Action<BackupProgressEventArgs>? onProgress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(backupName);

        var stopwatch = Stopwatch.StartNew();

        // Notificación inicial de progreso
        onProgress?.Invoke(new BackupProgressEventArgs
        {
            BackupName = backupName,
            Status = BackupStatus.Pending,
            ProgressPercentage = 0,
            CurrentStep = "Iniciando solicitud de backup en la controladora..."
        });

        try
        {
            // Elegimos el mecanismo de progreso de forma transparente para quien llama:
            // - Controlador real: eventos por WebSocket (más eficiente, RWS los soporta).
            // - Controlador virtual: no acepta suscripciones a /ctrl/backup, así que sondeamos.
            IDisposable? progressSubscription = null;
            TaskCompletionSource<BackupProgressEventArgs>? completionSignal = null;

            if (!_isVirtualController)
            {
                completionSignal = new TaskCompletionSource<BackupProgressEventArgs>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

                progressSubscription = await _subscriptionService.SubscribeToBackupProgressAsync(
                    backupName,
                    args =>
                    {
                        onProgress?.Invoke(args);
                        if (args.Status is BackupStatus.Completed or BackupStatus.Failed)
                        {
                            completionSignal.TrySetResult(args);
                        }
                    },
                    cancellationToken);
            }

            using var disposableSub = progressSubscription;

            var content = new FormUrlEncodedContent([
                new KeyValuePair<string, string>("backupname", backupName)
            ]);

            HttpResponseMessage response = await _httpClient.PostAsync(
                $"{BackupResource}?action=backup", 
                content, 
                cancellationToken);

            response.EnsureSuccessStatusCode();

            // Esperamos a que termine, por eventos si tenemos señal en curso, o por sondeo si no.
            BackupProgressEventArgs? finalState = completionSignal != null
                ? await WaitForBackupCompletionByEventsAsync(backupName, completionSignal, cancellationToken)
                : await WaitForBackupCompletionByPollingAsync(backupName, onProgress, cancellationToken);

            stopwatch.Stop();

            if (finalState is { Status: BackupStatus.Failed })
            {
                return new BackupResult
                {
                    BackupName = backupName,
                    Success = false,
                    Duration = stopwatch.Elapsed,
                    ErrorDetails = finalState.ErrorMessage ?? finalState.CurrentStep
                };
            }

            // Notificación final de éxito
            onProgress?.Invoke(new BackupProgressEventArgs
            {
                BackupName = backupName,
                Status = BackupStatus.Completed,
                ProgressPercentage = 100,
                CurrentStep = "Backup completado correctamente."
            });

            return new BackupResult
            {
                BackupName = backupName,
                Success = true,
                BackupPath = $"/hd0/{backupName}",
                Duration = stopwatch.Elapsed
            };
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            
            onProgress?.Invoke(new BackupProgressEventArgs
            {
                BackupName = backupName,
                Status = BackupStatus.Failed,
                ProgressPercentage = 0,
                CurrentStep = "Operación cancelada por el usuario.",
                ErrorMessage = "El Token de cancelación fue activado."
            });

            return new BackupResult
            {
                BackupName = backupName,
                Success = false,
                Duration = stopwatch.Elapsed,
                ErrorDetails = "Operación cancelada."
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            onProgress?.Invoke(new BackupProgressEventArgs
            {
                BackupName = backupName,
                Status = BackupStatus.Failed,
                ProgressPercentage = 0,
                CurrentStep = "Error inesperado durante la ejecución del backup.",
                ErrorMessage = ex.Message
            });

            return new BackupResult
            {
                BackupName = backupName,
                Success = false,
                Duration = stopwatch.Elapsed,
                ErrorDetails = ex.Message
            };
        }
    }

    #region Private Helpers

    /// <summary>
    /// Sondea el estado real del backup en el controlador (GET /ctrl/backup) hasta que
    /// termine (Completed/Failed) o se agote el número máximo de intentos.
    /// Se usa cuando el controlador es virtual, ya que este no admite suscripciones
    /// a eventos de progreso de backup.
    /// </summary>
    private async Task<BackupProgressEventArgs> WaitForBackupCompletionByPollingAsync(
        string backupName,
        Action<BackupProgressEventArgs>? onProgress,
        CancellationToken cancellationToken)
    {
        const int maxAttempts = 60; // Timeout aproximado de 1 minuto (60 intentos * 1000ms)
        const int pollingDelayMs = 1000;

        BackupStatus? lastReportedStatus = null;
        var lastArgs = new BackupProgressEventArgs
        {
            BackupName = backupName,
            Status = BackupStatus.Pending,
            ProgressPercentage = 0,
            CurrentStep = "Esperando el primer sondeo de estado."
        };

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(pollingDelayMs, cancellationToken);

            string rawState = await GetBackupStateAsync(cancellationToken);
            BackupStatus status = MapBackupState(rawState);

            // Solo notificamos cuando el estado real cambia, en vez de inventar un porcentaje.
            if (status != lastReportedStatus)
            {
                lastArgs = new BackupProgressEventArgs
                {
                    BackupName = backupName,
                    Status = status,
                    ProgressPercentage = status == BackupStatus.Completed ? 100 : 0,
                    CurrentStep = $"Estado del backup reportado por el controlador: '{rawState}'."
                };
                onProgress?.Invoke(lastArgs);
                lastReportedStatus = status;
            }

            if (status is BackupStatus.Completed or BackupStatus.Failed)
            {
                return lastArgs;
            }
        }

        throw new TimeoutException(
            $"El backup '{backupName}' no finalizó dentro del tiempo máximo de espera ({maxAttempts * pollingDelayMs / 1000}s).");
    }

    /// <summary>
    /// Espera a que llegue, vía WebSocket, un evento de progreso con estado final
    /// (Completed/Failed). Se usa cuando el controlador es real, que sí soporta
    /// suscripciones al recurso /ctrl/backup.
    /// </summary>
    private static async Task<BackupProgressEventArgs> WaitForBackupCompletionByEventsAsync(
        string backupName,
        TaskCompletionSource<BackupProgressEventArgs> completionSignal,
        CancellationToken cancellationToken)
    {
        const int timeoutSeconds = 60;

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

        using var registration = timeoutCts.Token.Register(() =>
            completionSignal.TrySetCanceled(timeoutCts.Token));

        try
        {
            return await completionSignal.Task;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException(
                $"El backup '{backupName}' no finalizó dentro del tiempo máximo de espera ({timeoutSeconds}s) esperando eventos de progreso.");
        }
    }

    /// <summary>
    /// Consulta el recurso /ctrl/backup del controlador y devuelve el valor crudo de 'backup-state'.
    /// </summary>
    private async Task<string> GetBackupStateAsync(CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await _httpClient.GetRwsAsync(BackupResource);
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync(cancellationToken);

        BackupStateResponseModel? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<BackupStateResponseModel>(json);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("The ABB robot returned a response that could not be parsed as a backup state.", ex);
        }

        return parsed?.State?.FirstOrDefault()?.backupstate ?? string.Empty;
    }

    /// <summary>
    /// Traduce el texto de estado devuelto por RWS a nuestro enum <see cref="BackupStatus"/>.
    /// Nota: los valores exactos de 'backup-state' no están documentados de forma exhaustiva,
    /// por lo que se hace una coincidencia flexible por palabras clave.
    /// </summary>
    private static BackupStatus MapBackupState(string rawState)
    {
        if (string.IsNullOrWhiteSpace(rawState))
        {
            return BackupStatus.Pending;
        }

        string normalized = rawState.ToLowerInvariant();

        if (normalized.Contains("error") || normalized.Contains("fail"))
        {
            return BackupStatus.Failed;
        }

        if (normalized.Contains("done") || normalized.Contains("complete") || normalized.Contains("finished") || normalized.Contains("ready"))
        {
            return BackupStatus.Completed;
        }

        if (normalized.Contains("progress") || normalized.Contains("running") || normalized.Contains("backing") || normalized.Contains("busy"))
        {
            return BackupStatus.Executing;
        }

        return BackupStatus.Pending;
    }

    #endregion
}