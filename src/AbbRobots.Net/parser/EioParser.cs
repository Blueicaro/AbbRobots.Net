using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AbbRobots.Net.Models; 
using System.Text;

namespace AbbRobots.Net.Parser;

public class EioParser : CfgParser
{
    // Las listas fuertemente tipadas que tú querías en Pascal
    public List<SignalItem> Signals { get; private set; } = [];
    public List<CrossConnectionItem> CrossConnections { get; private set; } = [];


    /// <summary>
    /// Loads and parses the configuration directly from the path of an EIO.cfg file.
    /// </summary>
    public void LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"The ABB configuration file was not found in: {filePath}");
        }

        // Leemos el texto y se lo pasamos al método especializado
        string rawText = File.ReadAllText(filePath);
        FromString(rawText);
    }
    public void FromString(string rawText)
    {
        Signals.Clear();
        CrossConnections.Clear();

        LoadAndCleanText(rawText);

        if (Sections.TryGetValue("EIO_SIGNAL", out var signalLines))
        {
            EioSignal(signalLines);
        }

        if (Sections.TryGetValue("EIO_CROSS", out var crossLines))
            EioCross(crossLines);
    }

    /// <summary>
    /// Save the signals and cross-connections while adhering to ABB's character-per-line limit.
    /// </summary>
    public void SaveToFile(string filePath)
    {
        StringBuilder sb = new StringBuilder();

        // 1. Cabecera oficial
        sb.AppendLine("EIO:CFG_1.0:6:1::");
        sb.AppendLine();

        // 2. Bloque de SEÑALES (EIO_SIGNAL)
        if (Signals.Count > 0)
        {
            sb.AppendLine("EIO_SIGNAL:");
            foreach (var signal in Signals)
            {
                // We gather all the attributes into a list to process them cleanly.
                List<string> attributes = new List<string> { $"  -Name \"{signal.Name}\"" };

                if (!string.IsNullOrEmpty(signal.SignalType)) attributes.Add($"-SignalType \"{signal.SignalType}\"");
                if (!string.IsNullOrEmpty(signal.Device)) attributes.Add($"-Device \"{signal.Device}\"");
                if (!string.IsNullOrEmpty(signal.DeviceMap)) attributes.Add($"-DeviceMap \"{signal.DeviceMap}\"");
                if (!string.IsNullOrEmpty(signal.Label)) attributes.Add($"-Label \"{signal.Label}\"");
                if (!string.IsNullOrEmpty(signal.Category)) attributes.Add($"-Category \"{signal.Category}\"");
                if (!string.IsNullOrEmpty(signal.Access)) attributes.Add($"-Access \"{signal.Access}\"");
                if (!string.IsNullOrEmpty(signal.DefaultValue)) attributes.Add($"-DefaultValue {signal.DefaultValue}");
                if (!string.IsNullOrEmpty(signal.Invert)) attributes.Add($"-Invert \"{signal.Invert}\"");
                // ... añade aquí el resto de campos si los necesitas ...

         
                sb.AppendLine(ConstruirLineaConWrap(attributes));
            }
            sb.AppendLine();
        }


        if (CrossConnections.Count > 0)
        {
            sb.AppendLine("EIO_CROSS:");
            foreach (var cross in CrossConnections)
            {
                List<string> atributos = new List<string> { $"  -Name \"{cross.Name}\"" };
                if (!string.IsNullOrEmpty(cross.Result)) atributos.Add($"-Res \"{cross.Res}\"");
                if (!string.IsNullOrEmpty(cross.Actor1)) atributos.Add($"-Act1 \"{cross.Act1}\"");
                if (!string.IsNullOrEmpty(cross.InvertActor1)) atributos.Add($"-Oper1 \"{cross.Oper1}\"");
                // Continuar....

                sb.AppendLine(ConstruirLineaConWrap(atributos));
            }
            sb.AppendLine();
        }

        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
    }

    /// <summary>
    /// Helper method that concatenates attributes and inserts a '\' followed by a line break 
    /// if the current line is about to exceed the recommended limit (e.g., 80 characters).
    /// </sumary>
    private string ConstruirLineaConWrap(List<string> atributos, int maxLongitud = 80)
    {
        StringBuilder lineaActual = new StringBuilder();
        StringBuilder resultadoFinal = new StringBuilder();

        for (int i = 0; i < atributos.Count; i++)
        {
            string atributo = atributos[i];

   
            if (lineaActual.Length + atributo.Length + 1 > maxLongitud && lineaActual.Length > 0)
            {
                resultadoFinal.AppendLine(lineaActual.ToString().TrimEnd() + " \\");
                lineaActual.Clear();
                lineaActual.Append("      "); 
            }

            if (lineaActual.Length > 6) 
            {
                lineaActual.Append(" ");
            }

            lineaActual.Append(atributo);
        }

 
        resultadoFinal.Append(lineaActual.ToString());

        return resultadoFinal.ToString();
    }
}


    private void EioCross(List<string> crossLines)
    {
        foreach (var line in crossLines)
        {
            string name = ExtractAttribute(line, "Name");
            string res = ExtractAttribute(line, "Res");

            //Oper 1 
            string act1 = ExtractAttribute(line, "Act1");
            string Oper1 = ExtractAttribute(line, "Oper1");
            string Invert1 = ExtractAttribute(line, "Act1_invert");

            //Oper2
            string act2 = ExtractAttribute(line, "Act2");
            string Oper2 = ExtractAttribute(line, "Oper2");
            string Invert2 = ExtractAttribute(line, "Act2_invert");

            //Oper3
            string act3 = ExtractAttribute(line, "Act3");
            string Oper3 = ExtractAttribute(line, "Oper3");
            string Invert3 = ExtractAttribute(line, "Act3_invert");

            //Oper4
            string act4 = ExtractAttribute(line, "Act4");
            string Oper4 = ExtractAttribute(line, "Oper4");
            string Invert4 = ExtractAttribute(line, "Act4_invert");

            //Oper5
            string act5 = ExtractAttribute(line, "Act4");
            string Invert5 = ExtractAttribute(line, "Act5_invert");


            if (!string.IsNullOrEmpty(name))
            {
                CrossConnections.Add(new CrossConnectionItem(name, res,
                                                                act1, Oper1, Invert1,
                                                                act2, Oper2, Invert2,
                                                                act3, Oper3, Invert3,
                                                                act4, Oper4, Invert4,
                                                                act5, Invert1));
            }
        }
    }

    private void EioSignal(List<string> signalLines)
    {
        foreach (var line in signalLines)
        {
         
            string name = ExtractAttribute(line, "Name");
            string type = ExtractAttribute(line, "SignalType");
            string device = ExtractAttribute(line, "Device");
            string deviceMap = ExtractAttribute(line, "DeviceMap");
            string label = ExtractAttribute(line, "Label");
            string category = ExtractAttribute(line, "Category");
            string access = ExtractAttribute(line, "Access");
            string defaultValue = ExtractAttribute(line, "DefaultValue");
            string invert = ExtractAttribute(line, "invert");
            string safeLevel = ExtractAttribute(line, "SafeLevel");
            string filtAct = ExtractAttribute(line, "FiltAct");
            string filtPas = ExtractAttribute(line, "FiltPas");
            string encType = ExtractAttribute(line, "EncType");
            string maxBitVal = ExtractAttribute(line, "MaxBitVal");
            string maxLog = ExtractAttribute(line, "MaxLog");
            string maxPhys = ExtractAttribute(line, "MaxPhys");
            string minPhysLimit = ExtractAttribute(line, "MinPhysLimit");
            string size = ExtractAttribute(line, "Size");


            if (!string.IsNullOrEmpty(name))
            {
                Signals.Add(new SignalItem(name, type, device, deviceMap, label,
                                            category, access, defaultValue,
                                            invert, safeLevel, filtAct,
                                            filtPas, encType, maxBitVal,
                                            maxLog, maxPhys, minPhysLimit, size));
            }
        }
    }

    /// <summary>
    /// Helper de vanguardia que busca cualquier atributo tipo "-Atributo "Valor"" o "-Atributo Valor"
    /// </summary>
    private static string ExtractAttribute(string line, string attribute)
    {
   
        var match = Regex.Match(line, @$"-{attribute}\s+""([^""]*)""|-{attribute}\s+(\S+)");
        if (match.Success)
        {
       
            return !string.IsNullOrEmpty(match.Groups[1].Value)
                ? match.Groups[1].Value
                : match.Groups[2].Value;
        }
        return string.Empty;
    }
}