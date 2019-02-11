using Telegram.Bot.Types.ReplyMarkups;

namespace BotControlPanel.Bots.WWTBCustomKeyboards
{
    public static class UnderdevKeyboard
    {
        public const string UnderdevAddButtonString = "Add entry to #underdev";
        public static KeyboardButton UnderdevAddButton { get; } = new KeyboardButton(UnderdevAddButtonString);
        public const string UnderdevEditButtonString = "Edit entry from #underdev";
        public static KeyboardButton UnderdevEditButton { get; } = new KeyboardButton(UnderdevEditButtonString);
        public const string UnderdevRemoveButtonString = "Remove entry from #underdev";
        public static KeyboardButton UnderdevRemoveButton { get; } = new KeyboardButton(UnderdevRemoveButtonString);
        public const string BackToStartKeyboardButtonString = StartKeyboard.BackToStartKeyboardButtonString;
        public static KeyboardButton BackToStartKeyboardButton { get; } = new KeyboardButton(
            BackToStartKeyboardButtonString);
        private static KeyboardButton[] row1 = { UnderdevAddButton };
        private static KeyboardButton[] row2 = { UnderdevEditButton };
        private static KeyboardButton[] row3 = { UnderdevRemoveButton };
        private static KeyboardButton[] row4 = { BackToStartKeyboardButton };
        private static KeyboardButton[][] array = { row1, row2, row3, row4 };
        public static ReplyKeyboardMarkup Markup { get; } = new ReplyKeyboardMarkup(array);
    }
}
