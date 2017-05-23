using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotControlPanel.Bots.WWTBCustomKeyboards
{
    public static class StartKeyboard
    {
        public const string BackToStartKeyboardButtonString = "Back to main menu";
        public const string ClosedlistButtonString = "#closedlist";
        public static KeyboardButton ClosedlistButton { get; } = new KeyboardButton(ClosedlistButtonString);
        public const string UnderdevButtonString = "#underdev";
        public static KeyboardButton UnderdevButton { get; } = new KeyboardButton(UnderdevButtonString);
        public const string RefreshChannelMessageButtonString = "Refresh channel message";
        public static KeyboardButton RefreshChannelMessageButton { get; } =
            new KeyboardButton(RefreshChannelMessageButtonString);
        public const string EditChangelogString = "Edit changelog";
        public static KeyboardButton EditChangelogButton = new KeyboardButton(EditChangelogString);
        private static KeyboardButton[] row1 = { ClosedlistButton };
        private static KeyboardButton[] row2 = { UnderdevButton };
        private static KeyboardButton[] row3 = { RefreshChannelMessageButton };
        private static KeyboardButton[] row4 = { EditChangelogButton };
        private static KeyboardButton[][] array = { row1, row2, row3, row4 };
        public static ReplyKeyboardMarkup Markup { get; } = new ReplyKeyboardMarkup(array);
    }
}
