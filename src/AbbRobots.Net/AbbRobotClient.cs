using System.Net;
using System.Net.Http.Headers;
using System.Text;
using AbbRobots.Net.WebServices;


namespace AbbRobots.Net;

public class AbbRobotClient : IAsyncDisposable, IDisposable
{
    // Private fields that will save services once connected


    public RobotWareService RobotWare => _robotWare
        ?? throw new InvalidOperationException("RobotWare cannot be accessed because the connection has not been established. Call ConnectAsync() first.");

    public IoService Io => _io ?? throw new InvalidOperationException("IO cannot be accessed because the connection has not been established. Call ConnectAsync() first.");

    public MastershipService Mastership => _mastership ?? throw new InvalidOperationException("Mastership cannot be accessed because the connection has not been established. Call ConnectAsync() first.");

    public SystemService System => _system ?? throw new InvalidOperationException("System cannot be accessed because the connection has not been established. Call ConnectAsync() first.");

    public ControllerService Controller => _controllerService ?? throw new InvalidOperationException("Controller cannot be accessed because the connection has not been established. Call ConnectAsync() first.");

    /// <summary>
    /// Servicio para consultar y gestionar la configuración (CFG) de la controladora.
    /// </summary>
    public ICfgService Cfg => _cfgService
        ?? throw new InvalidOperationException("Cfg cannot be accessed because the connection has not been established. Call ConnectAsync() first.");
    private bool? _isVirtualController;

    public bool IsVirtualController => _isVirtualController ?? throw new InvalidOperationException("Cannot be accessed because the connection has not been established. Call ConnectAsync() first.");

    private readonly HttpClient _httpClient;
    private readonly CookieContainer _cookieContainer;
    private readonly string _robotIp;

    private RobotWareService? _robotWare;
    private IoService? _io;
    private MastershipService? _mastership;
    private SystemService? _system;
    private ControllerService? _controllerService;
    private SubscriptionService? _subscriptionService;
    private CfgService? _cfgService;

    private bool _disposed;

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

        var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
        _httpClient.DefaultRequestHeaders.Connection.Add("Keep-Alive");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/hal+json;v=2.0");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/hal+json;v=2.0");
    }

    public async Task ConnectAsync()
    {
        var tempRobotWare = new RobotWareService(_httpClient);
        await tempRobotWare.InitializeAsync();

        _isVirtualController = tempRobotWare.IsVirtualController;
        _robotWare = tempRobotWare;
      

        _subscriptionService = new SubscriptionService(_httpClient, _robotIp, _cookieContainer, IsVirtualController);
        _io = new IoService(_httpClient, _subscriptionService);
        _mastership = new MastershipService(_httpClient);
        _system = new SystemService(_httpClient);
        _controllerService = new ControllerService(_httpClient, _subscriptionService);
        _cfgService = new CfgService (_httpClient);
        _robotWare =  new RobotWareService(_httpClient);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        if (_subscriptionService != null)
        {
            await _subscriptionService.DisposeAsync();
        }

        _httpClient.Dispose();

        GC.SuppressFinalize(this);
    }

    public void Dispose() => DisposeAsync().AsTask().GetAwaiter().GetResult();
}