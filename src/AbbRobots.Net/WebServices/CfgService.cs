
using System.ComponentModel.Design.Serialization;
using System.IO.Pipelines;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using AbbRobots.Net.WebServices.Models;

namespace AbbRobots.Net.WebServices;


public class CfgService : ICfgService
{
    private readonly HttpClient _httpClient;

    private const string BaseCfgResource = "/rw/cfg";

    public CfgService(HttpClient client)
    {
        _httpClient = client;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CfgResource>> GetCfgResourcesAsync(string? domain = null, CancellationToken cancellationToken = default)
    {
        string requestUri = string.IsNullOrWhiteSpace(domain) ? BaseCfgResource : $"{BaseCfgResource}/{domain.Trim().ToLowerInvariant()}";

        HttpResponseMessage response = await _httpClient.GetAsync(requestUri, cancellationToken);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonNode>(cancellationToken: cancellationToken);

        return ParseCfgResourcesFromResponse(json);

    }

private static IReadOnlyList<CfgResource> ParseCfgResourcesFromResponse(JsonNode? jsonNode)
    {
        var result = new List<CfgResource>();

        if (jsonNode == null)
            return result;

        var embeddedNode = jsonNode["_embedded"];
        if (embeddedNode == null)
            return result;

        // Evaluamos 'resources' (utilizado en /rw/cfg) y '_state' (utilizado en sub-dominios)
        var jsonArray = embeddedNode["resources"]?.AsArray() 
                     ?? embeddedNode["_state"]?.AsArray();

        if (jsonArray == null)
            return result;

        foreach (var item in jsonArray)
        {
            if (item == null) continue;

            string? title = item["_title"]?.ToString();
            string? href = item["_links"]?["self"]?["href"]?.ToString();
            string? type = item["_type"]?.ToString();
            
            // Determinar el nombre con fallback: 'name' -> '_title' -> 'href'
            string name = item["name"]?.ToString() 
                       ?? title 
                       ?? href 
                       ?? string.Empty;

            if (!string.IsNullOrEmpty(name))
            {
                result.Add(new CfgResource(
                    Name: name,
                    Title: title,
                    Type: type,
                    Url: href
                ));
            }
        }

        return result.AsReadOnly();
    }
}