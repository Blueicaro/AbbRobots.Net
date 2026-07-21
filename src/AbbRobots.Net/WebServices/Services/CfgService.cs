
namespace AbbRobots.Net.WebServices.Services;

public class CfgService
{
   private readonly HttpClient _httpClient;

   public CfgService( HttpClient client)
    {
        _httpClient = client;
    }  
}