using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace BotControlPanel.Bots
{
    public class WerewolfAchievementsBot
    {
        #region Variables
        private TelegramBotClient client;
        #endregion
        public WerewolfAchievementsBot(string token)
        {
            client = new TelegramBotClient(token);
            client.OnUpdate += Client_OnUpdate;
        }

        private void Client_OnUpdate(object sender, Telegram.Bot.Args.UpdateEventArgs e)
        {
            
        }

        #region Control Methods
        #region Is running
        public bool isRunning { get { return client.IsReceiving; } }
        #endregion
        #region Start Bot
        public bool startBot()
        {
            if (!client.IsReceiving)
            {
                client.StartReceiving();
                return true;
            }
            return true;
        }
        #endregion
        #region Stop Bot
        public bool stopBot()
        {
            if (client.IsReceiving)
            {
                client.StopReceiving();
                return true;
            }
            return true;
        }
        #endregion
        #endregion
    }
}
