using AbbRobots.Net.WebServices.Models;
namespace AbbRobots.Net.WebServices;

public interface IRetCodeService
{
    /// <summary>
    /// Get all defined RobotWare return codes.  
    /// </summary>   
    /// <returns>A list of RetCodeResource</returns>
    public Task <IReadOnlyList<RetCodeResource>> GetRetCodesServiceAsync(long code,CancellationToken cancellationToken=default);
}