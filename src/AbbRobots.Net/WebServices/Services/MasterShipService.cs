using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace AbbRobots.Net.WebServices.Services;

/// <summary>
/// Service dedicated to managing exclusive control (Mastership) over the different domains of the ABB robot.
/// </summary>

public class MastershipService
{
    private readonly HttpClient _httpClient;

    public MastershipService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Requests exclusive control of a specific domain (e.g., "cfg", "motion", "rapid").
    /// </summary>

    public async Task<bool> RequestAsync(string domain)
    {
       string url = $"rw/mastership/{domain}/request";

        // 1. Creamos el contenido del formulario vacío
        var contenido = new FormUrlEncodedContent(new Dictionary<string, string>());

        // 2. Limpiamos y configuramos las cabeceras específicas de esta petición POST
        var peticion = new HttpRequestMessage(HttpMethod.Post, url);
        peticion.Content = contenido;
        
        // Exigimos al robot que acepte el formato urlencoded de respuesta
        peticion.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));

        // 3. Enviamos el mensaje completo con sus cabeceras
        var respuesta = await _httpClient.SendAsync(peticion);
        return respuesta.IsSuccessStatusCode;
    }

    /// <summary>
    /// Releases exclusive control of a previously acquired domain.
    /// </summary>   
    public async Task<bool> ReleaseAsync(string domain)
    {
        string url = $"rw/mastership/{domain}/release";
        var response = await _httpClient.PostAsync(url, new FormUrlEncodedContent(new Dictionary<string,string>()));
        return response.IsSuccessStatusCode;
    }

}