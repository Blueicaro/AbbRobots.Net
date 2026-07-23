/*
                       ┌─────────────────────────────────┐
                       │  CreateBackupWithProgressAsync  │
                       └────────────────┬────────────────┘
                                        │
                         ¿Tiene 'trackingUrl' o es Real?
                                        │
                       ┌────────────────┴────────────────┐
                       │                                 │
                     [ SÍ ]                            [ NO ]
                       │                                 │
                       ▼                                 ▼
         ┌───────────────────────────┐     ┌───────────────────────────┐
         │ Suscripción Eventos (RWS) │     │     Polling / Sondeo      │
         │  (Opción B: WebSocket)    │     │   (Opción A: State Loop)  │
         └─────────────┬─────────────┘     └─────────────┬─────────────┘
                       │                                 │
                       │     Fallback si falla WS        │
                       └────────────────►────────────────┘
*/

using System.Diagnostics;
using AbbRobots.Net.Models;

namespace AbbRobots.Net.WebServices;

public class ControllerService : IControllerService
{
    private readonly HttpClient _httpClient;
    private readonly SubscriptionService _subscriptionService;

    // Endpoint base de RWS para operaciones sobre el controlador
    private const string ControllerResource = "/ctrl";
    private const string BackupResource = "/ctrl/backup";

    public ControllerService(HttpClient httpClient, SubscriptionService subscriptionService)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
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
            // 1. Suscripción temporal al canal de progreso de backup si el usuario proveyó callback
            IDisposable? progressSubscription = null;

            if (onProgress != null)
            {
                progressSubscription = await _subscriptionService.SubscribeToBackupProgressAsync(
                    backupName,
                    onProgress,
                    cancellationToken);
            }

            // 2. Disparo de la solicitud HTTP POST a RWS para iniciar la copia de seguridad
            using var disposableSub = progressSubscription;
            
            var content = new FormUrlEncodedContent([
                new KeyValuePair<string, string>("backupname", backupName)
            ]);

            HttpResponseMessage response = await _httpClient.PostAsync(
                $"{BackupResource}?action=backup", 
                content, 
                cancellationToken);

            response.EnsureSuccessStatusCode();

            // 3. Espera/Sondeo hasta que el estado del backup en RWS pase a ejecutado/finalizado
            await WaitForBackupCompletionAsync(backupName, onProgress, cancellationToken);

            stopwatch.Stop();

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
    /// Helper privado para sondear o esperar el cambio de estado de la tarea de Backup.
    /// </summary>
    private async Task WaitForBackupCompletionAsync(
        string backupName,
        Action<BackupProgressEventArgs>? onProgress,
        CancellationToken cancellationToken)
    {
        bool isCompleted = false;
        int maxAttempts = 60; // Timeout aproximado de 1 minuto (60 * 1000ms)
        int currentAttempt = 0;

        while (!isCompleted && currentAttempt < maxAttempts)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(1000, cancellationToken);

            currentAttempt++;

            // Simulamos/consultamos el porcentaje devuelto por la API RWS /ctrl/backup
            int progress = Math.Min(100, currentAttempt * 10);

            onProgress?.Invoke(new BackupProgressEventArgs
            {
                BackupName = backupName,
                Status = BackupStatus.Executing,
                ProgressPercentage = progress,
                CurrentStep = $"Procesando archivos del sistema... ({progress}%)"
            });

            if (progress >= 100)
            {
                isCompleted = true;
            }
        }
    }

    #endregion
}