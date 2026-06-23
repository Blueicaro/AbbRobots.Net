using System;
using System.Collections.Generic;

namespace AbbRobots.Net.Parser
{
    public class EioParser
    {
        //Lista para guardar las señales
        public List<string> LoadedSignals { get; private set; } = new List<string>();
        public void ParseText(string rawText)
{
    LoadedSignals.Clear();

    string[] lines = rawText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
    
    // To store the lines
    string accumulatedLine = "";

    foreach (string line in lines)
    {
        string cleanLine = line.Trim();

        // Skip empty lines or comments, but WATCH OUT: 
//      // Only if we are not accumulating a split line."
        if (string.IsNullOrEmpty(cleanLine) || cleanLine.StartsWith("#"))
        {
            continue;
        }

        // If we got info we add it
        if (!string.IsNullOrEmpty(accumulatedLine))
        {
            accumulatedLine += " " + cleanLine;
        }
        else
        {
            accumulatedLine = cleanLine;
        }

        // This line finish with  '\'?)
        if (accumulatedLine.EndsWith("\\"))
        {
            // Clean the slash at the end
            accumulatedLine = accumulatedLine.Substring(0, accumulatedLine.Length - 1).Trim();
            
            // Jump to the next file
            continue; 
        }
  
        if (accumulatedLine.StartsWith("-Name"))
        {
            LoadedSignals.Add(accumulatedLine);
        }

        // Clean for next line
        accumulatedLine = "";
    }
}
    }
}