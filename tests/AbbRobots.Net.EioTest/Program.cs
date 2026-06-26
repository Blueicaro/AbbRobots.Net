using System;
using System.Threading.Tasks;
using AbbRobots.Net;
using AbbRobots.Net.Models;
using AbbRobots.Net.WebServices;

Console.WriteLine("=== CONFIGURACIÓN DE SEÑALES EN VIVO ===");

// Instanciamos el cliente principal de tu librería
var robot = new AbbRobotClient ("localhost","Default User","robotics",80);

var newSignal = new SignalItem("doPrensaOk", "DO", "PN_Internal_Device", "1", "Señal de validación");

// 1. Pedimos el control del dominio de configuración a través del nuevo servicio dedicado
Console.WriteLine("Solicitando bloqueo del dominio de configuración...");
if (await robot.Mastership.RequestAsync("cfg"))
{
    Console.WriteLine(" -> ¡Dominio 'cfg' bloqueado en exclusiva!");

    // 2. Operamos sobre las I/O usando el servicio especializado de I/O
    Console.WriteLine($"Creando señal estructural '{newSignal.Name}'...");
    bool exito = await robot.Io.CreateSignalInConfigurationAsync(newSignal);

    if (exito)
    {
        Console.WriteLine(" -> ¡Señal inyectada con éxito en la base de datos interna!");
    }
    else
    {
        Console.WriteLine(" -> [Error] El robot rechazó la configuración de la señal.");
    }

    // 3. Liberamos el control usando de nuevo el servicio de Mastership
    Console.WriteLine("Liberando dominio 'cfg'...");
    await robot.Mastership.ReleaseAsync("cfg");

    // 4. Si la inyección fue bien, reiniciamos el robot (puedes mover el método Restart a un SystemService en el futuro)
    if (exito)
    {
        Console.WriteLine("Reiniciando el robot para aplicar cambios...");
        await robot.Io.RestartRobotAsync(); 
    }
}
else
{
    Console.WriteLine(" -> [Error] No se pudo obtener el Mastership. El recurso está ocupado por otra sesión.");
}