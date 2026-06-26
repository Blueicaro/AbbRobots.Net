using System.Net.Http;
using System.Threading.Tasks;

namespace AbbRobots.Net.WebServices.Services;

public class RobotWareServices
{
    private readonly HttpClient _httpClient;
    public RobotWareServices(HttpClient client)
    {
        _httpClient = client;
    }

    public async Task<string> GetSystemInfoAsync()
    {
        var response = await _httpClient.GetAsync("rw/system");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}