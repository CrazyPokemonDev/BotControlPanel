using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotControlPanel.Bots.AchBotInlineKeyboards
{
    class InlineKeyboardSubscribe
    {
        public static IReplyMarkup Get()
        {
            var subscribe = new InlineKeyboardButton("Subscribe", "")
            {
                Url = "https://t.me/werewolfwolfachievementbot?start=subscribe"
            };
            var unsubscribe = new InlineKeyboardButton("Unsubscribe", "")
            {
                Url = "https://t.me/werewolfwolfachievementbot?start=unsubscribe"
            };
            InlineKeyboardButton[] buttons = { subscribe, unsubscribe };

            IReplyMarkup markup = new InlineKeyboardMarkup(buttons);
            return markup;
        }
    }
}
