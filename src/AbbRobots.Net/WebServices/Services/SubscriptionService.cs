using System;
using System.Text;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AbbRobots.Net.WebServices;
using System.Net.WebSockets;
using System.Net;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using AbbRobots.Net.Models;
using System.Xml.Linq;
using System.Diagnostics;
using System.Xml;

namespace AbbRobots.Net.WebServices.Services;



public class SubscriptionService : IDisposable
{

    private readonly HttpClient _httpClient;
    private readonly string _robotIp;
    private CookieContainer _cookieContainer;
    private List<SubscriptionGroup> _activeGroups = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    public event Action<string, string>? OnNotificationReceived;
    public EventHandler<SignalChangedEventArgs>? OnSignalChanged;
    public EventHandler<ControllerStateChangeEventArgs>? OnControllerStateChanged;
    public SubscriptionService(HttpClient httpClient, string robotIp, CookieContainer cookieContainer)
    {
        _httpClient = httpClient;
        _robotIp = robotIp;
        _cookieContainer = cookieContainer;
    }

    public enum SubscriptionPriority
    {
        low = 0,
        Medium = 1,
        High = 2
    }

    /// <summary>
    /// Event router. Receive the XHTML from the WebSocket and dispatch the typed event.
    /// </summary>
    /// <param name="xmlMessage"></param>
    private void ParseAndEmitEvent(string xmlMessage)
    {
        try
        {
            var element = XElement.Parse(xmlMessage);
            string eventClass = element.Attribute("class")?.Value ?? string.Empty;

            switch (eventClass)
            {
                case "ios-signalstate-ev":
                    ProcessSignalEvent(element);
                    break;
                case "pnl-ctrlstate-ev":
                    ProcessControllerStateEvent(element);
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine($"[Parser] Event not supported");
                    break;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Parser] Invalid control message or frame: {ex.Message}");
        }
    }


    private void ProcessSignalEvent(XElement element)
    {
        string href = element.Element("a")?.Attribute("href")?.Value ?? string.Empty;
        string signalName = ExtractLastSegment(href);

        if (string.IsNullOrEmpty(signalName)) return;

        // Buscamos los valores dentro de las etiquetas span filtrando por su clase
        string valor = element.Elements("span")
            .FirstOrDefault(e => e.Attribute("class")?.Value == "lvalue")?.Value ?? "0";

        string estado = element.Elements("span")
            .FirstOrDefault(e => e.Attribute("class")?.Value == "lstate")?.Value ?? string.Empty;

        bool esSimulada = estado.Equals("simulated", StringComparison.OrdinalIgnoreCase);

        OnSignalChanged?.Invoke(this, new SignalChangedEventArgs
        {
            SignalName = signalName,
            Value = valor,
            IsSimulated = esSimulada
        });
    }
    private void ProcessControllerStateEvent(XElement elemento)
    {
        string estadoCtrl = elemento.Elements("span")
            .FirstOrDefault(e => e.Attribute("class")?.Value == "ctrlstate")?.Value ?? string.Empty;

        string modo = elemento.Elements("span")
            .FirstOrDefault(e => e.Attribute("class")?.Value == "ctrlmode")?.Value ?? string.Empty;

        OnControllerStateChanged?.Invoke(this, new ControllerStateChangeEventArgs
        {
            CtrlState = estadoCtrl,
            Mode = modo
        });
    }

    private string ExtractLastSegment(string href)
    {
        if (string.IsNullOrEmpty(href)) return string.Empty;
        // Limpiamos los parámetros de la URL tipo ';state' si vienen
        string path = href.Split(';')[0];
        return path.Split('/').Last();
    }
}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="signalName">String contains signal</param>
    /// <param name="subscriptionPriority">set priority. </param>
    public async Task SubscribeToSignalAsync(string signalName, SubscriptionPriority priority = SubscriptionPriority.Medium)
    {
        string resource = $"/rw/iosystem/signals/{signalName};state";
        await SubscribeToResourceInternalAsync(resource, priority);
    }

    public async Task SubscribeToControlStateAsync(SubscriptionPriority priority = SubscriptionPriority.Medium)
    {
        string resource = "/rw/panel/ctrlstate:state";
        await SubscribeToResourceInternalAsync(resource, priority);
    }

    private async Task SubscribeToResourceInternalAsync(string resourceUri, SubscriptionPriority priority)
    {
        //To avoid multiple call at same time
        await _semaphore.WaitAsync();
        try
        {
            int limitCapacity = priority == SubscriptionPriority.High ? 64 : 1000;
            var groupAvaliable = _activeGroups.FirstOrDefault(g => g.Priority == priority && (g.Resources.Count < limitCapacity));
            if (groupAvaliable != null)
            {
                // Add resource to group
                await AddResourceToGroupAsync(groupAvaliable, resourceUri);
            }
            else
            {
                if (_activeGroups.Count >= 10)
                {
                    throw new InvalidOperationException("The limit of 10 subscription groups has been reached.");
                }
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task AddResourceToGroupAsync(SubscriptionGroup group, string resourceUri)
    {
        string url = $"Rw/subscription/{group.GroupID}";

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
        string url = "rw/subscription";
        var fields = new Dictionary<string, string>
      {
          {"resources","1"},
          {"1",resourceUri},
          {"prority","1"},
          {"1-p",((int)priority).ToString()}
      };

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
        group.WebSocket = new ClientWebSocket();
        group.CancellationTokenSource = new CancellationTokenSource();

        group.WebSocket.Options.Cookies = _cookieContainer;
        group.WebSocket.Options.AddSubProtocol("rws_subscription");

        Uri wsUri = new Uri(group.WebSocketUrl);
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
            }
        }
        catch (OperationCanceledException)
        {

            //Do nothing
        }
        catch (Exception ex)
        {
            Console.Write($"[Grupo {group.GroupID}] Error de conexión WebSocket: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _semaphore.Dispose();
        foreach (var group in _activeGroups)
        {
            group.CancellationTokenSource?.Cancel();
            group.WebSocket?.Dispose();
            group.CancellationTokenSource?.Dispose();
        }
        _activeGroups.Clear();
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
}