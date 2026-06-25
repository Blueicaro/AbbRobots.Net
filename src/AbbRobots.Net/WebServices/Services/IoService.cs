using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using AbbRobots.Net.Models;

namespace AbbRobots.Net.WebServices.Services;

public class IoService
{
    private readonly HttpClient _client;

    public IoService(HttpClient client)
    {
        _client = client;
    }

    /// <summary>
    /// It retrieves all the robot's signals, already parsed and ready for use.
    /// </summary>    
   public async Task<List<RwsSignal>> GetSignalsAsync()
    {
        // 1. Petición HTTP nativa usando la cookie ya guardada
        var response = await _client.GetAsync("rw/iosystem/signals");
        response.EnsureSuccessStatusCode();

        // 2. Leer el string JSON
        string jsonCrudo = await response.Content.ReadAsStringAsync();

        // 3. Deserializar automáticamente con la potencia de .NET
        var resultado = JsonSerializer.Deserialize<RwsSignalResponse>(jsonCrudo);

        return resultado?.Embedded?.Resources ?? new List<RwsSignal>();
    }
}