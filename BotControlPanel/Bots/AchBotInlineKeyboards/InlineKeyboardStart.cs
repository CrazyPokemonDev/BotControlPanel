using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotControlPanel.Bots.AchBotInlineKeyboards
{
    public static class InlineKeyboardStart
    {
        public static IReplyMarkup Get(long chatid)
        {
            InlineKeyboardButton b = new InlineKeyboardButton("Start", "start_" + chatid.ToString());
            InlineKeyboardButton[] bs = { b };
            IReplyMarkup Markup = new InlineKeyboardMarkup(bs);
            return Markup;
        }
    }
}
