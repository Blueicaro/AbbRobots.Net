using System.Net.Http.Json;
using System.Text.Json.Nodes;
using AbbRobots.Net.WebServices.Models;

namespace AbbRobots.Net.WebServices;

public class RapidService : IRapidService
{
    private readonly HttpClient _httpClient;
    private readonly FileService _fileService;
    private const string BaseRapidResource = "/rw/rapid";

    public RapidService(HttpClient httpClient, FileService fileService)
    {
        _httpClient = httpClient;
        _fileService = fileService;
    }

    public async Task<IReadOnlyList<ModuleResource>> GetRapidModules(string taskName, CancellationToken cancellationToken = default)
    {
        string url = $"{BaseRapidResource}/{taskName}/modules";

        HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonNode>(cancellationToken: cancellationToken);

        return ParseModuleResourceResponse(json);

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

    public async Task<bool> ValidateRapidVariable(string taskName, string rapidVariable, string dataType)
    {
        string url = $"{BaseRapidResource}/symbols/validate";

        var fields = new Dictionary<string, string> {
            { "task", taskName },
            {"value",rapidVariable},
            {"datatype",dataType}
            };

        HttpResponseMessage response = await _httpClient.PostRwsFormAsync(url, fields);

        return response.IsSuccessStatusCode;
    }

    public async Task<List<string>> GetModuleText(string taskName, string moduleName,CancellationToken cancellationToken=default)
    {
        string url =$"{BaseRapidResource}/{taskName}/{moduleName}\text";

        HttpResponseMessage response = await _httpClient.GetAsync(url,cancellationToken);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonNode>(cancellationToken: cancellationToken);

        TextModuleResource? textModuleResource = ParseTextModuleResource(json);

        var content = new List<string>();

        if (textModuleResource==null) return content;

        if (!string.IsNullOrEmpty(textModuleResource.FilePath))
        {
                 return (List<string>)await _fileService.GetFileContent(textModuleResource.FilePath);
        }


        if (!string.IsNullOrEmpty(textModuleResource.ModuleText))
        {
             content.Add(textModuleResource.ModuleText);
             return content;
        }

        return content;
    }


    private static TextModuleResource? ParseTextModuleResource(JsonNode? jsonNode)
    {
        
        if (jsonNode == null ) return null;       
      

        var jsonArray = jsonNode["resources"]?.AsArray() ?? jsonNode["_state"]?.AsArray();

        if (jsonArray==null) return null;


        
        foreach(var item in jsonArray)
        {
            if (item == null) continue;

            string? changeCount = item["change-count"]?.ToString();
            string? filePath = item["file-path"]?.ToString();
            string? moduleLenght = item["module-length"]?.ToString();
            string? moduleText = item["module-text"]?.ToString(); 
            string? title = item["_title"]?.ToString();

            if (!string.IsNullOrEmpty(title))
            {
             return new TextModuleResource(
                Title : title,
                ChangeCount : changeCount,
                ModuleText:moduleText,
                ModuleLength:moduleLenght,
                FilePath: filePath
             );                
            } 
        }
      return null;  
    }



    private static IReadOnlyList<ModuleResource> ParseModuleResourceResponse(JsonNode? jsonNode)
    {

        var result = new List<ModuleResource>();
        if (jsonNode == null) return result;

        var jsonArray = jsonNode["resources"]?.AsArray()
                     ?? jsonNode["_state"]?.AsArray();

        if (jsonArray == null) return result;

        foreach (var item in jsonArray)
        {
            if (item == null) continue;

            string? title = item["_title"]?.ToString();
            string? name = item["name"]?.ToString();
            string? type = item["type"]?.ToString();
            if (!string.IsNullOrEmpty(name))
            {
                result.Add(new ModuleResource(
                    Name: name,
                    Title: title,
                    Type: type
                ));
            }

        }
        return result.AsReadOnly();
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