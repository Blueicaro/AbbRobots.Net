using AbbRobots.Net.WebServices.Models;
namespace AbbRobots.Net.WebServices;

public interface IRapidService
{
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
    Task<IReadOnlyList<TasksResource>> GetRapidTasksAsync(CancellationToken cancellationToken=default);
}