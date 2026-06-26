using System;
using System.IO;
using System.Collections.Generic;
using AbbRobots.Net.Models; 
using AbbRobots.Net.Parser; 



string rutaEio = "EIO.cfg"; 

if (!File.Exists(rutaEio))
{
    Console.WriteLine($"[Error] Can't find the file: {Path.GetFullPath(rutaEio)}");
    return;
}


var parser = new EioParser();

// Read the file
Console.WriteLine("Reading file...");
string contenidoEio = File.ReadAllText(rutaEio);

//  RawText
Console.WriteLine("Parser...");
parser.LoadFromFile(contenidoEio);

// Public property
var señalesEio = parser.Signals; 
Console.WriteLine($"Found signals: {señalesEio.Count}\n");


for (int i = 0; i< señalesEio.Count ; i++)
{
    //  SignalItem
    SignalItem signal = señalesEio[i];
    string signalName = signal.Name; 
    Console.WriteLine(signalName);
}




