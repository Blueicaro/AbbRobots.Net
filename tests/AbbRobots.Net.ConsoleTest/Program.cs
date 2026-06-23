using System;
using AbbRobots.Net.Parser;

namespace AbbRobots.Net.ConsoleTest;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== ABB Robots Parser  Test ===");

       string filename="EIO.cfg";
        // Instanciamos nuestro parser moderno
        var parser = new EioParser();
        if (!System.IO.File.Exists(filename))
        {
            Console.WriteLine($"File '{filename}' not found.");
            return;
        }
        // Procesamos el texto bruto
        parser.ProcessSignals(filename);

        // Mostramos los resultados en la consola
        Console.WriteLine($"\nSuccessfully parsed {parser.LoadedSignals.Count} signals:");
        foreach (var signal in parser.LoadedSignals) // <-- Asegúrate de que ponga LoadedSignals
        {
            Console.WriteLine($"-> {signal}");
        }
    }
}