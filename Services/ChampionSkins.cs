using System.Text;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace TelegramBot.Services;

public class ChampionSkins
{
    public async Task GetChampionSkinsAsync(string championName, long chatId, TelegramBotClient botClient)
    {
        try
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };
            
            var client = new HttpClient(handler);
            client.BaseAddress = new Uri(Constants.apiAddress);
            
            var result = await client.GetAsync($"LoL/GetChampionSkins?Name={championName}");
            result.EnsureSuccessStatusCode();
            var content = await result.Content.ReadAsStringAsync();
            var championSkinsResponse = JsonConvert.DeserializeObject<ChampionSkinsResponse>(content);
            
            foreach (var champion in championSkinsResponse.Champion)
            {
                foreach (var skin in champion.Skins)
                {
                    StringBuilder responseBuilder = new StringBuilder();
                    string skinName = skin.Name;
                    string imageUrl = skin.ImageUrl;
                    responseBuilder.AppendLine($"<b>Skin Name:</b> {skinName}");
                    responseBuilder.AppendLine(imageUrl);
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