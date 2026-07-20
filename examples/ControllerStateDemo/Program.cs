using System;
using System.Threading.Tasks;
using AbbRobots.Net;
using AbbRobots.Net.Models;
using AbbRobots.Net.WebServices;

Console.WriteLine("==================================================");
Console.WriteLine("       ABB OmniCore - Controller Status Demo      ");
Console.WriteLine("==================================================");

//robot Ip
string robotIp="127.0.0.1";

var robot = new AbbRobotClient(robotIp,"Default User", "robotics",80);

Console.WriteLine ($"\n[INFO] Connecting the controller to {robotIp}");

try
{   // Subscribe to the event BEFORE starting active listening
    robot.Events.OnControllerStateChanged+=(sender,e)=>
    {
        Console.WriteLine($"\n[EVENT] ¡Change detected in the Controller!");
        Console.WriteLine($"         -> Physical State (Motors): {e.CtrlState.ToUpper()}");
        Console.WriteLine($"         -> Mode of Operation:       {e.Mode.ToUpper()}");
    };
    Console.WriteLine("\n[READY] Listening to events in real time.");
    Console.WriteLine("Press any key on this console to exit the demo.");
    Console.ReadKey();
}
catch(Exception ex)
{
    Console.WriteLine ($"[ERROR] There was a failure in the demo: {ex.Message}");
}
