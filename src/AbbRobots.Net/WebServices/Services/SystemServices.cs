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
    private readonly HttpClient _client;

    public SystemService(HttpClient httpClient)
    {
        _client = httpClient;
    }

    ///<summary>
    /// Makes a reboot
    /// </summary>

public async Task<bool>RestartRobotAsync()
    {
        string url ="rw/panel/restart?action=restart";

        var postFields = new Dictionary<string,string>{{"restart-mode","restart"}};
        var response = await _client.PostAsync(url,new FormUrlEncodedContent(postFields));

        return response.IsSuccessStatusCode;

    }

}