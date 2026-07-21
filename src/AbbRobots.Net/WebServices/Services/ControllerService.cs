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


using System.Net.Http.Json;
using System.Text.Json;
using AbbRobots.Net.Models;

namespace AbbRobots.Net.WebServices.Services;

public enum BackupState
{
    Idle,
    InExecution,
    Completed,
    Error,
    Unknown
}

public class ControllerService
{
    private readonly HttpClient _httpClient;

    public ControllerService(HttpClient client)
    {
        _httpClient = client;
    }
    public async Task<BackupState> GetBackupStateAsync()
    {

        string url = "ctrl/backup/state";
        var responseMessage = await _httpClient.GetRwsAsync(url);
        if (!responseMessage.IsSuccessStatusCode)
        {
            return BackupState.Unknown;
        }
        string jsonRaw = await responseMessage.Content.ReadAsStringAsync();
        var response = JsonSerializer.Deserialize<BackupStateModel>(jsonRaw);

        if (response == null)
        {
            return BackupState.Unknown;
        }

        return response.backupstate.ToLower() switch
        {
            "backup_init" or "init" => BackupState.InExecution,
            "backup_running" or "running" => BackupState.InExecution,
            "backup_completed" or "completed" => BackupState.Completed,
            "backup_error" or "error" => BackupState.Error,
            "idle" => BackupState.Idle,
            _ => BackupState.Unknown
        };
    }

    /// <summary>
    /// Ask the controller to create a backup directly.
    /// Works transparently on both virtual and real controllers.
    /// </summary>
    public async Task CreateBackupAsync(string backupName, string? customPath = null)
    {
        _ = await CreateBackupInternalAsync(backupName, customPath);
    }

    /// <summary>
    /// Starts creating a backup and monitors its progress until it is finished.
    /// Recommended for real controllers.
    /// </summary>
    public async Task CreateBackupWithProgressAsync(string backupName,
            IProgress<string>? onProgress = null,
            string? customPath = null)
    {
        string? subscriptionUrl = await CreateBackupInternalAsync(backupName, customPath);
        onProgress?.Report("Backup request successfully sent.");

        if (string.IsNullOrEmpty(subscriptionUrl))
        {
            onProgress?.Report("Backup complete");
            return;
        }
        await MonitorBackupProgressAsync(subscriptionUrl,onProgress);
    }
    /// <summary>
    /// Ask the controller to create a backup directly.
    /// Works transparently on both virtual and real controllers.
    /// </summary>
    private async Task<string?> CreateBackupInternalAsync(string backupName, string? customPath)
    {
        if (string.IsNullOrWhiteSpace(backupName))
        {
            throw new ArgumentException("The backup name cannot be empty.", nameof(backupName));
        }

        var payload = new CreateBackupRequestModel
        {
            BackupName = backupName,
            BackupPath = customPath
        };

        var response = await _httpClient.PostAsJsonAsync("ctrl/backup/create", payload);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error requesting backup on OmniCore ({response.StatusCode}): {errorContent}");
        }

        // Extraemos la ubicación del recurso de seguimiento (si existe)
        string? subscriptionUrl = response.Headers.Location?.ToString();

        System.Diagnostics.Debug.WriteLine($"[Controller] Backup request '{backupName}' sent. Tracking: {subscriptionUrl}");

        return subscriptionUrl;
    }

    private async Task MonitorBackupProgressAsync(string? trackingUrl, IProgress<string>? progress)
    {
      if (string.IsNullOrEmpty(trackingUrl))
        {
            await MonitorViaPollingAsync(progress);
            return;

        }

    try
        {
            await MonitorViaSubscriptionAsync(trackingUrl,progress);
        }
        catch(Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Controller] Failure to backup subscription: {ex.Message}. Applying Fallback to Polling.");
            await MonitorViaPollingAsync(progress); 
        }      
    }

    private async  Task MonitorViaPollingAsync(IProgress<string> progress, CancellationToken cancellationToken = default)
    {
        progress?.Report("Initiating tracking by poll (Polling)...");
        while (!cancellationToken.IsCancellationRequested)
        {
            BackupState state = await GetBackupStateAsync();
            switch (state)
            {
              case BackupState.InExecution:
                progress?.Report("Backup in progress......");
                break;

            case BackupState.Completed:
                progress?.Report("Backup completed successfully.");
                return;

            case BackupState.Error:
                throw new InvalidOperationException("The controller reported an error during the creation of the backup.");

            case BackupState.Unknown:
            case BackupState.Idle:
                // Si vuelve a Idle o Unknown tras haber iniciado, interpretamos el estado
                break;  
            }
        }
    }
}
