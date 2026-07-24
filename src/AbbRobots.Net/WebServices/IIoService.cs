using AbbRobots.Net.WebServices.Models;

namespace AbbRobots.Net.WebServices;
/// <summary>
/// Provides operations to interact with the Input/Output (I/O) system of the ABB OmniCore/RobotWare robot.
/// </summary>
public interface IIoService
{
    /// <summary>
    /// Obtain the current value of an I/O signal asynchronously.
    /// </summary>
    /// <param name="signalName">Name of the sign (eg: "di_Sensor1", "do_Gripper").</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>The current value of the signal as text (e.g. "0", "1", "12.5").</returns>

    Task<string> GetSignalValueAsync(string signalName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets or forces the value of an I/O signal in the robot.
    /// </summary>
    /// <param name="signalName">Signal name to modify.</param>
    /// <param name="value">New value to be written.</param>
    /// <param name="cancellationToken">Cancelling token.</param>
    Task SetSignalValueAsync(string signalName, string value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribes to real-time state changes of a specific I/O signal.
    /// </summary>
    /// <param name="signalName">Signal name to monitor.</param>
    /// <param name="onSignalUpdate">Injected routine that will run each time the signal changes.</param>
    /// <param name="priority">Subscription priority in RWS.</param>
    /// <returns>Object <see cref="IDisposable"/> to cancel the subscription when making Dispose().</returns>
    Task<IDisposable> SubscribeToSignalAsync(string signalName, Action<SignalChangedEventArgs> onSignalUpdate, SubscriptionPriority priority = SubscriptionPriority.Medium);

}