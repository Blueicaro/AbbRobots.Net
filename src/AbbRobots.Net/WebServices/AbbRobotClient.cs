using System;
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

    public  IoService Io { get; }

    public AbbRobotClient(string ipAddress, string username, string password, int? port = null)
    {
        string uriString = port.HasValue
            ? $"https://{ipAddress}:{port.Value}/"
            : $"https://{ipAddress}/";

        _cookieContainer = new CookieContainer();

        var handler = new HttpClientHandler
        {
            CookieContainer = _cookieContainer,
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };

        _httpClient = new HttpClient(handler) { BaseAddress = new Uri(uriString) };

        // Cabecera de Autenticación Básica (como GenerarClave en Pascal)
        var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

        // Cabecera Connection: Keep-Alive
        _httpClient.DefaultRequestHeaders.Connection.Add("Keep-Alive");

        // Cabeceras estrictas de ABB en Pascal (sin validación para que .NET las envíe sí o sí)
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/hal+json;v=2.0");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/hal+json;v=2.0");

        Io = new IoService(_httpClient);

    }

    /// <summary>
    /// Réplica exacta de 'PrimeraConexion' de Pascal. Hace un GET a la raíz con las cabeceras preparadas.
    /// </summary>
    public async Task<bool> ConnectAsync()
    {
        try
        {
            // FHttpSend.Get(FRobotUrl);
            var response = await _httpClient.GetAsync("");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}