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
        public static IReplyMarkup Get(string usernameWithoutAt, long chatid)
        {
            InlineKeyboardButton b = new InlineKeyboardButton("Tell me your role");
            b.Url = "http://telegram.me/" + usernameWithoutAt + "?start=tellrole_" + chatid.ToString();
            InlineKeyboardButton[] bs = { b };
            IReplyMarkup Markup = new InlineKeyboardMarkup(bs);
            return Markup;
        }
    }
}
