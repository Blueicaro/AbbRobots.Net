using AbbRobots.Net.WebServices.Models;
namespace AbbRobots.Net.WebServices;

public interface IRapidService
{

    Task<ModulesResourceModel> GetRapidModules(string taskName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Description : Returns a list of rapid resource.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>RapidResource</returns>
    Task<IReadOnlyList<RapidResource>> GetRapidResourcesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a list of all rapid tasks.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>TasksResource</returns>
    Task<IReadOnlyList<TasksResource>> GetRapidTasksAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate rapid variable.
    /// </summary>
    /// <param name="taskName">String that contantains the name of the taks. Ej: T_ROB1</param>
    /// <param name="rapidVariable">String that contantains the rapid variable. Ej: [TRUE,[[0,0,0],[-1,0,0,0]],[1,[0,0,-1],[1,0,0,0],0,0,0]]</param>
    /// <param name="dataType">String with the name of data type. Ej: tooldata</param>
    /// <returns>true if succefull</returns>
    Task<bool> ValidateRapidVariable(string taskName, string rapidVariable, string dataType);

    /// <summary>
    ///   Get a module text.
    /// </summary>
    /// <param name="taskName">Name of the task where belongs the module</param>
    /// <param name="moduleName">Name of the module</param>
    /// <returns>A List of string </returns>
    Task<List<string>> GetModuleText(string taskName, string moduleName,CancellationToken cancellationToken=default);
}