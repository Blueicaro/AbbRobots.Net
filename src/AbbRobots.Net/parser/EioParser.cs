using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
//using AbbRobots.Net.Models; // Importamos nuestros modelos nuevos

namespace AbbRobots.Net.Parser;

public class EioParser : CfgParser
{
    // Las listas fuertemente tipadas que tú querías en Pascal
    public List<SignalItem> Signals { get; private set; } = [];
    public List<CrossConnectionItem> CrossConnections { get; private set; } = [];

    public void ProcessEioFile(string rawText)
    {
        Signals.Clear();
        CrossConnections.Clear();

        // 1. El padre clasifica el archivo por secciones en el Diccionario
        LoadAndCleanText(rawText);

        // 2. PARSEAR SECCIÓN DE SEÑALES (EIO_SIGNAL)
        if (Sections.TryGetValue("EIO_SIGNAL", out var signalLines))
        {
            foreach (var line in signalLines)
            {
                // Usamos Regex para extraer los atributos de la línea de ABB
                string name = ExtractAttribute(line, "Name");
                string type = ExtractAttribute(line, "SignalType");
                string device = ExtractAttribute(line, "Device");
                string deviceMap = ExtractAttribute(line, "DeviceMap");
                string label = ExtractAttribute(line, "Label");

                if (!string.IsNullOrEmpty(name))
                {
                    Signals.Add(new SignalItem(name, type, device, deviceMap, label));
                }
            }
        }

        // 3. PARSEAR SECCIÓN DE CONEXIONES CRUZADAS (EIO_CROSS_CONNECTION)
        if (Sections.TryGetValue("EIO_CROSS_CONNECTION", out var crossLines))
        {
            foreach (var line in crossLines)
            {
                string name = ExtractAttribute(line, "Name");
                string res = ExtractAttribute(line, "Res");
                string act1 = ExtractAttribute(line, "Act1");

                if (!string.IsNullOrEmpty(name))
                {
                    CrossConnections.Add(new CrossConnectionItem(name, res, act1));
                }
            }
        }
    }

    /// <summary>
    /// Helper de vanguardia que busca cualquier atributo tipo "-Atributo "Valor"" o "-Atributo Valor"
    /// </summary>
    private static string ExtractAttribute(string line, string attribute)
    {
        // Expresión regular que caza tanto texto con comillas como sin comillas
        var match = Regex.Match(line, @$"-{attribute}\s+""([^""]*)""|-{attribute}\s+(\S+)");
        if (match.Success)
        {
            // Devolvemos el grupo que haya capturado la información
            return !string.IsNullOrEmpty(match.Groups[1].Value) 
                ? match.Groups[1].Value 
                : match.Groups[2].Value;
        }
        return string.Empty;
    }
}