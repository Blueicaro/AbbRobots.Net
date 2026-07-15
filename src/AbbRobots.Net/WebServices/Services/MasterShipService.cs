using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace AbbRobots.Net.WebServices.Services;

/// <summary>
/// Service dedicated to managing exclusive control (Mastership) over the different domains of the ABB robot.
/// </summary>

public class MastershipService
{
    private readonly HttpClient _httpClient;

    public MastershipService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Requests exclusive control of a specific domain (e.g., "cfg", "motion", "rapid").
    /// </summary>

    public async Task<bool> RequestAsync(string domain= "")
    {
       HttpResponseMessage  response;
       if (domain == "")
        {
         response = await _httpClient.PostRwsFormAsync("rw/mastership/request",[]);    
        }
        else
       {
         response = await _httpClient.PostRwsFormAsync($"rw/mastership/{domain}/request",[]);
        }
       
       return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Releases exclusive control of a previously acquired domain.
    /// </summary>   
    public async Task<bool> ReleaseAsync(string domain="")
    {
       
        
        HttpResponseMessage response;
        if (domain == "")
        {
            response = await _httpClient.PostRwsFormAsync($"rw/mastership/release",[]);    
        }else
        {
            response = await _httpClient.PostRwsFormAsync($"rw/mastership/{domain}/release",[]);
        }
        
        return response.IsSuccessStatusCode;
    }

}