
namespace AbbRobots.Net.WebServices;

public class CfgService
{
   private readonly HttpClient _httpClient;

   public CfgService( HttpClient client)
    {
        _httpClient = client;
    }  
}