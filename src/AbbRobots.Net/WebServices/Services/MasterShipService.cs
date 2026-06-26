using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace AbbRobots.Net.WebServices.Services;

/// <summary>
/// Service dedicated to managing exclusive control (Mastership) over the different domains of the ABB robot.
/// </summary>

public class MastershipService
{
    private readonly HttpClient _client;

    public MastershipService(HttpClient httpClient)
    {
        _client = httpClient;
    }

    /// <summary>
    /// Requests exclusive control of a specific domain (e.g., "cfg", "motion", "rapid").
    /// </summary>

    public async Task<bool> ResquestAsync(string domain)
    {
        string url = $"rw/mastership/{domain}/request";

        var response = await _client.PostAsync(url, new FormUrlEncodedContent(new Dictionary<string, string>()));
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Releases exclusive control of a previously acquired domain.
    /// </summary>   
    public async Task<bool> ReleaseAsync(string domain)
    {
        string url = $"rw/mastership/{domain}/release";
        var response = await _client.PostAsync(url, new FormUrlEncodedContent(new Dictionary<string,string>()));
        return response.IsSuccessStatusCode;
    }

}