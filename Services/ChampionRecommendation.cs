using System.Text;
using Newtonsoft.Json;
using Telegram.Bot;

namespace TelegramBot.Services;

public class ChampionRecommendation
{
    public async Task GetChampionRecommendation(string userMessage, long chatId, TelegramBotClient botClient)
    {
        try
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };
            var requestData = new
            {
                messages = new[]
                {
                    new
                    {
                        content = userMessage
                    }
                }
            };
            
            var json = JsonConvert.SerializeObject(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using (var httpClient = new HttpClient(handler))
            {
                var apiUrl = "https://localhost:7124/LoL/OpenAI";
                var response = await httpClient.PostAsync(apiUrl, content);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                var openAiResponse = JsonConvert.DeserializeObject<ChampionRecommendationResponse>(responseBody);
                await botClient.SendTextMessageAsync(chatId, openAiResponse.Content);
            }
        }
        catch (HttpRequestException)
        {
            await botClient.SendTextMessageAsync(chatId, "Invalid data. Please try again with correct data.");
        }
    }
}

