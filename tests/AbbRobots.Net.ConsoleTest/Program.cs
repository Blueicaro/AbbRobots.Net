using System;
using System.IO;
using AbbRobots.Net.WebServices;

namespace AbbRobots.Net.ConsoleTest;

class Program
{
    static async Task Main(string[] args)
    {
        string ipRobot = "localhost";
        var myTobot = new AbbRobotClient(ipRobot,"Default User","robotics",80);

        try
        {
            Console.WriteLine("$(Conectando al robot en {ipRobot}...");
            await myTobot.Connect();
            Console.WriteLine("\n Conexión exitosa\n");
           
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Error al conectar con RobotStudio: {ex.Message}");
            Console.WriteLine("Asegúrate de que el controlador virtual está en ejecución y la IP/puerto es correcta.");

        }
        Console.WriteLine("Fin de la conexión");
    
         }
}