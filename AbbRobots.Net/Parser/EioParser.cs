using System;
using System.Collections.Generic;

namespace AbbRobots.Net.Parser
{
    public class EioParser
    {
        //Lista para guardar las señales
        public List<string> LoadedSignals{get; private set;}=new List<string>
        public void TextParser(string rawText)
        {
            LoadedSignals.Clear();
            string[] lines = rawText.Split(new[] {"\r\n","\r","\n"},StringSplitOptions.None);

            foreach (string line in lines)
            {
                //Remove white spaces at begin and End
                string cleanLine = line.Trim();
                
                //Skip empty lines and coments
                if ((string.IsNullOrWhiteSpace(cleanLine)) || cleanLine.StartsWith("#")){
                    continue;
                }

                if (cleanLine.StartsWith("-Name"))
                {
                    LoadedSignals.Add(cleanLine);
                }

            }

        }
    }
}