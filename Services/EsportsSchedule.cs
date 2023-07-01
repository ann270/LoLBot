using System.Text;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;

namespace TelegramBot.Services;

public class EsportsSchedule
{
    private Dictionary<long, List<Events>> userEvents = new Dictionary<long, List<Events>>();
    private Dictionary<long, bool> reminderStatus = new Dictionary<long, bool>();
    
    public async Task GetEsportsScheduleAsync(long chatId, TelegramBotClient botClient)
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        };

        var client = new HttpClient(handler);
        client.BaseAddress = new Uri(Constants.apiAddress);

        var response = await client.GetAsync("LoL/GetSchedule");
        response.EnsureSuccessStatusCode();
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonConvert.DeserializeObject<ScheduleResponse>(content);
        
        foreach (var match in result.Data.Schedule.Events)
        {
            StringBuilder responseBuilder = new StringBuilder();
            responseBuilder.AppendLine($"<u>StartTime:</u> {match.StartTime}");
            responseBuilder.AppendLine($"<u>BlockName:</u> {match.BlockName}");
            responseBuilder.AppendLine($"<u>League:</u> {match.League.Name}");
            responseBuilder.AppendLine($"<u>Teams:</u>");
            
            foreach (var team in match.Match.Teams)
            {
                responseBuilder.AppendLine($"- {team.Name}");
            }
            userEvents.TryGetValue(chatId, out var eventsList);
            if (eventsList == null)
            {
                eventsList = new List<Events>();
                userEvents[chatId] = eventsList;
            }
            eventsList.Add(match);
            
            var buttonText = "\uD83D\uDD14 Set Reminder";
            if (reminderStatus.ContainsKey(chatId) && reminderStatus[chatId])
            {
                buttonText = "Reminder is enabled";
            }
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData(buttonText, "set_reminder")
                }
            });

            var parseMode = ParseMode.Html;
            await botClient.SendTextMessageAsync(chatId, responseBuilder.ToString(), parseMode: parseMode, replyMarkup: inlineKeyboard);
        }
    }
    public async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery, ITelegramBotClient botClient)
    {
        if (callbackQuery.Data == "set_reminder")
        {
            var userId = callbackQuery.From.Id;
            if (reminderStatus.ContainsKey(userId) && reminderStatus[userId])
            {
                reminderStatus[userId] = false;
                await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "Reminder is disabled");

                userEvents.TryGetValue(userId, out var eventsList);

                if (eventsList != null && eventsList.Count > 0)
                {
                    var currentEvent = eventsList[0];
                    eventsList.RemoveAt(0);
                    //await DeleteEventFromDatabaseAsync(requestID, userID, chatId, botClient);
                }
            }
            else
            {
                reminderStatus[userId] = true;
                await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "Reminder is enabled");

                userEvents.TryGetValue(userId, out var eventsList);

                if (eventsList != null && eventsList.Count > 0)
                {
                    var currentEvent = eventsList[0];
                    eventsList.RemoveAt(0);
                    await AddEventToDatabaseAsync(userId, currentEvent);
                    //await GetFromDatabaseAsync(dbResponse.RequestID, userId.ToString(), callbackQuery.Message.Chat.Id, botClient);
                }
            }

            var buttonText = reminderStatus[userId] ? "Reminder is enabled" : "\uD83D\uDD14 Set Reminder";
            var updatedInlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData(buttonText, "set_reminder")
                }
            });
            await botClient.EditMessageReplyMarkupAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, updatedInlineKeyboard);
        }
    }


    public async Task AddEventToDatabaseAsync(long userId, Events currentEvent)
    {
        PostToDatabase postToDatabase = new PostToDatabase();
        DbResponse dbResponse = new DbResponse
        {
            RequestID = "",
            UserID = userId.ToString(),
            StartTime = currentEvent.StartTime,
            BlockName = currentEvent.BlockName,
            LeagueName = currentEvent.League.Name,
            MatchTeams = currentEvent.Match.Teams.Select(team => team.Name).ToList(),
            MatchID = currentEvent.Match.Id
        };
        await postToDatabase.PostToDatabaseAsync(dbResponse);
    }
    public async Task DeleteEventFromDatabaseAsync(string requestID, string userID, long chatId, ITelegramBotClient botClient)
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        };
        var client = new HttpClient(handler);
        client.BaseAddress = new Uri(Constants.apiAddress);
        
        var response = await client.DeleteAsync($"LoL/DeleteDataFromDB?requestID={requestID}&userID={userID}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<DbResponse>(content);
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Data deleted from the database successfully.");
        }
        else
        {
            Console.WriteLine($"Failed to delete data from the database. Status code: {response.StatusCode}");
        }
    }
    public async Task GetFromDatabaseAsync(string requestID, string userID, long chatId, ITelegramBotClient botClient)
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        };

        var client = new HttpClient(handler);
        client.BaseAddress = new Uri(Constants.apiAddress);

        var response = await client.GetAsync($"LoL/GetDataFromDB?requestID={requestID}&userID={userID}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<DbResponse>(content);

        DateTime startTime;
        if (DateTime.TryParse(result.StartTime, out startTime))
        {
            DateTime notificationTime = startTime.AddMinutes(-10);
            DateTime currentTime = DateTime.Now;
            if (currentTime < notificationTime)
            {
                TimeSpan delay = notificationTime - currentTime;
                await Task.Delay(delay);
                
                StringBuilder notificationBuilder = new StringBuilder();
                notificationBuilder.AppendLine("Нагадування про матч!");
                notificationBuilder.AppendLine($"Початок матчу: {result.StartTime}");
                notificationBuilder.AppendLine($"Блок: {result.BlockName}");
                notificationBuilder.AppendLine($"Ліга: {result.LeagueName}");
                notificationBuilder.AppendLine("Команди:");
                foreach (string team in result.MatchTeams)
                {
                    notificationBuilder.AppendLine($"- {team}");
                }
                await botClient.SendTextMessageAsync(chatId, notificationBuilder.ToString());
            }
        }
    }
}