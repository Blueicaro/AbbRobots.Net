using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AbbRobots.Net.WebServices;

var robot = new AbbRobotClient ("localhost","Default User","robotics",80);

 bool master = await robot.Mastership.RequestAsync("cfg");
 if (master)
{
    Console.WriteLine("Master requested");
     if (await robot.Mastership.ReleaseAsync("cfg"))
    {
        Console.WriteLine("Master Release");
    }
    else
    {
        
    }Console.WriteLine("Fail Master Release");
}
else
{
    Console.WriteLine("Master request fail");
}

