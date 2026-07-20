using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;

namespace AbbRobots.Net.WebServices.Services;


/// /// <summary>
///  Service dedicated to global robot system operations (restarts, statuses, versions).
/// </summary>

public class SystemService
{
    private readonly HttpClient _httpClient;

    public SystemService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public object Diagnostics { get; internal set; }

    ///<summary>
    /// Makes a reboot
    /// </summary>

    public async Task<bool> RestartRobotAsync()
    {

        var postFields = new Dictionary<string, string> { { "restart-mode", "restart" } };
        var response = await _httpClient.PostRwsFormAsync("rw/panel/restart?action=restart", postFields);
        return response.IsSuccessStatusCode;

    }

}