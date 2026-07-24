using AbbRobots.Net.WebServices.Models;

namespace AbbRobots.Net.WebServices;

public interface IMasterShipService
{
    /// <summary>
    /// Requests exclusive control of a specific domain (e.g., "cfg", "motion", "rapid").
    /// </summary>
    Task<bool> RequestAsync(string domain = "");

    /// <summary>
    /// Releases exclusive control of a previously acquired domain.
    /// </summary> 
    Task<bool> ReleaseAsync(string domain = "");
}
