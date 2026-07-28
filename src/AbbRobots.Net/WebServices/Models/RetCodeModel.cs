using System.Text.Json.Serialization;

namespace AbbRobots.Net.WebServices.Models;

public record RetCodeResource
(
    long Code,
    string? Name = null,
    string? Title = null,
    string? Severity = null,
    string? Description = null
);

internal record RetCodeModel(
    [property: JsonPropertyName("code")] long Code,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("severity")] string Severity,
    [property: JsonPropertyName("desc")] string Description
);

