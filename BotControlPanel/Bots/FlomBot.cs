using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;

namespace BotControlPanel.Bots
{
    public abstract class FlomBot
    {
        #region Variables
        protected TelegramBotClient client;
        #endregion

        #region Control Methods
        #region Start Bot
        public virtual bool StartBot()
        {
            if (!client.IsReceiving) client.StartReceiving();
            return true;
        }
        #endregion
        #region Stop Bot
        public virtual bool StopBot()
        {
            if (client.IsReceiving) client.StopReceiving();
            return true;
        }
        #endregion
        #region Is Running
        public virtual bool isRunning { get { return client.IsReceiving; } }
        #endregion
        #endregion

        #region Constructor
        public FlomBot(string token)
        {
            client = new TelegramBotClient(token);
            client.OnUpdate += Client_OnUpdate;
        }
        #endregion

        #region Update Method
        protected abstract void Client_OnUpdate(object sender, UpdateEventArgs e);
        #endregion
    }
}
