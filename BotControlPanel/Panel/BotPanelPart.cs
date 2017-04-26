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
        public Button StopButton { get; }

        public BotPanelPart(TextBlock textBlock, Button start, Button stop)
        {
            TextBlock = textBlock;
            StartButton = start;
            StopButton = stop;
        }
    }
}
