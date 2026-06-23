using System;
using System.Collections.Generic;

namespace AbbRobots.Net.Parser;

public class CfgParser
{
  
    protected Dictionary<string, List<String>> Sections{get; private set;}=[];
    

    public void LoadAndCleanText(string rawText)
    {
       Sections.Clear();

        string[] lines = rawText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        string accumulatedLine = "";
        string currentSection="UNDEFINED";

        foreach (string line in lines)
        {
            string cleanLine = line.Trim();
            
            //Jump spaces, or comments
            if (string.IsNullOrEmpty(cleanLine) || cleanLine.StartsWith("#"))
            {
                continue;
            }
            //Check if the line is a section header (e.g., [SectionName])
            if(cleanLine.EndsWith(":"))
            {
                //Extract the section name without the colon and trim any whitespace
                currentSection = cleanLine.Substring(0, cleanLine.Length - 1).Trim();
                if (!Sections.ContainsKey(currentSection))
                {
                    Sections[currentSection]=[];
                }
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

           
            Sections[currentSection].Add(accumulatedLine);
            accumulatedLine = "";
        }
    }
}