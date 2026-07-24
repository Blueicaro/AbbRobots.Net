using System.Text.Json;
using AbbRobots.Net.WebServices.Models;


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
    public async Task<string> GetSignalValueAsync(string signalName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(signalName);

        string requestUri = $"{BaseIoResource}/signals/{signalName}";
        HttpResponseMessage response = await _httpClient.GetRwsAsync(requestUri);
        response.EnsureSuccessStatusCode();

        string signalValue = await ExtractValueFromRwsResponseAsync(response, cancellationToken);
        return signalValue;
    }


    /// <inheritdoc />
    public async Task SetSignalValueAsync(string signalName, string value, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(signalName);
        ArgumentNullException.ThrowIfNull(value);

        string requestUri = $"{BaseIoResource}/signals/{signalName}?action=set";
        var fields = new Dictionary<string, string> { { "lvalue", value } };

        HttpResponseMessage response = await _httpClient.PostRwsFormAsync(requestUri, fields);
        response.EnsureSuccessStatusCode();
    }

    /// <inheritdoc />
    public Task<IDisposable> SubscribeToSignalAsync(string signalName, Action<SignalChangedEventArgs> onSignalUpdate, SubscriptionPriority priority = SubscriptionPriority.Medium)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(signalName);
        ArgumentNullException.ThrowIfNull(onSignalUpdate);

        return _subscriptionService.SubscribeToSignalAsync(signalName, onSignalUpdate, priority);
    }
    #endregion

    private async Task<string> ExtractValueFromRwsResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        string json = await response.Content.ReadAsStringAsync(cancellationToken);

        RwsSignalResponse? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<RwsSignalResponse>(json);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("The ABB robot returned a response that could not be parsed as a signal state.", ex);
        }

        RwsSignal? signal = parsed?.Embedded?.Resources?.FirstOrDefault();

        if (signal == null)
        {
            throw new InvalidOperationException("The ABB robot response did not contain any signal data.");
        }

        return signal.LogicalValue;
    }
}