
using AbbRobots.Net.Models;
namespace AbbRobots.Net.WebServices.Services;


public interface IControllerService
{
    /// <summary>
    /// Suscribe a los cambios de estado del controlador (ej: Motors On/Off, Auto/Manual)
    /// mediante el patrón de callback inyectado.
    /// </summary>
    Task<IDisposable> SubscribeToStateAsync(
        Action<ControllerStateChangeEventArgs> onStateChanged,
        SubscriptionPriority priority = SubscriptionPriority.Medium);

    /// <summary>
    /// Inicia la creación de un Backup en el controlador.
    /// Emite el avance mediante la rutina 'onProgress' hasta su finalización.
    /// </summary>
    /// <param name="backupName">Nombre del directorio o paquete de backup.</param>
    /// <param name="onProgress">Callback opcional para recibir el progreso en tiempo real.</param>
    /// <param name="cancellationToken">Token de cancelación de la operación.</param>
    Task<BackupResult> CreateBackupAsync(
        string backupName,
        Action<BackupProgressEventArgs>? onProgress = null,
        CancellationToken cancellationToken = default);
}