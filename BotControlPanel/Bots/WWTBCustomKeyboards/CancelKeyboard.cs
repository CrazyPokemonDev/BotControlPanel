using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotControlPanel.Bots.WWTBCustomKeyboards
{
    public static class CancelKeyboard
    {
        public const string CancelButtonString = "Cancel";
        public static KeyboardButton CancelButton { get; } = new KeyboardButton(CancelButtonString);
        private static KeyboardButton[] row1 = { CancelButton };
        public static ReplyKeyboardMarkup Markup { get; } = new ReplyKeyboardMarkup(row1);
    }
}
