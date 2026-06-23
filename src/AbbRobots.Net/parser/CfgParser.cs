using System;
using System.Collections.Generic;

namespace AbbRobots.Net.Parser
{
    public class CfgParser
    {
        // Protected allows inheriting classes (like EioParser) to access this list directly
        protected List<string> CleanedLogicalLines { get; private set; } = new List<string>();

        /// <summary>
        /// Reads raw configuration text, strips comments, and unifies multiline statements (\).
        /// </summary>
        public void LoadAndCleanText(string rawText)
        {
            CleanedLogicalLines.Clear();

            string[] lines = rawText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            string accumulatedLine = "";

            foreach (string line in lines)
            {
                string cleanLine = line.Trim();

                // Ignore empty lines or standalone comments
                if (string.IsNullOrEmpty(cleanLine) || cleanLine.StartsWith("#"))
                {
                    continue;
                }

                // If we have an active multiline accumulation, stitch them together
                if (!string.IsNullOrEmpty(accumulatedLine))
                {
                    accumulatedLine += " " + cleanLine;
                }
                else
                {
                    accumulatedLine = cleanLine;
                }

                // Check for ABB's line continuation character (\)
                if (accumulatedLine.EndsWith("\\"))
                {
                    // Strip the trailing backslash and wait for the next iteration
                    accumulatedLine = accumulatedLine.Substring(0, accumulatedLine.Length - 1).Trim();
                    continue; 
                }

                // Line is fully assembled and independent, store it
                CleanedLogicalLines.Add(accumulatedLine);
                accumulatedLine = "";
            }
        }
    }
}