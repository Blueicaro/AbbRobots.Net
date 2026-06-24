using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AbbRobots.Net.Models; // Importamos nuestros modelos nuevos

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

        LoadAndCleanText(rawText);

        if (Sections.TryGetValue("EIO_SIGNAL", out var signalLines))
        {
            EioSignal(signalLines);
        }

        if (Sections.TryGetValue("EIO_CROSS", out var crossLines))
            EioCross(crossLines);
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
            // Usamos Regex para extraer los atributos de la línea de ABB
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