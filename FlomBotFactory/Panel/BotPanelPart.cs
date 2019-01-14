using System.Windows.Controls;

namespace FlomBotFactory.Panel
{
    public class BotPanelPart
    {
        public TextBlock TextBlock { get; }

        public Button StartButton { get; }

        public Button SettingsButton { get; }

        public BotPanelPart(TextBlock textBlock, Button start, Button settings)
        {
            this.TextBlock = textBlock;
            this.StartButton = start;
            this.SettingsButton = settings;
        }
    }
}
