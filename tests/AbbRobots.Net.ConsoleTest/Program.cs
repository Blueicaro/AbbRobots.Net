using System;
using System.IO;
using AbbRobots.Net.Parser;

namespace AbbRobots.Net.ConsoleTest;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== ABB Robots Parser - Lazarus to .NET 10 ===");

        string filePath = Path.Combine(AppContext.BaseDirectory, "EIO.cfg");

        if (!File.Exists(filePath))
        {
            Console.WriteLine($"[ERROR] Fichero no encontrado.");
            return;
        }

        string realEioContent = File.ReadAllText(filePath);

        var parser = new EioParser();
        // Procesamos el archivo mapeando a objetos reales
        parser.ProcessEioFile(realEioContent);

        // 💡 EXCLUSIVO: Ahora accedemos a propiedades reales de objetos reales
        Console.WriteLine($"\n[SEÑALES DETECTADAS: {parser.Signals.Count}]");
        foreach (var sig in parser.Signals)
        {
            Console.WriteLine($"Señal: {sig.Name,-20} Tipo: {sig.SignalType,-5} Tarjeta: {sig.Device} Map: {sig.DeviceMap}");
        }

        Console.WriteLine($"\n[CONEXIONES CRUZADAS DETECTADAS: {parser.CrossConnections.Count}]");
        foreach (var cross in parser.CrossConnections)
        {
            Console.WriteLine($"Conexión: {cross.Name} -> Resultado: {cross.Result}");
        }
    }
}