using System.Text.Json.Nodes;

namespace AbbRobots.Net.WebServices;

/// <summary>
/// Common helpers for reading HAL-formatted RWS responses (root -> "_embedded" -> "resources"/"_state" -> [...]).
/// Centralizes parsing logic repeated across various services (RapidServices, IoService, etc.)
/// to avoid duplicating null checks and the choice between "resources" and "_state".
/// </summary>
public static class JsonNodeExtensions
{
    /// <summary>
    /// Returns the embedded elements ("_embedded" -> "resources" or "_state") from an RWS response.
    /// It never throws an exception due to unexpected structure: if any level is missing or an element
    /// is null, it is simply skipped, and processing continues with the rest.
    /// </summary>
    public static IEnumerable<JsonNode> GetEmbeddedResources(this JsonNode? root)
    {
        JsonArray? resources = root?["_embedded"]?["resources"]?.AsArray()
                             ?? root?["_embedded"]?["_state"]?.AsArray();

        if (resources == null)
        {
            yield break;
        }

        foreach (JsonNode? item in resources)
        {
            if (item != null)
            {
                yield return item;
            }
        }
    }


}