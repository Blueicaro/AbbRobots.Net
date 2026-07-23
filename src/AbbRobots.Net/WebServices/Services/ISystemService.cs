namespace AbbRobots.Net.WebServices.Services;

public  interface ISystemService
{
     ///<summary>
    /// Makes a reboot/// 
    /// </summary>
    Task<bool> RestartRobotAsync();
}