using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.Services;

namespace TelegramBot;

public enum BotState
{
    None,
    WaitingForVideoTitle,
    WaitingForUsername,
    WaitingForRegion,
    WaitingForChampionName,
    WaitingForContent
}
public class LoLBot
{
    private BotState botState = BotState.None;
    private CancellationToken cancellationToken = new CancellationToken();
    private ReceiverOptions receiverOptions = new ReceiverOptions { AllowedUpdates = { } };
    private readonly EsportsSchedule esportsSchedule = new EsportsSchedule();
    private string keyword;
    private string username;
    private string region;
    private string championName;
    private string content;

    public async Task Start()
    {
        Constants.botClient.StartReceiving(HandlerUpdateAsync, HandlerError, receiverOptions, cancellationToken);
        var botMe = await Constants.botClient.GetMeAsync();
        Console.WriteLine($"Бот {botMe.Username} почав працювати");
        Console.ReadKey();
    }

    private Task HandlerError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException =>
                $"Error in Telegram Bot API: \n {apiRequestException.ErrorCode}" +
                $"\n{apiRequestException.Message}",
            _ => exception.ToString()
        };
        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }

    private async Task HandlerUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if(update.Type == UpdateType.Message)
        {
            var message = update.Message;
            switch (message.Text)
            {
                case "/start":
                    Methods method = new Methods();
                    await method.Menu(message.Chat.Id, Constants.botClient);
                    break;
                case "\uD83D\uDCCAAccount Statistic":
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Please enter your username:");
                    botState = BotState.WaitingForUsername;
                    break;
                case "\uD83D\uDCC3eSports Events Schedule":
                    //EsportsSchedule esportsSchedule = new EsportsSchedule();
                    await esportsSchedule.GetEsportsScheduleAsync(message.Chat.Id, Constants.botClient);
                    break;
                case "\uD83C\uDF9EMatch Replay":
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Please enter the video title:");
                    botState = BotState.WaitingForVideoTitle;
                    break;
                case "\uD83C\uDFAEChampion Recommendation":
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Please enter the name of the champion you are playing against:");
                    botState = BotState.WaitingForContent;
                    break;
                case "\uD83D\uDC8EChampion Skins":
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Please enter the champion name:");
                    botState = BotState.WaitingForChampionName;
                    break;
               default:
                   if (botState == BotState.WaitingForVideoTitle)
                   {
                       keyword = message.Text;
                       MatchReplay matchReplay = new MatchReplay();
                       await matchReplay.GetMatchReplayAsync(keyword, message.Chat.Id, Constants.botClient);
                       botState = BotState.None;
                   }
                   else if (botState == BotState.WaitingForUsername)
                   {
                       username = message.Text;
                       await botClient.SendTextMessageAsync(message.Chat.Id, "Please enter your region:");
                       botState = BotState.WaitingForRegion;
                   }
                   else if (botState == BotState.WaitingForRegion)
                   {
                       region = message.Text;
                       PlayerStatistics playerStatistics = new PlayerStatistics();
                       await playerStatistics.GetPlayerStatisticsAsync(username, region, message.Chat.Id, Constants.botClient);
                       botState = BotState.None;
                   }
                   else if (botState == BotState.WaitingForChampionName)
                   {
                       championName = message.Text;
                       ChampionSkins championSkins = new ChampionSkins();
                       await championSkins.GetChampionSkinsAsync(championName, message.Chat.Id, Constants.botClient);
                       botState = BotState.None;
                   }
                   else if (botState == BotState.WaitingForContent)
                   {
                       content = message.Text;
                       ChampionRecommendation championRecommendation = new ChampionRecommendation();
                       await championRecommendation.GetChampionRecommendation(content, message.Chat.Id, Constants.botClient);
                       botState = BotState.None;
                   }
                   break;
            }
        }
        else if (update.Type == UpdateType.CallbackQuery)
        {
            var callbackQuery = update.CallbackQuery;
            await esportsSchedule.HandleCallbackQueryAsync(callbackQuery, botClient);
        }
    }
}