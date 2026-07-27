using System.Net.Http.Json;
using AbbRobots.Net.WebServices.Models;

namespace AbbRobots.Net.WebServices;


public class CfgService : ICfgService
{
    private readonly HttpClient _httpClient;

    private const string BaseCfgResource = "/rw/cfg";

    public CfgService(HttpClient client)
    {
        _httpClient = client;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CfgResource>> GetCfgResourcesAsync(
        string? domain = null,
        CancellationToken cancellationToken = default)
    {
        string requestUri = string.IsNullOrWhiteSpace(domain)
            ? BaseCfgResource
            : $"{BaseCfgResource}/{domain.Trim().ToLowerInvariant()}";

        HttpResponseMessage response = await _httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseModel = await response.Content
            .ReadFromJsonAsync<CfgResponseModel>(cancellationToken: cancellationToken);

        return responseModel?.Embedded?.Items
            .Where(i => !string.IsNullOrEmpty(i.ResolvedName))
            .Select(i => new CfgResource(
                Name: i.ResolvedName,
                Title: i.Title,
                Type: i.Type,
                Url: i.Links?.Self?.Href))
            .ToList()
            .AsReadOnly()
            ?? [];
    }


}