namespace AbbRobots.Net.WebServices;

public  interface ISystemService
{
     ///<summary>
    /// Makes a reboot/// 
    /// </summary>
    Task<bool> RestartRobotAsync();
}