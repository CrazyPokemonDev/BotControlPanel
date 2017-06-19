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
            InlineKeyboardButton start = new InlineKeyboardButton("Start", "start_" + chatid.ToString());
            InlineKeyboardButton abort = new InlineKeyboardButton("Abort", "stop_" + chatid.ToString());
            InlineKeyboardButton[] bs = { start, abort };
            IReplyMarkup Markup = new InlineKeyboardMarkup(bs);
            return Markup;
        }
    }
}
