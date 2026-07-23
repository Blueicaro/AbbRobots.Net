

namespace AbbRobots.Net.WebServices;


/// /// <summary>
///  Service dedicated to global robot system operations (restarts, statuses, versions).
/// </summary>

public class SystemService : ISystemService
{
    private readonly HttpClient _httpClient;

    public SystemService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    #region Public API
    /// <inheritdoc/>
    public async Task<bool> RestartRobotAsync()
    {

        var postFields = new Dictionary<string, string> { { "restart-mode", "restart" } };
        var response = await _httpClient.PostRwsFormAsync("rw/panel/restart?action=restart", postFields);
        return response.IsSuccessStatusCode;

    }
    #endregion

}