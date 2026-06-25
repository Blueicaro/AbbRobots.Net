using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using AbbRobots.Net.WebServices.Services;

namespace AbbRobots.Net.WebServices;
{
    public class AbbRobotClient
    {
        private readonly HttpClient _httpClient;
        
        public RobotWareServices RobotWare {get;}
        public IoService Io{get;}

        public AbbRobotClient(string ipAddress, string username="Default User", string password="robotics")
        {
            var baseUri = Uri($"https://{ipAddress}/");
            
            //Ignore certificates selfsigned by ABB
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback= (message, cert, chain, errors)=> true
            };

            _httpClient = new HttpClient(handler){BaseAddress = baseUri};

            var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

            RobotWare = new RobotWareServices(_httpClient);
            Io = new IoService(_httpClient)

        }
    }
}