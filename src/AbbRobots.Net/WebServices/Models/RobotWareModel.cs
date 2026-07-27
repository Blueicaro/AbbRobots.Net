using System.Text.Json.Serialization;

namespace AbbRobots.Net.WebServices.Models;
public class LicenseResponseModel
{
    [JsonPropertyName("state")]
    public List<LicenseStateModel> State { get; set; } = new();
}

public class LicenseStateModel
{
    [JsonPropertyName("license")]
    public string LicenseType { get; set; } = string.Empty;
}
/// <summary>
/// OpMode state
/// "state": [
///        {
///            "_type": "pnl-opmode",
///            "_title": "opmode",
///            "opmode": "AUTO"
/// </summary>
public record OpModeResource(
  string? Title = null,
  string? Type = null,
  string? OpMode = null
);
