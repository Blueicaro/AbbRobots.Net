using System;
using AbbRobots.Net.Parser;

namespace AbbRobots.Net.ConsoleTest;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== ABB Robots Parser Vanguard Test ===");

        // Un texto de prueba simulando un EIO.cfg real con líneas partidas (\) y comentarios (#)
        string mockEioCfg = """
        # Initial configuration file for testing
        EIO_SIGNAL:
              -Name "di_Siemens_OK" -SignalType "DI" -Device "Profinet"\
              -Label "PLC Communication OK" -DeviceMap "0"

              -Name "do_Robot_InHome" -SignalType "DO" -Device "Profinet"\
              -Label "Robot safe in home position" -DeviceMap "1"
        """;

        // Instanciamos nuestro parser moderno
        var parser = new EioParser();
        
        // Procesamos el texto bruto
        parser.ProcessSignals(mockEioCfg);

        // Mostramos los resultados en la consola
        Console.WriteLine($"\nSuccessfully parsed {parser.Signals.Count} signals:");
        foreach (var signal in parser.Signals)
        {
            Console.WriteLine($"-> {signal}");
        }
    }
}