using TelegramBotApi.Types.Markup;

namespace BotControlPanel.Bots.WWTBCustomKeyboards
{
    public static class ChangelogKeyboard
    {
        public const string AddPostToChangelogString = "Add Post to Changelog";
        public static KeyboardButton AddPostToChangelogButton = new KeyboardButton(AddPostToChangelogString);
        public static KeyboardButton BackToStartKeyboardButton = new KeyboardButton(StartKeyboard.BackToStartKeyboardButtonString);
        private static KeyboardButton[] row1 = { AddPostToChangelogButton };
        private static KeyboardButton[] row2 = { BackToStartKeyboardButton };
        private static KeyboardButton[][] array = { row1, row2 };
        public static ReplyKeyboardMarkup Markup { get; } = new ReplyKeyboardMarkup(array);
    }
}
