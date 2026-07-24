using AbbRobots.Net;

using var robot = new AbbRobotClient("localhost", "Default User", "robotics",80);
await robot.ConnectAsync();


