using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;

namespace BotControlPanel.Bots.AchBotInlineKeyboards
{
    public static class InlineKeyboardTellRole
    {
        public static IReplyMarkup Get(string usernameWithoutAt)
        {
            InlineKeyboardButton b = new InlineKeyboardButton("Tell me your role");
            b.Url = "t.me/" + usernameWithoutAt + "?start=tellrole";
            InlineKeyboardButton[] bs = { b };
            IReplyMarkup Markup = new InlineKeyboardMarkup(bs);
            return Markup;
        }
    }
}
