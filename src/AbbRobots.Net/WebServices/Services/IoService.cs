using System.Text.Json;
using AbbRobots.Net.Models;
namespace AbbRobots.Net.WebServices.Services;
public class IoService
{


    private readonly HttpClient _httpClient;
    private readonly SubscriptionService _subscriptionService;

    public IoService(HttpClient client, SubscriptionService subscription)
    {
        _httpClient = client;
        _subscriptionService = subscription;
    }

    /// <summary>
    /// It retrieves all the robot's signals, already parsed and ready for use.
    /// </summary>    
    public async Task<List<RwsSignal>> GetSignalsAsync()
    {

        var SignalList = new List<RwsSignal>();
        string urlNextPage = "rw/iosystem/signals";

        while (!string.IsNullOrEmpty(urlNextPage))
        {
            using var responseMessage = await _httpClient.GetRwsAsync(urlNextPage);
            if (!responseMessage.IsSuccessStatusCode)
            {
                break;
            }
            string jsonRaw = await responseMessage.Content.ReadAsStringAsync();
            var response = JsonSerializer.Deserialize<RwsSignalResponse>(jsonRaw);
            if (response?.Embedded?.Resources != null)
            {
                SignalList.AddRange(response.Embedded.Resources);
            }

            string? nextHref = response?.Links?.Next?.HRef;
            if (!string.IsNullOrEmpty(nextHref))
            {
                urlNextPage = nextHref.StartsWith("/rw.iosystem") ? nextHref : $"rw/iosystem/{nextHref}";
            }
        }
        return SignalList;
    }

    /// <summary>
    /// Creates or modifies an I/O signal in the robot configuration database (live EIO.cfg)
    /// using the official ABB CFG Service endpoint.
    /// </summary>

    public async Task<bool> CreateSignalInConfigurationAsync(SignalItem signal)
    {
        string urlEndpoint = "rw/cfg/eio/signal?action=create";

        var formFields = new Dictionary<string, string>
        {
            {"name" , signal.Name}
        };

        if (!string.IsNullOrEmpty(signal.SignalType)) formFields.Add("type", signal.SignalType); // Ojo: a veces en API pide 'type' en vez de 'signal-type'
        if (!string.IsNullOrEmpty(signal.Device)) formFields.Add("device", signal.Device);
        if (!string.IsNullOrEmpty(signal.DeviceMap)) formFields.Add("devicemap", signal.DeviceMap);
        if (!string.IsNullOrEmpty(signal.Label)) formFields.Add("label", signal.Label);
        if (!string.IsNullOrEmpty(signal.Category)) formFields.Add("category", signal.Category);
        if (!string.IsNullOrEmpty(signal.Access)) formFields.Add("access", signal.Access);
        if (!string.IsNullOrEmpty(signal.DefaultValue)) formFields.Add("default", signal.DefaultValue);
        if (!string.IsNullOrEmpty(signal.Invert)) formFields.Add("invert", signal.Invert);

        var postContent = new FormUrlEncodedContent(formFields);

        var response = await _httpClient.PostAsync(urlEndpoint, postContent);

        return response.IsSuccessStatusCode;

    }

    /// <summary>
    /// Changes the logical value of a signal on the ABB robot (for example, activates or deactivates a DO).
    /// </summary>
    /// <param name="signalName">Exact name of the signal on the I/O map</param>
    /// <param name="value">The new value in text format (usually "1" or "0").</param>
    /// <returns>True if the robot successfully accepted the change, False otherwise.</returns>
    public async Task<bool> WriteSignalAsync(string signalName, string value)
    {
        // RWS requires the "set" action to be passed as a parameter in the URL query string.
        string url = $"rw/iosystem/signals/{signalName}?action=set";
        var fields = new Dictionary<string, string>
     {
         {"lvalue",value}
     };
        // Helper, which already configures the urlencoded headers and the POST method.
        using var response = await _httpClient.PostRwsFormAsync(url, fields);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    ///  Changes the logical value of a signal on the ABB robot (for example, activates or deactivates a DO).
    /// </summary>
    /// <param name="signalName">Exact name of the signal on the I/O map</param>
    /// <param name="active">The new value in format true or false</param>
    /// <returns>True if the robot successfully accepted the change, False otherwise.</returns>
    public async Task<bool> WriteSignalAsync(string signalName, bool active)
    {
        return await WriteSignalAsync(signalName, active ? "1" : "0");
    }

    /// <summary>
    /// Se suscribe a una señal y ejecuta la rutina proporcionada cada vez que cambie.
    /// </summary>
    /// <param name="signalName">Nombre de la señal.</param>
    /// <param name="onSignalUpdate">Rutina/Método del usuario donde recibirá los datos.</param>
    /// <returns>Un IDisposable para detener la recepción de notificaciones de esta rutina.</returns>
    public async Task<IDisposable> SubscribeToSignalAsync(

        string signalName,
        Action<SignalChangedEventArgs> onSignalUpdate,
        SubscriptionPriority priority = SubscriptionPriority.Medium)
    {
        return await _subscriptionService.SubscribeToSignalAsync(signalName, onSignalUpdate, priority);
    }
}