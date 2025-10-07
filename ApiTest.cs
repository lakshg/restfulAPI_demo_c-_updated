using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ApiTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "ZenQA-ApiTests/1.0");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            
            try
            {
                Console.WriteLine("Testing GET /objects...");
                var response = await client.GetAsync("https://api.restful-api.dev/objects");
                Console.WriteLine($"Status: {response.StatusCode}");
                Console.WriteLine($"Headers: {response.Headers}");
                
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Content Length: {content.Length}");
                Console.WriteLine($"First 200 chars: {content.Substring(0, Math.Min(200, content.Length))}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}