

Console.WriteLine("===========================================");
Console.WriteLine("  ABB OmniCore/RobotWare SDK - Demo Client ");
Console.WriteLine("===========================================");

// 1. Configurar dirección del robot (RobotStudio local o Robot Real)
//string robotIp = "127.0.0.1"; // O "192.168.125.1" para conexión física por servicio
using var cts = new CancellationTokenSource();

// Cancelar operaciones con CTRL+C
Console.CancelKeyPress += (sender, e) =>
{
    Console.WriteLine("\nCancelando y cerrando conexión...");
    e.Cancel = true;
    cts.Cancel();
};

Console.WriteLine("\nPrograma finalizado correctamente.");