using System;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Threading;
//addusing

namespace ScriptedBot
{
    class Program
    {
        private const long flomsId = 267376056;

        private static TelegramBotClient client;
        private static bool running = true;
        private static string Username;
        //adddefinition

        public static void Main(string[] args)
        {
            client = new TelegramBotClient(args[0]);
            var t = client.GetMeAsync();
            t.Wait();
            Username = t.Result.Username;
            client.OnUpdate += Client_OnUpdate;
            client.StartReceiving();
            /*while (running)
            {
                //Beep beep i'm a sheep
            }*/
        }

        public static void Stop()
        {
            running = false;
            client.StopReceiving();
        }

        private static void Client_OnUpdate(object sender, UpdateEventArgs e)
        {
            try
            {
                if (e.Update.Type == UpdateType.Message &&
                    e.Update.Message.Type == MessageType.Text)
                {
                    Message msg = e.Update.Message;
                    string text = e.Update.Message.Text;
                    string cmd = text.Contains("@")
                        ? text.Remove(text.IndexOf('@')).ToLower()
                        : (text.Contains(" ")
                            ? text.Remove(text.IndexOf(' ')).ToLower()
                            : text.ToLower());
                    switch (cmd)
                    {
                        case "/stopbot":
                            if (msg.From.Id == flomsId)
                            {
                                running = false;
                                client.StopReceiving();
                                client.GetUpdatesAsync(e.Update.Id + 1);
                                client.SendMessageAsync(msg.Chat.Id, "*dies*");
                            }
                            break;
                            //newcommand
                    }
                }
            }
            catch (Exception ex)
            {
                client.SendMessageAsync(flomsId, "Error has ocurred in scriptedBot: "
                    + ex.InnerException + "\n" + ex.Message + "\n" + ex.StackTrace);
            }
        }
        //addmethod
    }
}
