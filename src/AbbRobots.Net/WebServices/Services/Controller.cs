using System.ComponentModel.DataAnnotations;
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
        
        string url ="ctrl/backup/state";
        var responseMessage = await _httpClient.GetRwsAsync(url);
        if (!responseMessage.IsSuccessStatusCode)
        {
            return  BackupState.Unknown;
        }
        string jsonRaw = await responseMessage.Content.ReadAsStringAsync();
        var response = JsonSerializer.Deserialize<BackupStateModel>(jsonRaw);

        if (response == null)
        {
         return  BackupState.Unknown;   
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
}
