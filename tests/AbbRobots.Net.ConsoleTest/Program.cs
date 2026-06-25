using System;
using System.IO;
using AbbRobots.Net.WebServices;
using AbbRobots.Net.WebServices.Services;


namespace AbbRobots.Net.ConsoleTest;

class Program
{
    static async Task Main(string[] args)
    {
        string ipRobot = "localhost";
        var myTobot = new AbbRobotClient(ipRobot, "Default User", "robotics", 80);

        try
        {
            Console.WriteLine($"Conectando al robot en {ipRobot}...");
            bool conected = await myTobot.ConnectAsync();
            if (conected)
            {
                Console.WriteLine("\n Succefull\n");
                var signals = await myTobot.Io.GetSignalsAsync();
                Console.WriteLine($"Signals count: {signals.Count}");
                Console.WriteLine("{0,-30} | {1,-5} | {2,-10} | {3,-5}", "NOMBRE SEÑAL", "TIPO", "CATEGORÍA", "VALOR");
                Console.WriteLine(new string('-', 60));

                foreach (var sig in signals)
                {
                    // Mostramos un extracto en consola
                    Console.WriteLine("{0,-30} | {1,-5} | {2,-10} | {3,-5}",
                        sig.Name, sig.Type, sig.Category, sig.LogicalValue);
                }

            }
            else
            {
                Console.WriteLine("\n No succefull\n");
            }


        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Error al conectar con RobotStudio: {ex.Message}");
            Console.WriteLine("Asegúrate de que el controlador virtual está en ejecución y la IP/puerto es correcta.");

        }
        Console.WriteLine("Fin de la conexión");

    }
}