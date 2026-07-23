namespace AbbRobots.Net.WebServices.Services;

public interface IRobotWareService
{
    Task<string> GetSystemInfoAsync();
}