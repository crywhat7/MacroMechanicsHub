
using System;
using System.Net.Http;
using MacroMechanicsHub.Models;

namespace MacroMechanicsHub.Services
{
    public class LoLApiService
    {
        private const string API_URL = "https://127.0.0.1";
        private const string PORT = "2999";
        private const string API_ENDPOINT_ALL_GAMEDATA = "/GetLiveclientdataAllgamedata";

        public bool IsApiAvailable()
        {
            try
            {
                using (var handler = new HttpClientHandler())
                {
                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                    using (var client = new HttpClient(handler))
                    {
                        var response = client.GetAsync($"{API_URL}:{PORT}{API_ENDPOINT_ALL_GAMEDATA}").Result;
                        return response.IsSuccessStatusCode;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public AllGameData GetAllGameData() {
            try
            {
                using (var handler = new HttpClientHandler())
                {
                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                    using (var client = new HttpClient(handler))
                    {
                        var response = client.GetAsync($"{API_URL}:{PORT}{API_ENDPOINT_ALL_GAMEDATA}").Result;
                        if (response.IsSuccessStatusCode)
                        {
                            var jsonResponse = response.Content.ReadAsStringAsync().Result;
                            return Newtonsoft.Json.JsonConvert.DeserializeObject<AllGameData>(jsonResponse);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return null;

        }

    }
}
