using System.Collections.Generic;
using System.Linq;

namespace AbbRobots.Net.Parser;

// Al poner ': CfgParser', EioParser hereda toda la lógica de limpiar el texto
public class EioParser : CfgParser
{
    // Usamos el nombre que tú elegiste: LoadedSignals
    public List<string> LoadedSignals { get; private set; } = [];

    public void ProcessSignals(string rawText)
    {
        LoadedSignals.Clear();

        // 1. Llamamos al método del padre para que limpie comentarios y junte las barras '\'
        LoadAndCleanText(rawText);

        // 2. Filtramos la lista resultante que nos da el padre (CleanedLogicalLines)
        LoadedSignals = CleanedLogicalLines
            .Where(line => line.StartsWith("-Name"))
            .ToList();
    }
}