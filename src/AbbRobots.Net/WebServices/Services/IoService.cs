using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using AbbRobots.Net.Models;

namespace AbbRobots.Net.WebServices.Services;

public class IoService
{
    private readonly HttpClient _httpClient;

    public IoService(HttpClient client)
    {
        _httpClient = client;
    }

    /// <summary>
    /// It retrieves all the robot's signals, already parsed and ready for use.
    /// </summary>    
    public async Task<List<RwsSignal>> GetSignalsAsync()
    {
        // 1. Petición HTTP nativa usando la cookie ya guardada
        var response = await _httpClient.GetAsync("rw/iosystem/signals");
        response.EnsureSuccessStatusCode();

        // 2. Leer el string JSON
        string jsonCrudo = await response.Content.ReadAsStringAsync();

        // 3. Deserializar automáticamente con la potencia de .NET
        var resultado = JsonSerializer.Deserialize<RwsSignalResponse>(jsonCrudo);

        return resultado?.Embedded?.Resources ?? new List<RwsSignal>();
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
}