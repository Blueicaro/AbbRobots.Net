using System.ComponentModel.DataAnnotations;
using AbbRobots.Net.WebServices.Models;

namespace AbbRobots.Net.WebServices;

public class FileService : IFileService
{
    private readonly HttpClient _httpCLient;
    private readonly string baseFileResource="/fileservice";
    public FileService(HttpClient httpClient)
    {
        _httpCLient = httpClient;
    }
    public async Task<List<string>> GetFileContent(string fileName)
    {
        var url = $"{baseFileResource}/${fileName}";

         HttpResponseMessage response = await _httpCLient.GetAsync(url);

        response.EnsureSuccessStatusCode();

        var content = new List<string>
        {
            await response.Content.ReadAsStringAsync()
        };

        return content;        
    }
}
