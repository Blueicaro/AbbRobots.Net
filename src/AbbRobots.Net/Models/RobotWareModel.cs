using System.Text.Json.Serialization;
using System.Collections.Generic;

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