using System.Text.Json.Serialization;

namespace AbbRobots.Net.Models;


///<summary
/// It represents the state of an I/O signal on the robot.
/// </summary>

public class SignalModel
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

///<summary>
/// Event data that triggers when an I/O signal changes value.
///</summary>

public class SignalChangedEventArgs : EventArgs
{
    public string SignalName { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool IsSimulated { get; set; }
}






/// <summary>
/// It represents the root of the JSON returned by the robot.
/// </summary>
public class RwsSignalResponse
{
    [JsonPropertyName("_embedded")]
    public EmbeddedSignals Embedded { get; set; } = new();
    [JsonPropertyName("_links")]
    public RwsLinks Links { get; set; } = new();
}

/// <summary>
/// Helper Class for reading RWS links
/// </summary>
public class RwsLinks
{
    [JsonPropertyName("next")]
    public RwsLinkItem? Next { get; set; }
}

public class RwsLinkItem
{
    [JsonPropertyName("href")]
    public string HRef { get; set; } = string.Empty;
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