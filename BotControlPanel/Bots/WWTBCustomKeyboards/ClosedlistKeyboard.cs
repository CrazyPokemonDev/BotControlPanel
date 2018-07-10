using TelegramBotApi.Types.Markup;

namespace BotControlPanel.Bots.WWTBCustomKeyboards
{
    public static class ClosedlistKeyboard
    {
        public const string ClosedlistAddButtonString = "Add entry to #closedlist";
        public static KeyboardButton ClosedlistAddButton { get; } = new KeyboardButton(ClosedlistAddButtonString);
        public const string ClosedlistEditButtonString = "Edit entry from #closedlist";
        public static KeyboardButton ClosedlistEditButton { get; } = new KeyboardButton(ClosedlistEditButtonString);
        public const string ClosedlistRemoveButtonString = "Remove entry from #closedlist";
        public static KeyboardButton ClosedlistRemoveButton { get; } = new KeyboardButton(ClosedlistRemoveButtonString);
        public const string BackToStartKeyboardButtonString = StartKeyboard.BackToStartKeyboardButtonString;
        public static KeyboardButton BackToStartKeyboardButton { get; } = new KeyboardButton(
            BackToStartKeyboardButtonString);
        private static KeyboardButton[] row1 = { ClosedlistAddButton };
        private static KeyboardButton[] row2 = { ClosedlistEditButton };
        private static KeyboardButton[] row3 = { ClosedlistRemoveButton };
        private static KeyboardButton[] row4 = { BackToStartKeyboardButton };
        private static KeyboardButton[][] array = { row1, row2, row3, row4 };
        public static ReplyKeyboardMarkup Markup { get; } = new ReplyKeyboardMarkup(array);
    }
}
