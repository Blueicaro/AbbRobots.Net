
using System.Net.Http.Json;
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
    public async Task<ModulesResource> GetRapidModules(string taskName, CancellationToken cancellationToken = default)
    {
        string url = $"{BaseRapidResource}/{taskName}/modules";

        HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseModel = await response.Content.ReadFromJsonAsync<ModulesResponseModel>(cancellationToken:cancellationToken);
        
        
        return responseModel?.Embebbed?.Items
        .Where(i => !string.IsNullOrEmpty(i.ResolvedName))
        .Select(i => new ModulesResource(
            Name: i.Name          
        ))
        .ToList().AsReadOnly() ??[]

        throw new NotImplementedException();
    }

    // public async Task<ModulesResourcel> GetRapidModules(string taskName, CancellationToken cancellationToken = default)
    // {
    //     string url = $"{BaseRapidResource}/{taskName}/modules";

    //     HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);

    //     response.EnsureSuccessStatusCode();

    //     var responseModel = await response.Content.ReadFromJsonAsync<ModulesResponseModel>(cancellationToken:cancellationToken);

    //     return responseModel?.Embebbed?.Items
    //     .Where (I => !string.IsNullOrEmpty(I.ResolvedName))
    //     .Select(i=> new ModuleResource(
    //         Name
    //     ))


    // }


    public async Task<IReadOnlyList<TasksResource>> GetRapidTasksAsync(CancellationToken cancellationToken = default)
    {
        string url = $"{BaseRapidResource}/tasks";
        HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseModel = await response.Content.ReadFromJsonAsync<TasksResponseModel>(cancellationToken: cancellationToken);

        return responseModel?.Embedded?.Items
        .Where(i => !string.IsNullOrEmpty(i.ResolvedName))
        .Select(i => new TasksResource(
            Name: i.ResolvedName,
            Type: i.Type,
            Title: i.Title,
            TaskState: i.TaskState,
            Excstate: i.ExcState,
            Active: i.Active,
            MotionTask: i.MotionTask,
            Url: i.Links?.Self?.Href))
            .ToList().AsReadOnly() ?? [];

    }

    // public async Task<IReadOnlyList<RapidResource>> GetRapidResourcesAsync(CancellationToken cancellationToken = default)
    // {
    //     HttpResponseMessage response = await _httpClient.GetAsync(BaseRapidResource, cancellationToken);
    //     response.EnsureSuccessStatusCode();
    //     var json = await response.Content.ReadFromJsonAsync<JsonNode>(cancellationToken: cancellationToken);
    //     return ParseRapidResourcesResponse(json);
    // }

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



    public Task<IReadOnlyList<RapidResource>> GetRapidResourcesAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<TasksResource>> GetRapidTasksAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<string>> GetModuleText(string taskName, string moduleName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}