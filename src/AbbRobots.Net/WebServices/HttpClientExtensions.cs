using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AbbRobots.Net.WebServices;

public static class HttpClientExtensions
{
    /// <summary>
    /// Sends a POST request to the ABB robot using the x-www-form-urlencoded header format required by RWS.
    /// </summary>
    public static async Task<HttpResponseMessage> PostRwsFormAsync(this HttpClient httpClient, string url, Dictionary<string, string> postFields)
    {
       var content = new FormUrlEncodedContent(postFields);
        
     
        content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded")
        {
            Parameters = { new NameValueHeaderValue("v", "2.0") }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = content
        };
      
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

        
        return await httpClient.SendAsync(request);
    }

    /// <summary>
    /// Sends a GET request to the ABB robot, configuring the RWS-native accept headers.
    /// Returns the complete HttpResponseMessage so the caller can decide how to handle the stream.
    /// </summary>

    public static async Task<HttpResponseMessage> GetRwsAsync(this HttpClient httpClient, string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        // Le decimos al robot que aceptamos JSON (preferido en OmniCore/RWS modernos) o XML
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

        // También permitimos texto plano o cualquier flujo de datos (octets) por si descargamos 
        // un archivo de texto de un módulo RAPID (.mod) o una configuración (.cfg)
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        return await httpClient.SendAsync(request);
    }
}