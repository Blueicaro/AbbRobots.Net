using System;
using System.Collections.Generic;

namespace AbbRobots.Net.Parser;

public class CfgParser
{
    // Protected permite que EioParser pueda leer esta lista directamente
    protected List<string> CleanedLogicalLines { get; private set; } = [];

    public void LoadAndCleanText(string rawText)
    {
        CleanedLogicalLines.Clear();

        string[] lines = rawText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        string accumulatedLine = "";

        foreach (string line in lines)
        {
            string cleanLine = line.Trim();

            if (string.IsNullOrEmpty(cleanLine) || cleanLine.StartsWith("#"))
            {
                continue;
            }

            if (!string.IsNullOrEmpty(accumulatedLine))
            {
                accumulatedLine += " " + cleanLine;
            }
            else
            {
                accumulatedLine = cleanLine;
            }

            if (accumulatedLine.EndsWith("\\"))
            {
                accumulatedLine = accumulatedLine.Substring(0, accumulatedLine.Length - 1).Trim();
                continue; 
            }

            CleanedLogicalLines.Add(accumulatedLine);
            accumulatedLine = "";
        }
    }
}