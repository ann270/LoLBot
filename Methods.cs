using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot;

public class Methods
{ 
    public async Task Menu(long chatId, TelegramBotClient botClient)
    {
        ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(
            new[]
            {
                new KeyboardButton[] { "\uD83D\uDCCAAccount Statistic", "\uD83D\uDCC3eSports Events Schedule" },
                new KeyboardButton[] { "\uD83C\uDF9EMatch Replay", "\uD83C\uDFAEChampion Recommendation" },
                new KeyboardButton[] { "\uD83D\uDC8EChampion Skins" }
            }
        )
        {
            ResizeKeyboard = true
        };
        await botClient.SendTextMessageAsync(chatId, "Hello! This bot can show you player account statistics, eSports events schedule, " +
                                                     "it allows you to watch match replay and provide champion recommendations. " +
                                                     "You can also view all the skins available for any champion.\n" +
                                                     "Choose the action you want to perform:", replyMarkup: replyKeyboardMarkup);
    }
}