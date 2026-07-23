using AbbRobots.Net;

using var robot = new AbbRobotClient("localhost", "Default User", "robotics",80);
await robot.ConnectAsync();

// 1. Obtener todos los dominios principales (eio, moc, sys, etc.)
var dominios = await robot.Cfg.GetCfgResourcesAsync();
foreach (var d in dominios)
{
    Console.WriteLine($"Dominio CFG: {d.Name} ({d.Title})");
}

// 2. Obtener los recursos específicos del dominio EIO
var recursosEio = await robot.Cfg.GetCfgResourcesAsync("eio");
foreach (var res in recursosEio)
{
    Console.WriteLine($"Recurso EIO: {res.Name}");
}

