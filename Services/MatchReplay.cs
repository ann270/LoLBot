using System.Text;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace TelegramBot.Services;

public class MatchReplay
{
    public async Task GetMatchReplayAsync(string keyword, long chatId, TelegramBotClient botClient)
    {
        try
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };
        
            var client = new HttpClient(handler);
            client.BaseAddress = new Uri(Constants.apiAddress);

            var result = await client.GetAsync($"LoL/YouTubeSearch?Keyword={keyword}");
            result.EnsureSuccessStatusCode();
            var content = await result.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<YouTubeResponse>(content);
        
            foreach (var item in response.Items)
            {
                if (!string.IsNullOrEmpty(item.Url))
                {
                    StringBuilder responseBuilder = new StringBuilder();
                    responseBuilder.AppendLine(item.Url);
                    var parseMode = ParseMode.Html;
                    await botClient.SendTextMessageAsync(chatId, responseBuilder.ToString(), parseMode: parseMode);
                }
            }
        }
        catch (HttpRequestException)
        {
            await botClient.SendTextMessageAsync(chatId, "Invalid data. Please try again with correct data.");
        }
    }
}