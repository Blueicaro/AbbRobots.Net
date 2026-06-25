using System.Net.Http;
using System.Threading.Tasks;

namespace AbbRobots.Net.WebServices.Services;

public class RobotWareServices
{
    private readonly HttpClient _client;
    public RobotWareServices(HttpClient client)
    {
        _client = client;
    }

    public async Task<string> GetSystemInfoAsync()
    {
        var response = await _client.GetAsync("rw/system");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStreamAsync();
    }
}