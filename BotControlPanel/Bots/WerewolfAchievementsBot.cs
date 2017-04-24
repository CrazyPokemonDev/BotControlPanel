using BotControlPanel.Bots.AchBotInlineKeyboards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotControlPanel.Bots
{
    public class WerewolfAchievementsBot
    {
        #region Variables
        private TelegramBotClient client;
        private Dictionary<long, int> groupsMessageIdsDict = new Dictionary<long, int>();
        private Dictionary<long, string> groupsMessagesDict = new Dictionary<long, string>();
        private Dictionary<long, string> waitingFor = new Dictionary<long, string>();
        #endregion
        #region Constants
        private const string botUsername = "werewolfwolfachievementbot";
        #endregion
        #region Constructor
        public WerewolfAchievementsBot(string token)
        {
            client = new TelegramBotClient(token);
            client.OnUpdate += Client_OnUpdate;
        }
        #endregion

        #region Update Handler
        private void Client_OnUpdate(object sender, Telegram.Bot.Args.UpdateEventArgs e)
        {
            try
            {
                if (e.Update.Type == UpdateType.MessageUpdate)
                {
                    if (e.Update.Message.Type == MessageType.TextMessage)
                    {
                        #region Stargame recognized
                        if ((e.Update.Message.Text == "/startgame@werewolfbot" ||
                            e.Update.Message.Text == "/startgame" ||
                            e.Update.Message.Text == "/startgame@werewolfbetabot" ||
                            e.Update.Message.Text == "/startchaos@werewolfbot" ||
                            e.Update.Message.Text == "/startchaos" ||
                            e.Update.Message.Text == "/startchaos@werewolfbetabot")
                            && (e.Update.Message.Chat.Type == ChatType.Group ||
                            e.Update.Message.Chat.Type == ChatType.Supergroup))
                        {
                            IReplyMarkup rm = InlineKeyboardTellRole.Get(botUsername,
                                e.Update.Message.Chat.Id);
                            Task<Message> t = client.SendTextMessageAsync(e.Update.Message.Chat.Id,
                                "➡️*CURRENT GAME*⬅️\nNo roles given yet",
                                parseMode: ParseMode.Markdown,
                                replyMarkup: rm);
                            t.Wait();
                            if (groupsMessageIdsDict.ContainsKey(e.Update.Message.Chat.Id))
                            {
                                groupsMessageIdsDict.Remove(e.Update.Message.Chat.Id);
                            }
                            groupsMessageIdsDict.Add(e.Update.Message.Chat.Id, t.Result.MessageId);
                            if (groupsMessagesDict.ContainsKey(e.Update.Message.Chat.Id))
                            {
                                groupsMessagesDict.Remove(e.Update.Message.Chat.Id);
                            }
                            groupsMessagesDict.Add(e.Update.Message.Chat.Id, "➡️*CURRENT GAME*⬅️");
                            return;
                        }
                        #endregion

                        #region Custom start
                        if (e.Update.Message.Text.StartsWith("/start ")
                            && e.Update.Message.Chat.Type == ChatType.Private)
                        {
                            string arg = e.Update.Message.Text.Substring(7);
                            if (arg.StartsWith("tellrole_"))
                            {
                                Task t = client.SendTextMessageAsync(e.Update.Message.Chat.Id,
                                    "Tell me your role, please:");
                                t.Wait();
                                if (!waitingFor.ContainsKey(e.Update.Message.Chat.Id))
                                    waitingFor.Add(e.Update.Message.Chat.Id, arg);
                                return;
                            }
                        }
                        #endregion

                        #region Waiting for
                        if (waitingFor.ContainsKey(e.Update.Message.Chat.Id))
                        {
                            string arg = waitingFor[e.Update.Message.Chat.Id];
                            if (arg.StartsWith("tellrole_"))
                            {
                                long chatid = Convert.ToInt64(arg.Substring(9));
                                string news = groupsMessagesDict[chatid];
                                string newnews = "";
                                foreach (string s in news.Split('\n'))
                                {
                                    if (!s.StartsWith(e.Update.Message.From.FirstName + ": "))
                                    {
                                        newnews += s + "\n";
                                    }
                                }
                                newnews += e.Update.Message.From.FirstName + ": " + e.Update.Message.Text;
                                groupsMessagesDict[chatid] = newnews;
                                Task t = client.EditMessageTextAsync(chatid, groupsMessageIdsDict[chatid],
                                    newnews, parseMode: ParseMode.Markdown,
                                    replyMarkup: InlineKeyboardTellRole.Get(botUsername, chatid));
                                t.Wait();
                                waitingFor.Remove(e.Update.Message.Chat.Id);
                            }
                        }
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                client.SendTextMessageAsync(267376056, "WWAchBot Error:" + ex.Message + ex.StackTrace);
            }
        }
        #endregion

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
