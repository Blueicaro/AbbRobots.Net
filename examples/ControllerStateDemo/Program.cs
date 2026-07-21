using System;
using System.Threading.Tasks;
using AbbRobots.Net.WebServices;
using AbbRobots.Net.WebServices.Services; // Asegura el namespace de SubscriptionPriority si hace falta

Console.WriteLine("==================================================");
Console.WriteLine("       ABB OmniCore - Controller Status Demo      ");
Console.WriteLine("==================================================");

string robotIp = "localhost"; 
Console.WriteLine($"\n[INFO] Connecting the controller to {robotIp}...");



