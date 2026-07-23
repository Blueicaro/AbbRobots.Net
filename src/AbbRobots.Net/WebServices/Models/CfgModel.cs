namespace AbbRobots.Net.WebServices.Models;

/// <summary>
/// Representa un recurso o dominio de configuración (CFG) en la controladora ABB.
/// </summary>
public record CfgResource(
    string Name,
    string? Title = null,
    string? Type = null,
    string? Url = null
);