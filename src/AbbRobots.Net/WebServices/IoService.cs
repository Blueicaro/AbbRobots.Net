using System.Net.Http.Json;
using AbbRobots.Net.Models;
using AbbRobots.Net.WebServices;

namespace AbbRobots.Net.WebServices;

public class IoService : IIoService
{
    #region Fields
    private readonly HttpClient _httpClient;
    private readonly SubscriptionService _subscriptionService;
    private const string BaseIoResource = "/rw/iosystem";
    #endregion
    #region  Constructor
    public IoService(HttpClient httpClient, SubscriptionService subscriptionService)
    {
        _httpClient = httpClient;
        _subscriptionService = subscriptionService;
    }
    #endregion

    #region  Public API
    /// <inheritdoc/>
    public async Task<string> GtSignalValueAsync(string signalName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(signalName);
        string requestUri = $"{BaseIoResource}/signals{signalName}/state";
        HttpResponseMessage response = await _httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();
        string signalValue = await ExtractValueFromRwsResponseAsync(response, cancellationToken);
        return signalValue;
    }


    /// <inheritdoc />
    public Task SetSignalValueAsync(string signalName, string value, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<IDisposable> SubscribeToSignalAsync(string signalName, Action<SignalChangedEventArgs> onSignalUpdate, SubscriptionPriority priority = SubscriptionPriority.Medium)
    {
        throw new NotImplementedException();
    }
    #endregion

    private async Task<string> ExtractValueFromRwsResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}