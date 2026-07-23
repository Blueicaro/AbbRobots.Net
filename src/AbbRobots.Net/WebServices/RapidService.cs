using System.Net.Http.Json;
using System.Text.Json.Nodes;
using AbbRobots.Net.WebServices.Models;

namespace AbbRobots.Net.WebServices;

public class RapidServices : IRapidService
{
    private readonly HttpClient _httpClient;
    private const string baseIoResource = "/rw/rapid";
    public RapidServices(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public async Task<IReadOnlyList<RapidResource>> GetRapidResourcesAsync(CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.GetAsync(baseIoResource, cancellationToken);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonNode>(cancellationToken: cancellationToken);
        return ParseRapidResourcesResponse(json);
    }

    
    public Task<IReadOnlyList<TasksResource>> GetRapidTasksAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    private static IReadOnlyList<RapidResource> ParseRapidResourcesResponse(JsonNode? jsonNode)
    {
        var result = new List<RapidResource>();

        if (jsonNode == null) return result;

        var embeddedNode = jsonNode["_embedded"];
        if (embeddedNode == null) return result;

        var jsonArray = embeddedNode["resources"]?.AsArray()
                    ?? embeddedNode["_state"]?.AsArray();

        if (jsonArray == null) return result;

        foreach (var item in jsonArray)
        {
            if (item == null) continue;

            string? title = item["_title"]?.ToString();
            string? href = item["_links"]?["self"]?["href"]?.ToString();
            string? type = item["_type"]?.ToString();

            string name = item["name"]?.ToString() ?? title ?? href ?? string.Empty;

            if (!string.IsNullOrEmpty(name))
            {
                result.Add(new RapidResource(
                    Name: name,
                    Title: title,
                    Type: type,
                    Url: href
                ));
            }

        }
        return result;
    }

}
