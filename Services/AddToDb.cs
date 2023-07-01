using System.Text;
using Newtonsoft.Json;

namespace TelegramBot.Services;

public class PostToDatabase
{
    public async Task PostToDatabaseAsync(DbResponse dbResponse)
    {
        var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };
            
            using (var httpClient = new HttpClient(handler))
            {
                var json = JsonConvert.SerializeObject(dbResponse);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync("https://localhost:7124/LoL/AddToDB", content);
                
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Record added to the database successfully.");
                }
                else
                {
                    Console.WriteLine($"Failed to add record to the database. Status code: {response.StatusCode}");
                }
            }
    }
    
}