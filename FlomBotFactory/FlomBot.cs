using FlomBotFactory.Events;
using FlomBotFactory.Panel;
using System;
using System.Windows;
using TelegramBotApi;
using TelegramBotApi.Enums;
using TelegramBotApi.Types.Events;

namespace FlomBotFactory
{
    public abstract class FlomBot
    {
        protected string token = "";
        public FlomBot.State BotState = State.Functionable;
        protected TelegramBot client;
        protected const long Flom = 267376056;

        public BotPanelPart Panel { get; set; }

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
                    if (client != null) client.OnUpdate -= Client_OnUpdate;
                    client = new TelegramBot(token);
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

        public abstract string Name { get; }

        public event EventHandler<BotStartedEventArgs> BotStarted;

        public event EventHandler<BotStoppedEventArgs> BotStopped;

        public virtual bool StartBot()
        {
            if (BotState == State.Errored)
                return false;
            if (!client.IsReceiving)
                client.StartReceiving((UpdateType[])null);
            EventHandler<BotStartedEventArgs> botStarted = BotStarted;
            if (botStarted != null)
                botStarted((object)this, new BotStartedEventArgs(this));
            return true;
        }

        public virtual bool StopBot()
        {
            if (client.IsReceiving)
                client.StopReceiving();
            EventHandler<BotStoppedEventArgs> botStopped = BotStopped;
            if (botStopped != null)
                botStopped((object)this, new BotStoppedEventArgs(this));
            return true;
        }

        public virtual bool IsRunning
        {
            get
            {
                try
                {
                    return client.IsReceiving;
                }
                catch
                {
                    return false;
                }
            }
        }

        public FlomBot(string token)
        {
            Token = token;
        }

        public FlomBot()
        {
            Token = "null";
        }

        protected abstract void Client_OnUpdate(object sender, UpdateEventArgs e);

        public enum State
        {
            Errored,
            Functionable,
        }
    }
}
