
using System.Net.Http.Json;
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

    public async Task<bool> ValidateRapidVariableAsync(string taskName, string rapidVariable, string dataType)
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


    public async Task<IReadOnlyList<ModulesResource>> GetRapidModulesAsync(string taskName, CancellationToken cancellationToken = default)
    {
         //https://localhost:80/rw/rapid/tasks/T_ROB1/modules
        string url = $"{BaseRapidResource}/tasks/{taskName}/modules";

        HttpResponseMessage response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var responseModel = await response.Content.ReadFromJsonAsync<ModulesResponseModel>(cancellationToken:cancellationToken);

        var items = responseModel?.Items ??[];

        return items
        .Where (i=> !string.IsNullOrEmpty(i.ResolvedName))
        .Select(i => new ModulesResource(
           Name:i.ResolvedName,
           Type: i.Type,
           Title: i.Title
        )).ToList().AsReadOnly()??[];

    }

    public async Task<IReadOnlyList<string>> GetModuleTextAsync(string taskName, string moduleName, CancellationToken cancellationToken = default)
    {
        // https://localhost:80/rw/rapid/tasks/T_ROB1/modules/MainModule/text
        string url =$"{BaseRapidResource}/tasks/{taskName}/modules/{moduleName}/text";

        HttpResponseMessage response = await _httpClient.GetAsync(url,cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseModel = await response.Content.ReadFromJsonAsync<ModuleResource>(cancellationToken:cancellationToken);

        

        string rawString= string.Empty;
  

        if (!string.IsNullOrEmpty(responseModel?.ModuleText))
        {
            rawString = responseModel.ModuleText;
        }else if (!string.IsNullOrEmpty(responseModel?.FilePath))
        {
            response = await _httpClient.GetAsync(responseModel.FilePath,cancellationToken);
            response.EnsureSuccessStatusCode();
            rawString = await response.Content.ReadAsStringAsync(cancellationToken);
        }else
        {
            return[];
        }

        return rawString.Split(["\r\n","\n"], StringSplitOptions.None);
    }
}