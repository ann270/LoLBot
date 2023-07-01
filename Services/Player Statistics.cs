using System.Text;
using Newtonsoft.Json;
using Telegram.Bot;

namespace TelegramBot.Services;

public class PlayerStatistics
{
    public async Task GetPlayerStatisticsAsync(string username, string region, long chatId, TelegramBotClient botClient)
    {
        try
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };

            var client = new HttpClient(handler);
            client.BaseAddress = new Uri(Constants.apiAddress);

            var result = await client.GetAsync($"LoL/GetAccountStatistic?Username={username}&Region={region}");
            result.EnsureSuccessStatusCode();
            var content = await result.Content.ReadAsStringAsync();
            var player = JsonConvert.DeserializeObject<List<PlayerDetailsResponse>>(content);

            StringBuilder responseBuilder = new StringBuilder();
            responseBuilder.AppendLine("<u>Player statistics:</u>");
            foreach (var playerDetails in player)
            {
                responseBuilder.AppendLine($"<b>Username:</b> {playerDetails.Username}");
                responseBuilder.AppendLine($"<b>Rank:</b> {playerDetails.Rank}");
                responseBuilder.AppendLine($"<b>LP:</b> {playerDetails.Lp}");
                responseBuilder.AppendLine($"<b>Win/Loss Ratio:</b> {playerDetails.WinLossRatio}");
                responseBuilder.AppendLine();

                responseBuilder.AppendLine("<u>Most Played Champions:</u>");

                foreach (var champ in playerDetails.MostPlayedChamps)
                {
                    responseBuilder.AppendLine($"● <b>Name:</b> {champ.Name}");
                    responseBuilder.AppendLine($"  <b>Win Percentage:</b> {champ.WinPercentage}");
                    responseBuilder.AppendLine($"  <b>Total Games:</b> {champ.TotalGames}");
                }
            }

            string responseMessage = responseBuilder.ToString();
            await Constants.botClient.SendTextMessageAsync(chatId, responseMessage, null,
                Telegram.Bot.Types.Enums.ParseMode.Html, disableWebPagePreview: true);
        }
        catch (HttpRequestException)
        {
            await botClient.SendTextMessageAsync(chatId, "Invalid data. Please try again with correct data.");
        }
    }
}