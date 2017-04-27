using System;
using System.Windows.Forms;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Threading;
//addusing

namespace BotControlPanel.ScriptBot
{
    class Program
    {
        private const long flomsId = 267376056;

        private static TelegramBotClient client;
        private static bool running = true;
        //adddefinition

        static void Main(string[] args)
        {
            client = new TelegramBotClient(args[0]);
            client.OnUpdate += Client_OnUpdate;
            client.StartReceiving();
            while (running)
            {
                //Beep beep i'm a sheep
            }
        }

        private static void Client_OnUpdate(object sender, UpdateEventArgs e)
        {
            try
            {
                if (e.Update.Type == UpdateType.MessageUpdate &&
                    e.Update.Message.Type == MessageType.TextMessage)
                {
                    Telegram.Bot.Types.Message msg = e.Update.Message;
                    string text = e.Update.Message.Text;
                    string cmd = text.Contains("@")
                        ? text.Remove(text.IndexOf('@')).ToLower()
                        : text.ToLower();
                    switch (cmd)
                    {
                        case "/stopbot":
                            if (msg.From.Id == flomsId)
                            {
                                running = false;
                                client.StopReceiving();
                                client.SendTextMessageAsync(msg.Chat.Id, "*dies*");
                            }
                            break;
                            //newcommand
                    }
                }
            }
            catch (Exception ex)
            {
                client.SendTextMessageAsync(flomsId, "Error has ocurred in scriptedBot: "
                    + ex.InnerException + "\n" + ex.Message + "\n" + ex.StackTrace);
            }
        }
    }
}
