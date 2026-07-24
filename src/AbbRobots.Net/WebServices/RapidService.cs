using System.Net.Http.Json;
using System.Text.Json.Nodes;
using AbbRobots.Net.WebServices.Models;

namespace AbbRobots.Net.WebServices;

public class RapidService : IRapidService
{
    private readonly HttpClient _httpClient;
    private const string BaseRapidResource = "/rw/rapid";

    public RapidService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<RapidResource>> GetRapidResourcesAsync(CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.GetAsync(BaseRapidResource, cancellationToken);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonNode>(cancellationToken: cancellationToken);
        return ParseRapidResourcesResponse(json);
    }

    public async Task<IReadOnlyList<TasksResource>> GetRapidTasksAsync(CancellationToken cancellationToken = default)
    {
        string url = $"{BaseRapidResource}/tasks";
        HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonNode>(cancellationToken: cancellationToken);
        return ParseTaskResourceResponse(json);
    }

    public Task<bool> ValidateRapidVariable(string taskName, string rapidVariable, string dataType)
    {
        throw new NotImplementedException();
    }

    private static IReadOnlyList<TasksResource> ParseTaskResourceResponse(JsonNode? jsonNode)
    {
        var result = new List<TasksResource>();

        foreach (JsonNode item in jsonNode.GetEmbeddedResources())
        {
            string? href = item["_links"]?["self"]?["href"]?.ToString();
            string? type = item["type"]?.ToString();
            string name = item["name"]?.ToString() ?? href ?? string.Empty;
            string? taskState = item["taskstate"]?.ToString();
            string? excState = item["excstate"]?.ToString();
            string? active = item["active"]?.ToString();
            string? motionTask = item["motiontask"]?.ToString();

            if (!string.IsNullOrEmpty(name))
            {
                result.Add(new TasksResource(
                    Name: name,
                    Type: type,
                    TaskState: taskState,
                    Excstate: excState,
                    Active: active,
                    MotionTask: motionTask,
                    Url: href
                ));
            }
        }

        return result;
    }

    private static IReadOnlyList<RapidResource> ParseRapidResourcesResponse(JsonNode? jsonNode)
    {
        var result = new List<RapidResource>();

        foreach (JsonNode item in jsonNode.GetEmbeddedResources())
        {
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