using System;
using System.Threading.Tasks;
using AbbRobots.Net.WebServices;
using AbbRobots.Net.WebServices.Services; // Asegura el namespace de SubscriptionPriority si hace falta

Console.WriteLine("==================================================");
Console.WriteLine("       ABB OmniCore - Controller Status Demo      ");
Console.WriteLine("==================================================");

// Puedes probar aquí con la IP real o una falsa para verificar el timeout/error
string robotIp = "localhost"; 
Console.WriteLine($"\n[INFO] Connecting the controller to {robotIp}...");

var robot = new AbbRobotClient(robotIp, "Default User", "robotics",80);

try
{
    // 1. Nos suscribimos al evento antes de abrir el canal
    robot.Events.OnControllerStateChanged += (sender, e) =>
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[EVENTO DE ROBOT] Estado: {e.CtrlState} | Modo: {e.Mode}");
        Console.ResetColor();
    };

    // 2. Ahora sí, esperamos de verdad la suscripción y conexión del WebSocket
    await robot.Events.SubscribeToControllerStateAsync();

    Console.WriteLine("\n[READY] Listening to events in real time.");
    Console.WriteLine("Press any key on this console to exit the demo.");
    Console.ReadKey();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\n[ERROR DE CONEXIÓN] Ocurrió un fallo al suscribirse: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Detalle: {ex.InnerException.Message}");
    }
    Console.ResetColor();
}