using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using BotControlPanel.Panel;

namespace BotControlPanel.Bots
{
    public abstract class FlomBot
    {
        #region Variables
        protected TelegramBotClient client;
        public BotPanelPart Panel { get; set; }
        #region Token
        protected string token = "";
        public string Token
        {
            get
            {
                return token;
            }
            set
            {
                token = value;
                try
                {
                    client = new TelegramBotClient(token);
                    client.OnUpdate -= Client_OnUpdate;
                    client.OnUpdate += Client_OnUpdate;
                    BotState = State.Functionable;
                }
                catch
                {
                    client = null;
                    BotState = State.Errored;
                }
            }
        }
        #endregion
        public State BotState = State.Functionable;
        #endregion
        #region Constants
        public abstract string Name { get; }
        protected const long Flom = 267376056;
        #endregion

        #region Control Methods
        #region Start Bot
        public virtual bool StartBot()
        {
            if (BotState == State.Errored) return false;
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
        public virtual bool IsRunning { get { try { return client.IsReceiving; } catch { return false; } } }
        #endregion
        #endregion

        #region Constructor
        /// <summary>
        /// Creates Client using the token and assigns
        /// <see cref="Client_OnUpdate(object, UpdateEventArgs)"/> to it.
        /// </summary>
        /// <param name="token">Token string of the bot to use</param>
        public FlomBot(string token)
        {
            Token = token;
        }

        /// <summary>
        /// You still need to assign a token to this later.
        /// </summary>
        public FlomBot()
        {
            Token = "null";
        }
        #endregion

        #region Enums
        public enum State
        {
            Errored,
            Functionable
        }
        #endregion

        #region Update Method
        protected abstract void Client_OnUpdate(object sender, UpdateEventArgs e);
        #endregion
    }
}
