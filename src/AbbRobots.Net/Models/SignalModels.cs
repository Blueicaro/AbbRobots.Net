using System.Text.Json.Serialization;

namespace AbbRobots.Net.Models;

/// <Summary>
/// It represents the root of the JSON returned by the robot.
/// 
public class RwsSignalResponse
{
    [JsonPropertyName("_embedded")]
    public EmbeddedSignals Embedded{get; set;} = new();
}

public class EmbeddedSignals
{
    [JsonPropertyName("resources")]
    public List<RwsSignal> Resources { get; set; } = new();
}

/// <summary>
/// Represents each individual signal from the ABB I/O map./// 
/// </summary>
public class RwsSignal
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty; // DI, DO, GI, GO

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty; 

    [JsonPropertyName("lvalue")]
    public string LogicalValue { get; set; } = string.Empty; // "0" o "1"

    [JsonPropertyName("lstate")]
    public string LogicalState { get; set; } = string.Empty; // "not simulated", etc.


    public bool IsActive => LogicalValue == "1";
}