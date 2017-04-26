using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BotControlPanel.Panel
{
    public class BotPanelPart
    {
        public TextBlock TextBlock { get; }
        public Button StartButton { get; }
        public Button SettingsButton { get; }

        public BotPanelPart(TextBlock textBlock, Button start, Button settings)
        {
            TextBlock = textBlock;
            StartButton = start;
            SettingsButton = settings;
        }
    }
}
