namespace AbbRobots.Net.WebServices;

public interface IRobotWareService
{
    Task<string> GetSystemInfoAsync();
}