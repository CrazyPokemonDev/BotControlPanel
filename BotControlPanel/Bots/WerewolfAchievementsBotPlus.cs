using BotControlPanel.Bots.AchBotInlineKeyboards;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotControlPanel.Bots
{
    public class WerewolfAchievementsBotPlus
    {
        #region Custom "Game" class
        class Game
        {
            #region Pre-declared stuff, such as variables and constants
            public Message pinmessage { get; set; }
            public List<User> players { get; set; }
            public state gamestate { get; set; }
            private TelegramBotClient client;
            public Dictionary<User, string> role = new Dictionary<User, string>();
            public long GroupId { get; }

            private const string joinMessageText = "*Join this game!*\n\nPin this message and remember "
                + "to press start as soon as the game starts.";
            private const string stoppedMessageText = "*This game is finished!*";
            private string playerlist;
            #endregion

            public Game(TelegramBotClient cl, long groupid)
            {
                client = cl;
                UpdatePlayerlist();
                GroupId = groupid;
            }

            public enum state
            {
                Joining,
                Running,
                Stopped
            }

            public bool AddPlayer(User newplayer)
            {
                if (!players.Contains(newplayer) && gamestate == state.Joining)
                {
                    players.Add(newplayer);
                    UpdatePlayerlist();
                    return true;                    
                }
                return false;
            }

            public void Start()
            {
                gamestate = state.Running;
            }

            public void Stop()
            {
                gamestate = state.Stopped;
                UpdatePlayerlist();
            }

            public bool RemovePlayer(User oldplayer)
            {
                if(players.Contains(oldplayer))
                {
                    players.Remove(oldplayer);
                    UpdatePlayerlist();
                    return true;
                }
                return false;
            }

            public void UpdatePlayerlist()
            {
                playerlist = "*Players:*";

                foreach(var p in players)
                {
                    playerlist += "\n" + p.FirstName;
                    if (gamestate == state.Running)
                    {
                        if (role.ContainsKey(p)) playerlist += ": " + role[p];
                        else playerlist += ": No role detected yet";
                    }
                }
                if (gamestate == state.Running)
                    client.EditMessageTextAsync(pinmessage.Chat.Id, pinmessage.MessageId, joinMessageText
                        + "\n\n" + playerlist, parseMode: ParseMode.Markdown,
                        replyMarkup: InlineKeyboardStop.Get(GroupId)).Wait();
                else if (gamestate == state.Joining)
                    client.EditMessageTextAsync(pinmessage.Chat.Id, pinmessage.MessageId, joinMessageText
                        + "\n\n" + playerlist, parseMode: ParseMode.Markdown,
                        replyMarkup: InlineKeyboardStart.Get(GroupId)).Wait();
                else if (gamestate == state.Stopped)
                    client.EditMessageTextAsync(pinmessage.Chat.Id, pinmessage.MessageId, stoppedMessageText,
                        parseMode: ParseMode.Markdown).Wait();
            }
        }
        #endregion

        #region Variables
        private TelegramBotClient client;
        private Dictionary<long, Game> games = new Dictionary<long, Game>();
        private Dictionary<string, string> roleAliases = new Dictionary<string, string>();
        List<User> justCalledStop = new List<User>();
        #endregion
        #region Constants
        private const string basePath = "C:\\Olfi01\\BotControlPanel\\AchievementsBot\\";
        private const string aliasesPath = basePath + "aliases.dict";
        private List<long> adminIds = new List<long>() { 267376056 };   //Ludwig, add your ID
        #endregion
        #region Constructor
        public WerewolfAchievementsBotPlus(string token)
        {
            client = new TelegramBotClient(token);
            client.OnUpdate += Client_OnUpdate;
            client.OnCallbackQuery += Client_OnCallbackQuery;
        }
        #endregion

        #region Callback Query Handler
        private void Client_OnCallbackQuery(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            string data = e.CallbackQuery.Data;
            #region Callback Query Start
            if (data.StartsWith("start_"))
            {
                long id = Convert.ToInt64(data.Substring(6));
                if (games.ContainsKey(id))
                {
                    games[id].Start();
                    games[id].UpdatePlayerlist();
                    client.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "Game is now considered running.").Wait();
                }
                else
                {
                    client.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "Did not find that game.").Wait();
                }
            }
            #endregion
            #region Callback Query Stop
            else if (data.StartsWith("stop_"))
            {
                long id = Convert.ToInt64(data.Substring(5));
                if (games.ContainsKey(id))
                {
                    if (justCalledStop.Contains(e.CallbackQuery.From))
                    {
                        games[id].Stop();
                        games[id].UpdatePlayerlist();
                        games.Remove(id);
                        client.AnswerCallbackQueryAsync(e.CallbackQuery.Id,
                            "The game is now considered stopped.").Wait();
                        justCalledStop.Remove(e.CallbackQuery.From);
                    }
                    else
                    {
                        justCalledStop.Add(e.CallbackQuery.From);
                        client.AnswerCallbackQueryAsync(e.CallbackQuery.Id,
                            "Press this button again if the game really has stopped already.").Wait();
                        Timer t = new Timer(new TimerCallback
                            (
                                delegate
                                {
                                    justCalledStop.Remove(e.CallbackQuery.From);
                                }
                            ), null, 10*1000, Timeout.Infinite);
                    }
                }
                else
                {
                    client.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "Did not find that game.").Wait();
                }
            }
            #endregion
        }
        #endregion
        #region Update Handler
        private void Client_OnUpdate(object sender, Telegram.Bot.Args.UpdateEventArgs e)
        {
            if (e.Update.Type == UpdateType.MessageUpdate)
            {
                if (e.Update.Message.Type == MessageType.TextMessage && 
                    (e.Update.Message.Chat.Type == ChatType.Group ||
                    e.Update.Message.Chat.Type == ChatType.Supergroup))
                {
                    var text = e.Update.Message.Text;
                    var msg = e.Update.Message;

                    #region Commands only
                    switch(text.Replace("@werewolfbot", "").Replace('!', '/').Replace("@werewolfachievementbot", ""))
                    {
                        case "/startgame":
                        case "/startchaos":
                            if (games.ContainsKey(msg.Chat.Id))
                            {
                                client.SendTextMessageAsync(msg.Chat.Id, "It seems there is already a game running in here! Stop that before you start a new one!");
                            }
                            else
                            {
                                Task<Message> t = client.SendTextMessageAsync(msg.Chat.Id, "Initializing new game...");
                                t.Wait();
                                var gamemessage = t.Result;
                                var gameplayers = new List<User>();
                                var game = new Game(client, msg.Chat.Id)
                                {
                                    players = gameplayers,
                                    gamestate = Game.state.Joining,
                                    pinmessage = gamemessage
                                };

                                games.Add(msg.Chat.Id, game);

                                games[msg.Chat.Id].AddPlayer(msg.From);
                            }
                            return;

                        case "/join":
                            if(games.ContainsKey(msg.Chat.Id) && games[msg.Chat.Id].gamestate == Game.state.Joining)
                            {
                                if (!games[msg.Chat.Id].AddPlayer(msg.From))
                                {
                                    client.SendTextMessageAsync(msg.Chat.Id, "Failed to add " + msg.From.FirstName + " to the players!").Wait();
                                }
                            }
                            else
                            {
                                client.SendTextMessageAsync(msg.Chat.Id, "It seems there is no game running in your group, or it can't be joined at the moment.").Wait();
                            }
                            return;

                        case "/flee":
                            if(games.ContainsKey(msg.Chat.Id) && games[msg.Chat.Id].gamestate == Game.state.Joining)
                            {
                                if(!games[msg.Chat.Id].RemovePlayer(msg.From))
                                {
                                    client.SendTextMessageAsync(msg.Chat.Id, "Failed to remove " + msg.From.FirstName + " from the players!").Wait();
                                }
                            }
                            else
                            {
                                client.SendTextMessageAsync(msg.Chat.Id, "It seems there is no game running in your group, or it can't be joined at the moment.").Wait();
                            }
                            return;

                        case "/dead":
                            if(games.ContainsKey(msg.Chat.Id) && games[msg.Chat.Id].gamestate == Game.state.Running)
                            {
                                if(!games[msg.Chat.Id].RemovePlayer(msg.From))
                                {
                                    client.SendTextMessageAsync(msg.Chat.Id, "Failed to remove dead player " + msg.From.FirstName + "...").Wait(); ;
                                }
                            }
                            else
                            {
                                client.SendTextMessageAsync(msg.Chat.Id, "It seems there is no running game at the moment, so no player can be dead!").Wait();
                            }
                            return;
                        case "/addalias":
                            client.SendTextMessageAsync(msg.Chat.Id,
                                "You need to write an alias behind this in the following format:\n"
                                + "Alias - Role").Wait();
                            return;
                    }
                    #endregion

                    #region addalias
                    if (text.StartsWith("/addalias "))
                    {
                        if (adminIds.Contains(msg.From.Id))
                            {
                            string args = text.Substring(10);
                            if (args.Split('-').Length != 2)
                            {
                                client.SendTextMessageAsync(msg.Chat.Id, "Wrong format. Use this:\nAlias - Role").Wait();
                            }
                            else
                            {
                                roleAliases.Add(args.Split('-')[0], args.Split('-')[1]);
                                writeAliasesFile();
                                client.SendTextMessageAsync(msg.Chat.Id, "Sucessfully added alias", 
                                    replyToMessageId: msg.MessageId).Wait();
                            }
                        }
                        else
                        {
                            client.SendTextMessageAsync(
                                msg.Chat.Id, "You are not an admin of this bot!", replyToMessageId: msg.MessageId).Wait();
                        }
                    }
                    #endregion

                    #region The heavy part: checking for each and every alias
                    if (games.ContainsKey(msg.Chat.Id))
                    {
                        if (games[msg.Chat.Id].gamestate == Game.state.Running)
                        {
                            Game g = games[msg.Chat.Id];
                            foreach (var kvp in roleAliases)
                            {
                                if ((" " + text + " ").Contains(" " + kvp.Key + " "))
                                {
                                    if (!g.role.ContainsKey(msg.From))
                                    {
                                        g.role.Add(msg.From, kvp.Value);
                                        g.UpdatePlayerlist();
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    #endregion
                }
            }
        }
        #endregion

        #region File Methods
        private void writeAliasesFile()
        {
            if (!System.IO.Directory.Exists(basePath)) System.IO.Directory.CreateDirectory(basePath);
            System.IO.File.WriteAllText(aliasesPath, JsonConvert.SerializeObject(roleAliases));
        }
        #endregion

        #region Control Methods
        #region Is running
        public bool isRunning { get { return client.IsReceiving; } }
        #endregion
        #region Start Bot
        public bool startBot()
        {
            if (System.IO.File.Exists(aliasesPath))
            {
                roleAliases = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                    System.IO.File.ReadAllText(aliasesPath));
            }
            else
            {
                if (!System.IO.Directory.Exists(basePath)) System.IO.Directory.CreateDirectory(basePath);
                System.IO.File.Create(aliasesPath);
            }
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
