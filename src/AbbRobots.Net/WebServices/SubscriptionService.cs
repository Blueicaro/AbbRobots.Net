using System.Text;
using System.Net.WebSockets;
using System.Net;
using AbbRobots.Net.WebServices.Models;
using System.Xml.Linq;


namespace AbbRobots.Net.WebServices;



public class SubscriptionService : IDisposable
{
    public event EventHandler<SignalChangedEventArgs>? OnSignalChanged;
    public event EventHandler<ControllerStateChangeEventArgs>? OnControllerStateChanged;
    public event Action<string, string>? OnNotificationReceived;
    public event EventHandler<BackupProgressEventArgs>? OnBackupProgressChanged;
    public event EventHandler<BackupStateChangeEventArgs>? OnBackupStateUpdate;
    private readonly HttpClient _httpClient;
    private readonly string _robotIp;
    private bool _disposed;
    private bool _isVirtualController;
    private CookieContainer _cookieContainer;
    private List<SubscriptionGroup> _activeGroups = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    public SubscriptionService(HttpClient httpClient, string robotIp, CookieContainer cookieContainer, bool isVirtualController)
    {
        _httpClient = httpClient;
        _robotIp = robotIp;
        _cookieContainer = cookieContainer;
        _isVirtualController = isVirtualController;
    }

    public void Dispose() => DisposeAsync().AsTask().GetAwaiter().GetResult();
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        await _semaphore.WaitAsync();
        List<SubscriptionGroup> groupsSnapshot;
        try { groupsSnapshot = _activeGroups.ToList(); }
        finally { _semaphore.Release(); }

        foreach (var group in groupsSnapshot)
            group.CancellationTokenSource?.Cancel();

        await Task.WhenAll(groupsSnapshot
            .Where(g => g.ListenerTask != null)
            .Select(g => g.ListenerTask!));

        foreach (var group in groupsSnapshot)
        {
            group.WebSocket?.Dispose();
            group.CancellationTokenSource?.Dispose();
        }

        _activeGroups.Clear();
        _semaphore.Dispose();
    }

    public async Task<IDisposable> SubscribeToSignalAsync(string signalName, Action<SignalChangedEventArgs> onSignalUpdate, SubscriptionPriority priority)
    {
        EventHandler<SignalChangedEventArgs> handler = (sender, args) =>
        {
            if (args.SignalName.Equals(signalName, StringComparison.OrdinalIgnoreCase))
            {
                onSignalUpdate(args);
            }
        };

        OnSignalChanged += handler;
        string resource = $"/rw/iosystem/signals/{signalName};state";
        await SubscribeToResourceInternalAsync(resource, priority);
        return new SubscriptionDisposer(() =>
        {
            OnSignalChanged -= handler;
        });
    }


    /// <summary>
    /// Subscribe al evento que genera los backups cuando se crean
    /// </summary>
    /// <returns></returns>
    public async Task<IDisposable> SubscribeToBackupAsync(string backupResource, Action<BackupStateChangeEventArgs> onBackupUpdate)
    {
        EventHandler<BackupStateChangeEventArgs> handler = (sender, args) => onBackupUpdate(args);
        OnBackupStateUpdate += handler;

        string resource = backupResource;
        await SubscribeToResourceInternalAsync(resource, SubscriptionPriority.low);
        return new SubscriptionDisposer(() => OnBackupStateUpdate -= handler);

    }

    public async Task<IDisposable> SubscribeToControllerStateAsync(
        Action<ControllerStateChangeEventArgs> onStateUpdate,
        SubscriptionPriority priority = SubscriptionPriority.Medium)
    {
        EventHandler<ControllerStateChangeEventArgs> handler = (sender, args) => onStateUpdate(args);
        OnControllerStateChanged += handler;

        string resource = "/rw/panel/ctrl-state";
        await SubscribeToResourceInternalAsync(resource, priority);

        return new SubscriptionDisposer(() =>
        {
            OnControllerStateChanged -= handler;
        });
    }


    internal async Task SubscribeToResourceInternalAsync(string resourceUri, SubscriptionPriority priority)
    {
        Console.WriteLine($"[DEBUG] Entrando a suscribir: {resourceUri}. Grupos activos actuales: {_activeGroups.Count}");
        await _semaphore.WaitAsync();
        try
        {
            int limitCapacity = priority == SubscriptionPriority.High ? 64 : 1000;
            var groupAvaliable = _activeGroups.FirstOrDefault(g => g.Priority == priority && (g.Resources.Count < limitCapacity));

            if (groupAvaliable != null)
            {
                await AddResourceToGroupAsync(groupAvaliable, resourceUri);
            }
            else
            {
                if (_activeGroups.Count >= 10)
                {
                    throw new InvalidOperationException("The limit of 10 subscription groups has been reached.");
                }

                Console.WriteLine($"[Creando subscription] resourceUri:{resourceUri}");
                await CreateNewSubscriptionGroupAsync(resourceUri, priority, limitCapacity);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }


    public async Task<IDisposable> SubscribeToBackupProgressAsync(
        string backupName,
        Action<BackupProgressEventArgs> onProgress,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(onProgress);


        EventHandler<BackupProgressEventArgs> handler = (sender, args) =>
        {
            if (args.BackupName.Equals(backupName, StringComparison.OrdinalIgnoreCase))
            {
                onProgress(args);
            }
        };


        OnBackupProgressChanged += handler;


        await SubscribeToResourceInternalAsync("/ctrl/backup", SubscriptionPriority.High);

        // Devolvemos el limpiador
        return new SubscriptionDisposer(() =>
        {
            OnBackupProgressChanged -= handler;
        });
    }

    /// <summary>
    /// Event router. Receive the XHTML from the WebSocket and dispatch the typed event.
    /// </summary>
    /// <param name="xmlMessage"></param>
    private void ParseAndEmitEvent(string xmlMessage)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(xmlMessage)) return;

            var doc = XDocument.Parse(xmlMessage);

            // NameSpace of XML
            XNamespace ns = "http://www.w3.org/1999/xhtml";

            // searching (ns + "li")
            var targetElement = doc.Descendants(ns + "li")
                .FirstOrDefault(e => e.Attribute("class")?.Value == "ios-signalstate-ev" ||
                                     e.Attribute("class")?.Value == "pnl-ctrlstate-ev");

            if (targetElement == null)
            {
                System.Diagnostics.Debug.WriteLine("[Parser] No supported event was found in the plot.");
                return;
            }

            string eventClass = targetElement.Attribute("class")?.Value ?? string.Empty;

            switch (eventClass)
            {
                case "ios-signalstate-ev":
                    ProcessSignalEvent(targetElement, ns);
                    break;
                case "pnl-ctrlstate-ev":
                    ProcessControllerStateEvent(targetElement, ns);
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine("[Parser] Event not supported");
                    break;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Parser] Invalid control message or frame: {ex.Message}");
        }
    }

    private void ProcessControllerStateEvent(XElement liElement, XNamespace ns)
    {
        // Buscamos el hijo <span> que tiene la clase 'ctrlstate'
        var stateSpan = liElement.Descendants(ns + "span")
                                 .FirstOrDefault(s => s.Attribute("class")?.Value == "ctrlstate");

        if (stateSpan != null)
        {
            string ctrlStateValue = stateSpan.Value; // "motoron" o "motoroff"

            // Disparamos el evento de tu SDK hacia la demo
            OnControllerStateChanged?.Invoke(this, new ControllerStateChangeEventArgs
            {
                CtrlState = ctrlStateValue,
                Mode = "Auto" // O el modo si lo extraes de otro nodo similar
            });
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("[Parser] Found pnl-ctrlstate-ev but missing inner ctrlstate span.");
        }
    }

    private void ProcessSignalEvent(XElement liElement, XNamespace ns)
    {
        var anchorElement = liElement.Descendants(ns + "a").FirstOrDefault();
        var valueSpan = liElement.Descendants(ns + "span")
                                 .FirstOrDefault(s => s.Attribute("class")?.Value == "lvalue");
        var stateSpan = liElement.Descendants(ns + "span")
                                 .FirstOrDefault(s => s.Attribute("class")?.Value == "lstate");

        if (anchorElement != null && valueSpan != null)
        {
            string href = anchorElement.Attribute("href")?.Value ?? string.Empty;

            // Getting the name
            string lastSegment = href.Split('/').LastOrDefault() ?? "UnknownSignal";

            // Cuttin ; to take the signal
            string signalName = lastSegment.Split(';').FirstOrDefault() ?? lastSegment;

            string signalValue = valueSpan.Value;

            // if blocked then is simulated
            bool isSimulated = false;
            if (stateSpan != null)
            {
                isSimulated = stateSpan.Value.Equals("blocked", StringComparison.OrdinalIgnoreCase);
            }

            // Event!
            OnSignalChanged?.Invoke(this, new SignalChangedEventArgs
            {
                SignalName = signalName,
                Value = signalValue,
                IsSimulated = isSimulated
            });
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("[Parser] Found ios-signalstate-ev but missing anchor or lvalue span.");
        }
    }


    private async Task AddResourceToGroupAsync(SubscriptionGroup group, string resourceUri)
    {
        string url = $"subscription/{group.GroupID}";

        int newIndice = group.Resources.Count + 1;
        var fields = new Dictionary<string, string>
      {
          {"resources", newIndice.ToString()},
          {newIndice.ToString(),resourceUri},
          {"priority",newIndice.ToString()},
          {$"{newIndice}-p",((int)group.Priority).ToString()}

      };

        using var response = await _httpClient.PutAsync(url, new FormUrlEncodedContent(fields));
        response.EnsureSuccessStatusCode();

        group.Resources.Add(resourceUri);
    }

    private async Task CreateNewSubscriptionGroupAsync(string resourceUri, SubscriptionPriority priority, int maxCapacity)
    {
        string url = "subscription";
        var fields = new Dictionary<string, string>
      {
          {"resources","1"},
          {"1",resourceUri},
          {"priority","1"},
          {"1-p",((int)priority).ToString()}
      };

        Console.WriteLine($"[CreateNewSubscriptionGroupAsync] url:{url}");

        using var response = await _httpClient.PostRwsFormAsync(url, fields);
        response.EnsureSuccessStatusCode();

        //(We extract the WebSocket URL from the "Location" header returned by the robot
        if (response.Headers.Location == null)
        {
            throw new InvalidOperationException("The ABB robot did not return the WebSocket (Location Header) address in the response.");
        }

        string webSocketUrl = response.Headers.Location.ToString();
        string groupId = webSocketUrl.Split('/').Last();

        var newGroup = new SubscriptionGroup
        {
            GroupID = groupId,
            WebSocketUrl = webSocketUrl,
            Priority = priority,
            MaxCapacity = maxCapacity
        };

        newGroup.Resources.Add(resourceUri);
        _activeGroups.Add(newGroup);
        await ConnectWebSocketAsync(newGroup);

    }

    private async Task ConnectWebSocketAsync(SubscriptionGroup group)
    {
        group.WebSocket ??= new ClientWebSocket();
        group.CancellationTokenSource = new CancellationTokenSource();

        group.WebSocket.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, SslPolicyErrors) =>
        {
            return true;
        };


        group.WebSocket.Options.Cookies = _cookieContainer;
        group.WebSocket.Options.AddSubProtocol("rws_subscription");

        Uri wsUri = new Uri(group.WebSocketUrl);
        Console.WriteLine($"[ConnectWebSocketAsync] wsUri:{wsUri}");
        await group.WebSocket.ConnectAsync(wsUri, group.CancellationTokenSource.Token);
        group.ListenerTask = Task.Run(() => ListenLoopAsync(group, group.CancellationTokenSource.Token), group.CancellationTokenSource.Token);
    }
    private async Task ListenLoopAsync(SubscriptionGroup group, CancellationToken token)
    {
        var buffer = new byte[1024 * 8];
        try
        {
            while (group.WebSocket != null && group.WebSocket.State == WebSocketState.Open && !token.IsCancellationRequested)
            {
                var result = await group.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }

                string messageRaw = Encoding.UTF8.GetString(buffer, 0, result.Count);


                OnNotificationReceived?.Invoke(group.GroupID, messageRaw);


                ParseAndEmitEvent(messageRaw);
            }
        }
        catch (OperationCanceledException)
        {
            // Do nothing
        }
        catch (Exception ex)
        {
            Console.Write($"[Grupo {group.GroupID}] Error de conexión WebSocket: {ex.Message}");
        }
    }
    internal class SubscriptionGroup
    {
        public string GroupID { get; set; } = string.Empty;
        public string WebSocketUrl { get; set; } = string.Empty;
        public SubscriptionPriority Priority { get; set; }
        public int MaxCapacity { get; set; }
        public List<string> Resources { get; set; } = new();
        public ClientWebSocket? WebSocket { get; set; }
        public CancellationTokenSource? CancellationTokenSource { get; set; }
        public Task? ListenerTask { get; set; }

    }

    /// <summary>
    /// Notifica a los suscriptores cuando se recibe una actualización de progreso de backup desde el WebSocket.
    /// </summary>
    internal void RaiseBackupProgressChanged(BackupProgressEventArgs args)
    {
        OnBackupProgressChanged?.Invoke(this, args);
    }
}
internal class SubscriptionDisposer : IDisposable
{
    private readonly Action _disposeAction;
    public SubscriptionDisposer(Action disposeAction) => _disposeAction = disposeAction;
    public void Dispose() => _disposeAction?.Invoke();
}