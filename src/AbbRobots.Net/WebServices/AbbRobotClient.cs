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

    // Private fields that will save services once connected
    private RobotWareService? _robotWare;
    private IoService? _io;
    private MastershipService? _mastership;
    private SystemService? _system;
    private ControllerService? _controllerService;

    private SubscriptionService _subscriptionService;

    public RobotWareService RobotWare => _robotWare
        ?? throw new InvalidOperationException("RobotWare cannot be accessed because the connection has not been established. Call ConnectAsync() first.");

    public IoService Io => _io ?? throw new InvalidOperationException("IO cannot be accessed because the connection has not been established. Call ConnectAsync() first.");

    public MastershipService Mastership => _mastership ?? throw new InvalidOperationException("Mastership cannot be accessed because the connection has not been established. Call ConnectAsync() first.");

    public SystemService System => _system ?? throw new InvalidOperationException("System cannot be accessed because the connection has not been established. Call ConnectAsync() first.");

    public SubscriptionService Events => _subscriptionService ?? throw new InvalidOperationException("Events cannot be accessed because the connection has not been established. Call ConnectAsync() first.");

    public ControllerService Controller => _controllerService ?? throw new InvalidOperationException("Controller cannot be accessed because the connection has not been established. Call ConnectAsync() first.");

    private bool? _isVirtualController;

    public bool IsVirtualController => _isVirtualController ?? throw new InvalidOperationException("Cannot be accessed because the connection has not been established. Call ConnectAsync() first.");


    private readonly HttpClient _httpClient;
    private readonly CookieContainer _cookieContainer;

    private readonly string _robotIp;


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




    }

    public async Task ConnectAsync()
    {
        // 1. Lógica de handshake / autenticación si fuera necesaria

        // 2. Instanciamos un RobotWareService temporal para leer la licencia de forma segura
        var tempRobotWare = new RobotWareService(_httpClient);
        await tempRobotWare.InitializeAsync();

        // 3. Si la inicialización no ha fallado, guardamos la licencia y activamos TODOS los servicios
        _isVirtualController = tempRobotWare.IsVirtualController;

        _robotWare = tempRobotWare;
        _io = new IoService(_httpClient);
        _mastership = new MastershipService(_httpClient);
        _system = new SystemService(_httpClient);
        _controllerService = new ControllerService(_httpClient);

      
    }
}