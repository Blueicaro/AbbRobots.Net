using AbbRobots.Net.WebServices.Models;

namespace AbbRobots.Net.WebServices;

public interface ICfgService
{
    /// <summary>
    /// Gets the list of available configuration domains (CFG) resources.
    /// </summary>
    /// <param name="domain">
    /// Optional domain to consult (e.g. "eio", "moc", "sys"). 
    /// If it is null or empty, return the list of root domains.
    /// </param>
    /// <param name="cancellationToken">Cancelling token.</param>
    /// <returns>Collection of configuration resources found.</returns>
    Task<IReadOnlyList<CfgResource>> GetCfgResourcesAsync(string? domain = null, CancellationToken cancellationToken=default);
}