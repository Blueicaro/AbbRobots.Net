
using System.Text.Json.Serialization;

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

internal record CfgResponseModel
{
    [JsonPropertyName("_embedded")]
    public CfgEmbeddedModel? Embedded { get; init; }
}

internal record CfgEmbeddedModel
{
    [JsonPropertyName("resources")]
    public List<CfgResourceModel>? Resources { get; init; }

    [JsonPropertyName("_state")]
    public List<CfgResourceModel>? State { get; init; }

    // Devuelve el primero que no sea null (resources o _state)
    public List<CfgResourceModel> Items => Resources ?? State ?? [];
}

internal record CfgResourceModel
{
    [JsonPropertyName("_links")]
    public CfgLinksModel? Links { get; init; }

    [JsonPropertyName("_type")]
    public string? Type { get; init; }

    [JsonPropertyName("_title")]
    public string? Title { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    // Fallback: name -> _title -> href
    public string ResolvedName => Name ?? Title ?? Links?.Self?.Href ?? string.Empty;
}

internal record CfgLinksModel
{
    [JsonPropertyName("self")]
    public CfgLinkModel? Self { get; init; }
}

internal record CfgLinkModel
{
    [JsonPropertyName("href")]
    public string? Href { get; init; }
}


