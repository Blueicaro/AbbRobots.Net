using System;
using System.Dynamic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AbbRobots.Net.WebServices.Services;

namespace AbbRobots.Net.WebServices;

public class AbbRobotClient
{
    private readonly HttpClient _httpClient;
    private readonly CookieContainer _cookieContainer;

    private readonly string _robotIp;

    public  IoService Io { get; }

    public MastershipService Mastership{get; }

    public SystemService System{get;}

    public SubscriptionService Events{get;}

    public AbbRobotClient(string ipAddress, string username, string password, int? port = null)
    {
        string uriString = port.HasValue
            ? $"https://{ipAddress}:{port.Value}/"
            : $"https://{ipAddress}/";

            _robotIp = ipAddress;

        _cookieContainer = new CookieContainer();

        var handler = new HttpClientHandler
        {
            CookieContainer = _cookieContainer,
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };

        _httpClient = new HttpClient(handler) { BaseAddress = new Uri(uriString) };

        // Basic Authentication Header 
        var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

        // Connection Header: Keep-Alive
        _httpClient.DefaultRequestHeaders.Connection.Add("Keep-Alive");

        // Strict ABB headers (without validation, ensuring .NET sends them regardless)
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/hal+json;v=2.0");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/hal+json;v=2.0");

        Io = new IoService(_httpClient);
        Mastership = new MastershipService(_httpClient);
        System = new SystemService(_httpClient);

        Events = new SubscriptionService(_httpClient,_robotIp,_cookieContainer);

    }

    public async Task<bool> ConnectAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}