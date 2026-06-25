using System.Net. Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AbbRobots.Net.WebServices.Services;

public class IoService
{
    private readonly HttpClient _client;

    public IoService(HttpClient client)
    {
        _client = client;
    }

    public async Task<string>GetSignalValueAsync(string signalName)
    {
        var response = await _client.GetAsync("$rs/iosystem/signals/{signalName}");
        response.EnsureSuccessStatusCode;
        return await response.Content.ReadAsStringAsync();
    }
}