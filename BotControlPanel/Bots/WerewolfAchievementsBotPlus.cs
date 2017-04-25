using BotControlPanel.Bots.AchBotInlineKeyboards;
using Newtonsoft.Json;
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
            private Dictionary<User, string> role = new Dictionary<User, string>();


            private const string joinMessageText = "*Join this game!*\n\nPin this message and remember to press start as soon as the game starts.";
            private string playerlist;
            #endregion

            public Game(TelegramBotClient cl)
            {
                client = cl;
                UpdatePlayerlist();
            }

            public enum state
            {
                Joining,
                Running
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

            private void UpdatePlayerlist()
            {
                playerlist = "*Players:*";

                foreach(var p in players)
                {
                    playerlist += "\n" + p.FirstName;
                }
                client.EditMessageTextAsync(pinmessage.Chat.Id, pinmessage.MessageId, joinMessageText + "\n\n" + playerlist, parseMode: ParseMode.Markdown).Wait();
            }
        }
        #endregion

        #region Variables
        private TelegramBotClient client;
        private Dictionary<long, Game> games = new Dictionary<long, Game>();
        private Dictionary<string, string> roleAliases = new Dictionary<string, string>();
        #endregion
        #region Constants
        private const string basePath = "C:\\Olfi01\\BotControlPanel\\AchievementsBot\\";
        private const string aliasesPath = basePath + "aliases.dict";
        #endregion
        #region Constructor
        public WerewolfAchievementsBotPlus(string token)
        {
            client = new TelegramBotClient(token);
            client.OnUpdate += Client_OnUpdate;
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
                                var game = new Game(client)
                                {
                                    players = gameplayers,
                                    gamestate = Game.state.Joining,
                                    pinmessage = gamemessage
                                };

                                games.Add(msg.Chat.Id, game);

                                games[msg.Chat.Id].AddPlayer(msg.From);
                            }
                            break;

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
                            break;

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
                            break;

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
                            break;
                        case "/addalias":
                            client.SendTextMessageAsync(msg.Chat.Id,
                                "You need to write an alias behind this in the following format:\n"
                                + "Alias - Role").Wait();
                            break;
                    }
                    #endregion

                    if (text.StartsWith("/addalias "))
                    {
                        
                    }
                }
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
