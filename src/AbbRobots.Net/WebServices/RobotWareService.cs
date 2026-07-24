using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AbbRobots.Net.WebServices.Models;

namespace AbbRobots.Net.WebServices;

public class RobotWareService:IRobotWareService
{
    private readonly HttpClient _httpClient;

    public bool IsVirtualController { get; private set; }
    public RobotWareService(HttpClient client)
    {
        _httpClient = client;
    }

    public async Task<string> GetSystemInfoAsync()
    {
        var response = await _httpClient.GetRwsAsync("rw/system");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
    internal async Task InitializeAsync()
    {

        var responseMessage = await _httpClient.GetAsync("/rw/system/license");
        responseMessage.EnsureSuccessStatusCode();
        string jsonRaw = await responseMessage.Content.ReadAsStringAsync();

        var response = JsonSerializer.Deserialize<LicenseResponseModel>(jsonRaw);
        var licenseData = response?.State?[0];

        if (licenseData == null || string.IsNullOrEmpty(licenseData.LicenseType))
        {
          throw new InvalidOperationException("[RobotWareServices.InitializeAsync]Controllers returns no data");   
        }       
      
        IsVirtualController = licenseData.LicenseType.ToUpper() == "VIRTUAL_CONTROLLER";

    }
}