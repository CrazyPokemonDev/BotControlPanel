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
    /*
     ----------------------------------------------------------------------------------------------------
     + Check possible achievements with missing player achievements and number/roles of available players
     + 
     + 
     + 
     ----------------------------------------------------------------------------------------------------
     */
    public class WerewolfAchievementsBotPlus : FlomBot
    {
        class Game
        {
            public Message pinmessage { get; set; }
            public state gamestate { get; set; }
            private TelegramBotClient client;
            public Dictionary<roles, string> rolestring = getRolestringDict();
            public long GroupId { get; }

            public Dictionary<long, string> names = new Dictionary<long, string>();
            public Dictionary<long, roles> role = new Dictionary<long, roles>();
            public Dictionary<long, bool> love = new Dictionary<long, bool>();
            

            private const string joinMessageText = "<b>Join this game!</b>\n\nPin this message and remember "
                + "to press start when the roles are assigned and the game begins. <b>DON'T PRESS START BEFORE THE ROLES ARE ASSIGNED!</b>";
            private const string runMessageText = "<b>Game running!</b>\n\nPress stop <b>ONCE THE GAME STOPPED!</b>";
            private const string stoppedMessageText = "<b>This game is finished!</b>";
            private string playerlist;

            public Game(TelegramBotClient cl, long groupid, Message pin)
            {
                client = cl;
                GroupId = groupid;
                pinmessage = pin;
                UpdatePlayerlist();
            }

            public enum state
            {
                Joining,
                Running,
                Stopped
            }

            public enum achievements
            {
                //These achievements are attainable:
                WelcomeToHell,
                WelcomeToTheAsylum,
                AlzheimersPatient,
                OHAIDER,
                SpyVsSpy,
                IHaveNoIdeaWhatImDoing,
                Enochlophobia,
                Introvert,
                Naughty,
                Dedicated,
                Obsessed,
                Masochist,
                WobbleWobble,
                Inconspicuous,
                Survivalist,
                Promiscuous,
                MasonBrother,
                DoubleShifter,
                HeyManNiceShot,
                ThatsWhyYouDontStayHome,
                DoubleKill,
                ShouldHaveKnown,
                ISeeALackOfTrust,
                SundayBloodySunday,
                ChangeSidesWorks,
                ForbiddenLove,
                TheFirstStone,
                SmartGunner,
                SpeedDating,
                EvenAStoppedClockIsRightTwiceADay,
                SoClose,
                CultistConvention,
                SelfLoving,
                ShouldveSaidSomething,
                TannerOverkill,
                CultistFodder,
                LoneWolf,
                PackHunter,
                SavedByTheBullet,
                InForTheLongHaul,
                OHSHI,
                Veteran,
                DoubleVision,
                Streetwise,
                SerialSamaritan,

                //Following achievements are unattainable:
                HeresJohnny,
                IveGotYourBack,
                BlackSheep,
                Explorer,
                Linguist,
                Developer
            }

            public bool isAchievable(achievements achv)
            {
                var gameroles = role.Values;

                int wolves = 0;
                wolves += gameroles.Count(x => x == roles.AlphaWolf);
                wolves += gameroles.Count(x => x == roles.Werewolf);
                wolves += gameroles.Count(x => x == roles.WolfCub);

                int spawnableWolves = wolves;                
                spawnableWolves += gameroles.Count(x => x == roles.WildChild);
                spawnableWolves += spawnableWolves > 0 ? gameroles.Count(x => x == roles.Cursed) : 0;
                spawnableWolves += spawnableWolves > 0 ? gameroles.Count(x => x == roles.Doppelgänger) : 0;
                spawnableWolves += gameroles.Count(x => x == roles.Traitor);
                


                switch (achv)
                {
                    case achievements.ChangeSidesWorks:
                        return gameroles.Contains(roles.Doppelgänger) || gameroles.Contains(roles.WildChild) || gameroles.Contains(roles.Traitor) || gameroles.Contains(roles.AlphaWolf) || gameroles.Contains(roles.ApprenticeSeer) || gameroles.Contains(roles.Cursed);

                    case achievements.CultistConvention:
                        return gameroles.Count(x => x != roles.AlphaWolf && x != roles.WolfCub && x != roles.Werewolf && x != roles.SerialKiller && x != roles.CultistHunter) >= 10 && gameroles.Contains(roles.Cultist);

                    case achievements.CultistFodder:
                        return gameroles.Contains(roles.Cultist) && gameroles.Contains(roles.CultistHunter);

                    case achievements.DoubleKill:
                        return gameroles.Contains(roles.SerialKiller) && gameroles.Contains(roles.Hunter);

                    case achievements.DoubleShifter:
                        return false; // TOO HARD YET, GOTTA BE FIXED!

                    case achievements.DoubleVision:
                        return gameroles.Contains(roles.ApprenticeSeer) && gameroles.Contains(roles.Doppelgänger) && (gameroles.Contains(roles.Seer) || gameroles.Contains(roles.SeerFool));

                    case achievements.EvenAStoppedClockIsRightTwiceADay:
                        return gameroles.Contains(roles.Fool) || gameroles.Contains(roles.SeerFool);

                    case achievements.ForbiddenLove:
                        return gameroles.Contains(roles.Villager) && gameroles.Contains(roles.Cupid) && spawnableWolves >= 1;

                    case achievements.HeyManNiceShot:
                        return gameroles.Contains(roles.Hunter);

                    case achievements.Inconspicuous:
                        return names.Count >= 20;

                    case achievements.ISeeALackOfTrust:
                        return gameroles.Contains(roles.Seer) || gameroles.Contains(roles.SeerFool) || gameroles.Contains(roles.ApprenticeSeer);

                    case achievements.LoneWolf:
                        return wolves == 1 && !gameroles.Contains(roles.Traitor) && names.Count >= 10;

                    case achievements.Masochist:
                        return gameroles.Contains(roles.Tanner);

                    case achievements.MasonBrother:
                        return gameroles.Count(x => x == roles.Mason) >= 2;

                    case achievements.OHSHI:
                        return (wolves >= 1 || gameroles.Contains(roles.SerialKiller)) && gameroles.Contains(roles.Cupid);

                    case achievements.PackHunter:
                        return spawnableWolves >= 7;

                    case achievements.Promiscuous:
                        return gameroles.Contains(roles.Harlot) && gameroles.Count(x => x != roles.Werewolf && x != roles.WolfCub && x != roles.AlphaWolf && x != roles.SerialKiller && x != roles.Harlot) >= 5;

                    case achievements.SavedByTheBullet:
                        return gameroles.Contains(roles.Gunner) && spawnableWolves >= 1;

                    case achievements.SelfLoving:
                        return gameroles.Contains(roles.Cupid);

                    case achievements.SerialSamaritan:
                        return gameroles.Contains(roles.SerialKiller) && spawnableWolves >= 3;

                    case achievements.ShouldHaveKnown:
                        return gameroles.Contains(roles.Beholder) && (gameroles.Contains(roles.Seer) || gameroles.Contains(roles.SeerFool));

                    case achievements.ShouldveSaidSomething:
                        return spawnableWolves >= 1 && gameroles.Contains(roles.Cupid);

                    case achievements.SmartGunner:
                        return gameroles.Contains(roles.Gunner) && (spawnableWolves >= 2 || (spawnableWolves == 1 && gameroles.Contains(roles.SerialKiller)) || gameroles.Contains(roles.Cultist));

                    case achievements.SoClose:
                        return gameroles.Contains(roles.Tanner);

                    case achievements.SpeedDating:
                        return gameroles.Contains(roles.Cupid);

                    case achievements.Streetwise:
                        return gameroles.Contains(roles.Detective) && (spawnableWolves + gameroles.Count(x => x == roles.SerialKiller || x == roles.Cultist) >= 3);

                    case achievements.SundayBloodySunday:
                        return false; // TOO HARD YET, GOTTA BE FIXED!

                    case achievements.TannerOverkill:
                        return gameroles.Contains(roles.Tanner);

                    case achievements.ThatsWhyYouDontStayHome:
                        return gameroles.Contains(roles.Harlot) && (spawnableWolves >= 1 || gameroles.Contains(roles.Cultist));

                    case achievements.WobbleWobble:
                        return gameroles.Contains(roles.Drunk);

                    default:
                        // UNATTAINABLE ONES AND ONES BOT CAN'T KNOW:
                        // AlzheimersPatient
                        // BlackSheep
                        // Dedicated
                        // Developer
                        // Enochlophobia
                        // Explorer
                        // HeresJohnny
                        // IHaveNoIdeaWhatImDoing
                        // InForTheLongHaul
                        // Introvert
                        // IveGotYourBack
                        // Linguist
                        // Naughty
                        // Obsessed
                        // OHAIDER
                        // SpyVsSpy
                        // Survivalist
                        // TheFirstStone
                        // Veteran
                        // WelcomeToHell
                        // WelcomeToTheAsylum
                        return false;
                }
            }

            public enum roles
            {
                Villager,
                Werewolf,
                Drunk,
                Seer,
                Cursed,
                Harlot,
                Beholder,
                Gunner,
                Traitor,
                GuardianAngel,
                Detective,
                ApprenticeSeer,
                Cultist,
                CultistHunter,
                WildChild,
                Fool,
                Mason,
                Doppelgänger,
                Cupid,
                Hunter,
                SerialKiller,
                Tanner,
                Mayor,
                Prince,
                Sorcerer,
                ClumsyGuy,
                Blacksmith,
                AlphaWolf,
                WolfCub,
                SeerFool, // Used if not sure whether seer or fool
                Dead,
                Unknown
            }

            public static Dictionary<roles, string> getRolestringDict()
            {
                Dictionary<roles, string> dict = new Dictionary<roles, string>();
                dict.Add(roles.AlphaWolf, "Alpha Wolf 🐺⚡️");
                dict.Add(roles.ApprenticeSeer, "App Seer 🙇");
                dict.Add(roles.Beholder, "Beholder 👁");
                dict.Add(roles.Blacksmith, "Blacksmith ⚒");
                dict.Add(roles.ClumsyGuy, "Clumsy Guy 🤕");
                dict.Add(roles.Cultist, "Cultist 👤");
                dict.Add(roles.CultistHunter, "Cult Hunter 💂");
                dict.Add(roles.Cupid, "Cupid 🏹");
                dict.Add(roles.Cursed, "Cursed 😾");
                dict.Add(roles.Detective, "Detective 🕵️");
                dict.Add(roles.Doppelgänger, "Doppelgänger 🎭");
                dict.Add(roles.Drunk, "Drunk 🍻");
                dict.Add(roles.Fool, "Fool 🃏");
                dict.Add(roles.GuardianAngel, "Guardian Angel 👼");
                dict.Add(roles.Gunner, "Gunner 🔫");
                dict.Add(roles.Harlot, "Harlot 💋");
                dict.Add(roles.Hunter, "Hunter 🎯");
                dict.Add(roles.Mason, "Mason 👷");
                dict.Add(roles.Mayor, "Mayor 🎖");
                dict.Add(roles.Prince, "Prince 👑");
                dict.Add(roles.Seer, "Seer 👳");
                dict.Add(roles.SerialKiller, "Serial Killer 🔪");
                dict.Add(roles.Sorcerer, "Sorcerer 🔮");
                dict.Add(roles.Tanner, "Tanner 👺");
                dict.Add(roles.Traitor, "Traitor 🖕");
                dict.Add(roles.Villager, "Villager 👱");
                dict.Add(roles.Werewolf, "Werewolf 🐺");
                dict.Add(roles.WildChild, "Wild Child 👶");
                dict.Add(roles.WolfCub, "Wolf Cub 🐶");
                dict.Add(roles.SeerFool, "Seer OR Fool 👳🃏");

                dict.Add(roles.Dead, "DEAD 💀");
                dict.Add(roles.Unknown, "No role detected yet");
                return dict;
            }

            public static Dictionary<achievements, string> getAchvDict()
            {
                var dict = new Dictionary<achievements, string>();
                dict.Add(achievements.AlzheimersPatient, "Alzheimer's Patient");
                dict.Add(achievements.BlackSheep, "Black Sheep");
                dict.Add(achievements.ChangeSidesWorks, "Change Sides Works");
                dict.Add(achievements.CultistConvention, "Cultist Convention");
                dict.Add(achievements.CultistFodder, "Cultist Fodder");
                dict.Add(achievements.Dedicated, "Dedicated");
                dict.Add(achievements.Developer, "Developer");
                dict.Add(achievements.DoubleKill, "Double Kill");
                dict.Add(achievements.DoubleShifter, "Double Shifter");
                dict.Add(achievements.DoubleVision, "Double Vision");
                dict.Add(achievements.Enochlophobia, "Enochlophobia");
                dict.Add(achievements.EvenAStoppedClockIsRightTwiceADay, "Even A Stopped Clock Is Right Twice A Day");
                dict.Add(achievements.Explorer, "Explorer");
                dict.Add(achievements.ForbiddenLove, "Forbidden Love");
                dict.Add(achievements.HeresJohnny, "Here's Johnny");
                dict.Add(achievements.HeyManNiceShot, "Hey Man, Nice Shot!");
                dict.Add(achievements.IHaveNoIdeaWhatImDoing, "I Have No Idea What I'm Doing");
                dict.Add(achievements.Inconspicuous, "Inconspicuous");
                dict.Add(achievements.InForTheLongHaul, "In For The Long Haul");
                dict.Add(achievements.Introvert, "Introvert");
                dict.Add(achievements.ISeeALackOfTrust, "I See A Lack Of Trust");
                dict.Add(achievements.IveGotYourBack, "I've Got Your Back");
                dict.Add(achievements.Linguist, "Linguist");
                dict.Add(achievements.LoneWolf, "Lone Wolf");
                dict.Add(achievements.Masochist, "Masochist");
                dict.Add(achievements.MasonBrother, "Mason Brother");
                dict.Add(achievements.Naughty, "Naughty");
                dict.Add(achievements.Obsessed, "Obsessed");
                dict.Add(achievements.OHAIDER, "O HAI DER");
                dict.Add(achievements.OHSHI, "OH SHI-");
                dict.Add(achievements.PackHunter, "Pack Hunter");
                dict.Add(achievements.Promiscuous, "Promiscuous");
                dict.Add(achievements.SavedByTheBullet, "Saved By The Bullet");
                dict.Add(achievements.SelfLoving, "Self Loving");
                dict.Add(achievements.SerialSamaritan, "Serial Samaritan");
                dict.Add(achievements.ShouldHaveKnown, "Should Have Known");
                dict.Add(achievements.ShouldveSaidSomething, "Should've Said Something");
                dict.Add(achievements.SmartGunner, "Smart Gunner");
                dict.Add(achievements.SoClose, "So Close");
                dict.Add(achievements.SpeedDating, "Speed Dating");
                dict.Add(achievements.SpyVsSpy, "Spy Vs Spy");
                dict.Add(achievements.Streetwise, "Streetwise");
                dict.Add(achievements.SundayBloodySunday, "Sunday Bloody Sunday");
                dict.Add(achievements.Survivalist, "Survivalist");
                dict.Add(achievements.TannerOverkill, "Tanner Overkill");
                dict.Add(achievements.ThatsWhyYouDontStayHome, "That's Why You Don't Stay Home");
                dict.Add(achievements.TheFirstStone, "The First Stone");
                dict.Add(achievements.Veteran, "Veteran");
                dict.Add(achievements.WelcomeToHell, "Welcome To Hell");
                dict.Add(achievements.WelcomeToTheAsylum, "Welcome To The Asylum");
                dict.Add(achievements.WobbleWobble, "Wobble Wobble");
                return dict;
            }

            public bool AddPlayer(User newplayer)
            {
                if (!names.ContainsKey(newplayer.Id) && gamestate == state.Joining)
                {
                    names.Add(newplayer.Id, newplayer.FirstName.Length > 15 ? newplayer.FirstName.Remove(15) : newplayer.FirstName);
                    role.Add(newplayer.Id, roles.Unknown);
                    love.Add(newplayer.Id, false);
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
            }

            public bool RemovePlayer(User oldplayer)
            {
                if(names.ContainsKey(oldplayer.Id))
                {
                    names.Remove(oldplayer.Id);
                    role.Remove(oldplayer.Id);
                    love.Remove(oldplayer.Id);
                    UpdatePlayerlist();
                    return true;
                }
                return false;
            }

            public void UpdatePlayerlist()
            {
                playerlist = gamestate == state.Running
                    ? $"<b>LYNCHORDER ({names.Keys.Count(x => role[x] != roles.Dead)} of {names.Keys.Count}):</b>\n"
                    : $"<b>Players ({names.Keys.Count}):</b>\n";

                foreach(var p in names.Keys.Where(x => role[x] != roles.Dead))
                {
                    if(gamestate == state.Joining) playerlist += names[p] + "\n";
                    else if (gamestate == state.Running)
                    {
                        if (role[p] != roles.Unknown) playerlist += "<b>" + names[p] + "</b>: " + rolestring[role[p]];
                        else playerlist += "<b>" + names[p] + "</b>: " + rolestring[roles.Unknown];

                        if (love[p]) playerlist += "❤️";
                        playerlist += "\n";
                    }
                }
                
                if (gamestate == state.Running)
                {
                    playerlist += "\n\n<b>DEAD PLAYERS 💀:</b>";

                    foreach (var p in names.Keys.Where(x => role[x] == roles.Dead))
                    {
                        playerlist += "\n" + names[p];
                    }
                }

                if (gamestate == state.Running)
                    client.EditMessageTextAsync(pinmessage.Chat.Id, pinmessage.MessageId, runMessageText
                        + "\n\n" + playerlist, parseMode: ParseMode.Html,
                        replyMarkup: InlineKeyboardStop.Get(GroupId)).Wait();
                else if (gamestate == state.Joining)
                    client.EditMessageTextAsync(pinmessage.Chat.Id, pinmessage.MessageId, joinMessageText
                        + "\n\n" + playerlist, parseMode: ParseMode.Html,
                        replyMarkup: InlineKeyboardStart.Get(GroupId)).Wait();
                else if (gamestate == state.Stopped)
                    client.EditMessageTextAsync(pinmessage.Chat.Id, pinmessage.MessageId, stoppedMessageText,
                        parseMode: ParseMode.Html).Wait();
            }
        }

        private Dictionary<long, Game> games = new Dictionary<long, Game>();
        private Dictionary<string, Game.roles> roleAliases = new Dictionary<string, Game.roles>();
        List<long> justCalledStop = new List<long>();
        public override string Name { get; } = "Werewolf Achievements Bot";
        private const string basePath = "C:\\Olfi01\\BotControlPanel\\AchievementsBot\\";
        private const string aliasesPath = basePath + "aliases.dict";
        private const string version = "3.3";
        private readonly List<long> allowedgroups = new List<long>() { -1001070844778, -1001078561643 };
        public List<long> disaledgroups = new List<long>();
        private readonly List<long> adminIds = new List<long>() { 267376056, 295152997 };
        private readonly List<string> defaultAliases = new List<string>()
        {
            "alphawolf",
            "apprenticeseer",
            "beholder",
            "blacksmith",
            "clumsyguy",
            "cultist",
            "cultisthunter",
            "cupid",
            "cursed",
            "detective",
            "doppelgänger",
            "drunk",
            "fool",
            "guardianangel",
            "gunner",
            "harlot",
            "hunter",
            "mason",
            "mayor",
            "prince",
            "seer",
            "seerfool",
            "serialkiller",
            "sorcerer",
            "tanner",
            "traitor",
            "villager",
            "werewolf",
            "wildchild",
            "wolfcub"
        };
        public WerewolfAchievementsBotPlus(string token) : base(token)
        {
            try
            {
                client.OnCallbackQuery += Client_OnCallbackQuery;
            }
            catch { }
        }
        private void Client_OnCallbackQuery(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            try
            {
                string data = e.CallbackQuery.Data;
                if (data.StartsWith("start_"))
                {
                    long id = Convert.ToInt64(data.Substring(6));
                    if (games.ContainsKey(id))
                    {
                        if (games[id].names.Count >= 5 || id == allowedgroups[0]) // player limit disabled for test group
                        {
                            games[id].Start();
                            games[id].UpdatePlayerlist();
                            client.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "Game is now considered running.").Wait();
                            client.SendTextMessageAsync(id, $"<b>{e.CallbackQuery.From.FirstName}</b> has considered the game started!", parseMode: ParseMode.Html).Wait();
                        }
                        else
                        {
                            client.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "Too less players to start the game!").Wait();
                            client.SendTextMessageAsync(id, $"<b>{e.CallbackQuery.From.FirstName}</b> tried to start the game but there are too less players yet", parseMode: ParseMode.Html);
                        }
                    }
                    else
                    {
                        client.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "Did not find that game.").Wait();
                    }
                }
                else if (data.StartsWith("stop_"))
                {
                    long id = Convert.ToInt64(data.Substring(5));
                    if (games.ContainsKey(id))
                    {
                        if (justCalledStop.Contains(e.CallbackQuery.From.Id))
                        {
                            games[id].Stop();
                            games[id].UpdatePlayerlist();
                            games.Remove(id);
                            client.AnswerCallbackQueryAsync(e.CallbackQuery.Id,
                                "The game is now considered stopped.").Wait();
                            client.SendTextMessageAsync(id, $"<b>{e.CallbackQuery.From.FirstName}</b> has considered the game stopped!", parseMode: ParseMode.Html);
                            justCalledStop.Remove(e.CallbackQuery.From.Id);
                        }
                        else
                        {
                            justCalledStop.Add(e.CallbackQuery.From.Id);
                            client.AnswerCallbackQueryAsync(e.CallbackQuery.Id,
                                "Press this button again if the game really has stopped already.").Wait();
                            Timer t = new Timer(new TimerCallback
                                (
                                    delegate
                                    {
                                        justCalledStop.Remove(e.CallbackQuery.From.Id);
                                    }
                                ), null, 10 * 1000, Timeout.Infinite);
                        }
                    }
                    else
                    {
                        client.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "Did not find that game.").Wait();
                    }
                }
            }
            catch (Exception ex)
            {
                client.SendTextMessageAsync(adminIds[0], "Error in achievements callback: " + ex.Message
                    + "\n" + ex.StackTrace);
                client.SendTextMessageAsync(adminIds[1], "Error in achievements callback: " + ex.Message
                    + "\n" + ex.StackTrace);
            }
        }

        private Message ReplyToMessage(string text, Update u)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                if (u.Message != null)
                {
                    var chatid = u.Message.Chat.Id;
                    var messageid = u.Message.MessageId;

                    try
                    {
                        var task = client.SendTextMessageAsync(chatid, text, replyToMessageId: messageid, parseMode: ParseMode.Html);
                        task.Wait();
                        var msg = task.Result;
                        return msg;
                    }
                    catch (Exception e)
                    {
                        client.SendTextMessageAsync(adminIds[0], e.Message).Wait();
                        client.SendTextMessageAsync(adminIds[1], e.Message).Wait();
                        client.SendTextMessageAsync(u.Message.Chat.Id, "Tried to send something to this chat but failed! The devs were informed! Sorry!").Wait();
                        return null;
                    }
                }
            }
            return null;
        }

        protected override void Client_OnUpdate(object sender, Telegram.Bot.Args.UpdateEventArgs e)
        {
            try
            {
                if (e.Update.Type == UpdateType.MessageUpdate && e.Update.Message.Chat.Type != ChatType.Private && !allowedgroups.Contains(e.Update.Message.Chat.Id))
                {
                    client.LeaveChatAsync(e.Update.Message.Chat.Id).Wait();
                    return;
                }
                if (e.Update.Type == UpdateType.MessageUpdate)
                {
                    if (e.Update.Message.Type == MessageType.TextMessage &&
                        (e.Update.Message.Chat.Type == ChatType.Group ||
                        e.Update.Message.Chat.Type == ChatType.Supergroup))
                    {
                        var text = e.Update.Message.Text;
                        var msg = e.Update.Message;
                        var u = e.Update;

                        if (text.ToLower().Replace("@werewolfwolfachievementbot", "") == "/togglegroup" && adminIds.Contains(msg.From.Id))
                        {
                            string word;

                            if (disaledgroups.Contains(msg.Chat.Id))
                            {
                                disaledgroups.Remove(msg.Chat.Id);
                                word = "enabled";
                            }
                            else
                            {
                                disaledgroups.Add(msg.Chat.Id);
                                word = "disabled";
                            }
                            ReplyToMessage("<b>The bot is now " + word + " for this group!</b>", u);
                        }
                        if(text.Contains(' ') && text.Split(' ')[0].ToLower().Replace("@werewolfwolfachievementbot", "") == "/announce" && adminIds.Contains(msg.From.Id))
                        {
                            client.SendTextMessageAsync(allowedgroups[1], text.Remove(0, text.IndexOf(' ')), parseMode: ParseMode.Html);
                            ReplyToMessage("Successfully announced!", u);
                        }

                        if (!disaledgroups.Contains(msg.Chat.Id))
                        {
                            switch (text.ToLower().Replace("@werewolfbot", "").Replace('!', '/').Replace("@werewolfwolfachievementbot", ""))
                            {
                                case "/startgame":
                                case "/startchaos":
                                    if (games.ContainsKey(msg.Chat.Id) && games[msg.Chat.Id].gamestate == Game.state.Running)
                                    {
                                        ReplyToMessage("It seems there is already a game running in here! Stop that before you start a new one!", u);
                                    }
                                    else
                                    {
                                        if (games.ContainsKey(msg.Chat.Id))
                                        {

                                            if (!games[msg.Chat.Id].AddPlayer(msg.From))
                                            {
                                                ReplyToMessage("Failed to add <b>" + msg.From.FirstName + "</b> to the players!", u);
                                            }
                                        }
                                        else
                                        {
                                            Message m = ReplyToMessage("Initializing new game...", u);
                                            if (m == null) return;
                                            var game = new Game(client, msg.Chat.Id, m);
                                            games.Add(msg.Chat.Id, game);
                                            games[msg.Chat.Id].AddPlayer(msg.From);
                                        }
                                    }
                                    return;

                                case "/join":
                                    if (games.ContainsKey(msg.Chat.Id) && games[msg.Chat.Id].gamestate == Game.state.Joining)
                                    {
                                        if (!games[msg.Chat.Id].AddPlayer(msg.From))
                                        {
                                            ReplyToMessage("Failed to add <b>" + msg.From.FirstName + "</b> to the players!", u);
                                        }
                                    }
                                    else
                                    {
                                        ReplyToMessage("It seems there is no game running in your group, or it can't be joined at the moment.", u);
                                    }
                                    return;

                                case "/stopgame":
                                    if (games.ContainsKey(msg.Chat.Id))
                                    {
                                        if (justCalledStop.Contains(msg.From.Id))
                                        {
                                            games[msg.Chat.Id].Stop();
                                            games[msg.Chat.Id].UpdatePlayerlist();
                                            games.Remove(msg.Chat.Id);
                                            ReplyToMessage($"<b>{msg.From.FirstName}</b> has considered the game stopped!", u);
                                            justCalledStop.Remove(msg.From.Id);
                                        }
                                        else
                                        {
                                            justCalledStop.Add(msg.From.Id);
                                            ReplyToMessage("Use this command again to confirm you want to stop the game.", u);
                                            Timer t = new Timer(new TimerCallback
                                                (
                                                    delegate
                                                    {
                                                        justCalledStop.Remove(msg.From.Id);
                                                    }
                                                ), null, 10 * 1000, Timeout.Infinite);
                                        }
                                    }
                                    return;

                                case "/flee":
                                case "/dead":
                                    if (games.ContainsKey(msg.Chat.Id))
                                    {
                                        Game g = games[msg.Chat.Id];

                                        User dead = msg.ReplyToMessage != null && g.names.Keys.Contains(msg.ReplyToMessage.From.Id)
                                                ? msg.ReplyToMessage.From
                                                : (
                                                    g.names.Keys.Contains(msg.From.Id)
                                                        ? msg.From
                                                        : null
                                                  );
                                        if (dead == null || !g.names.ContainsKey(dead.Id)) return;

                                        switch (g.gamestate)
                                        {
                                            case Game.state.Joining:
                                                if (!g.RemovePlayer(dead))
                                                {
                                                    ReplyToMessage("Failed to remove player <b>" + dead.FirstName + "</b> from the game.", u);
                                                }
                                                break;

                                            case Game.state.Running:
                                                g.role.Remove(dead.Id);
                                                g.role.Add(dead.Id, Game.roles.Dead);
                                                g.UpdatePlayerlist();
                                                break;
                                        }

                                    }
                                    return;
                                case "/addalias":
                                    if (adminIds.Contains(msg.From.Id)) ReplyToMessage("You need to write an alias behind this in the following format:\n Alias Role", u);
                                    else ReplyToMessage("You are not a bot admin!", u);
                                    return;
                                case "/ping":
                                    ReplyToMessage("<b>PENG!</b>", u);
                                    return;

                                case "/version":
                                    ReplyToMessage($"Werewolf Achievements Manager.\n <b>Version {version}.</b>", u);
                                    return;

                                case "/listalias":
                                    var rolestrings = Game.getRolestringDict();
                                    var listalias = "<b>ALL ALIASSES OF ALL ROLES:</b>\n";
                                    foreach (var thisrole in rolestrings.Keys)
                                    {
                                        listalias += "\n\n<b>" + rolestrings[thisrole] + "</b>";

                                        foreach (var alias in roleAliases.Where(x => x.Value == thisrole))
                                        {
                                            listalias += "\n" + alias.Key;
                                        }
                                    }
                                    ReplyToMessage(listalias, u);
                                    return;

                                case "/achv":
                                    if (games.ContainsKey(msg.Chat.Id))
                                    {
                                        Game g = games[msg.Chat.Id];
                                        string possible = "<b>POSSIBLE ACHIEVEMENTS:</b>\n";

                                        foreach (var achv in Game.getAchvDict().Keys)
                                        {
                                            possible += g.isAchievable(achv)
                                                ? Game.getAchvDict()[achv] + "\n"
                                                : "";
                                        }
                                        ReplyToMessage(possible, u);
                                    }
                                    return;

                                case "/love":
                                    if(games.ContainsKey(msg.Chat.Id))
                                    {
                                        Game g = games[msg.Chat.Id];

                                        User lover = msg.ReplyToMessage != null && g.names.Keys.Contains(msg.ReplyToMessage.From.Id)
                                                ? msg.ReplyToMessage.From
                                                : (
                                                    g.names.Keys.Contains(msg.From.Id)
                                                        ? msg.From
                                                        : null
                                                  );
                                        if (lover == null || !g.names.ContainsKey(lover.Id)) return;

                                        g.love[lover.Id] = !g.love[lover.Id] ? true : false;
                                        g.UpdatePlayerlist();
                                    }
                                    return;
                            }

                            if (text.StartsWith("/addalias"))
                            {
                                if (adminIds.Contains(msg.From.Id))
                                {
                                    if (text.Split(' ').Count() == 3)
                                    {
                                        string alias = text.Split(' ')[1].ToLower();
                                        string roleS = text.Split(' ')[2];
                                        Game.roles role = GetRoleByAlias(roleS);
                                        if (role == Game.roles.Unknown)
                                        {
                                            ReplyToMessage("The role was not recognized! Adding alias failed!", u);
                                        }
                                        else if (!roleAliases.Keys.Contains(alias))
                                        {
                                            roleAliases.Add(alias, role);
                                            writeAliasesFile();
                                            ReplyToMessage($"Alias <i>{alias}</i> successfully added for role <b>{role}</b>.", u);
                                        }
                                        else
                                        {
                                            roleAliases[alias] = role;
                                            writeAliasesFile();
                                            ReplyToMessage($"Alias <i>{alias}</i> successfully updated for role <b>{role}</b>.", u);
                                        }

                                    }
                                    else ReplyToMessage("Failed: Wrong command syntax. Syntax: /addalias <alias>", u);
                                }
                                else ReplyToMessage("You are not a bot admin!", u);

                            }

                            if (text.StartsWith("/delalias"))
                            {
                                if (adminIds.Contains(msg.From.Id))
                                {
                                    if (text.Split(' ').Count() == 2)
                                    {
                                        string alias = text.Split(' ')[1].ToLower();

                                        if (roleAliases.ContainsKey(alias))
                                        {
                                            roleAliases.Remove(alias);
                                            writeAliasesFile();
                                            ReplyToMessage($"Alias <i>{alias}</i> was successfully removed!", u);
                                        }
                                        else
                                        {
                                            ReplyToMessage($"Couldn't find Alias <i>{alias}</i>!", u);
                                        }
                                    }
                                    else ReplyToMessage("Failed: Wrong command syntax. Syntax: /delalias <alias>", u);
                                }
                                else ReplyToMessage("You are not a bot admin!", u);
                            }

                            if (games.ContainsKey(msg.Chat.Id))
                            {
                                if (games[msg.Chat.Id].gamestate == Game.state.Running)
                                {
                                    Game g = games[msg.Chat.Id];

                                    long player = 0;
                                    if (msg.ReplyToMessage != null)
                                    {
                                        if (g.names.Keys.Contains(msg.ReplyToMessage.From.Id)) player = msg.ReplyToMessage.From.Id;
                                    }
                                    else if (g.names.Keys.Contains(msg.From.Id))
                                    {
                                        player = msg.From.Id;
                                    }

                                    if (player == 0) return;

                                    List<string> Keys = roleAliases.Keys.ToList();
                                    foreach (string s in defaultAliases)
                                    {
                                        Keys.Add(s);
                                    }

                                    if (g.role[player] == Game.roles.Unknown && Keys.Contains(text.ToLower()))
                                    {
                                        var role = GetRoleByAlias(text.ToLower());
                                        if (role != Game.roles.Unknown)
                                        {
                                            g.role.Remove(player);
                                            g.role.Add(player, GetRoleByAlias(text.ToLower()));
                                            g.UpdatePlayerlist();
                                        }
                                    }
                                    if (text.ToLower().StartsWith("now ") && Keys.Contains(text.ToLower().Substring(4)))
                                    {
                                        var role = GetRoleByAlias(text.ToLower().Substring(4));
                                        if (role != Game.roles.Unknown)
                                        {
                                            var oldRole = g.role[player];
                                            if (oldRole != role)
                                            {
                                                g.role[player] = role;
                                                g.UpdatePlayerlist();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                client.SendTextMessageAsync(adminIds[0], "Error in Achievements Bot: " +
                    ex.InnerException + "\n" + ex.Message + "\n" + ex.StackTrace).Wait();
                client.SendTextMessageAsync(adminIds[1], "Error in Achievements Bot: " +
                    ex.InnerException + "\n" + ex.Message + "\n" + ex.StackTrace).Wait();
            }
        }


        private Game.roles GetRoleByAlias(string alias)
        {
            if (roleAliases.ContainsKey(alias)) return roleAliases[alias];
            else
            {
                switch (alias)
                {
                    case "alphawolf":
                        return Game.roles.AlphaWolf;

                    case "apprenticeseer":
                        return Game.roles.ApprenticeSeer;

                    case "beholder":
                        return Game.roles.Beholder;

                    case "blacksmith":
                        return Game.roles.Blacksmith;

                    case "clumsyguy":
                        return Game.roles.ClumsyGuy;

                    case "cultist":
                        return Game.roles.Cultist;

                    case "cultisthunter":
                        return Game.roles.CultistHunter;

                    case "cupid":
                        return Game.roles.Cupid;

                    case "cursed":
                        return Game.roles.Cursed;

                    case "detective":
                        return Game.roles.Detective;

                    case "doppelgänger":
                        return Game.roles.Doppelgänger;

                    case "drunk":
                        return Game.roles.Drunk;

                    case "fool":
                        return Game.roles.Fool;

                    case "guardianangel":
                        return Game.roles.GuardianAngel;

                    case "gunner":
                        return Game.roles.Gunner;

                    case "harlot":
                        return Game.roles.Harlot;

                    case "hunter":
                        return Game.roles.Hunter;

                    case "mason":
                        return Game.roles.Mason;

                    case "mayor":
                        return Game.roles.Mayor;

                    case "prince":
                        return Game.roles.Prince;

                    case "seer":
                        return Game.roles.Seer;

                    case "seerfool":
                        return Game.roles.SeerFool;

                    case "serialkiller":
                        return Game.roles.SerialKiller;

                    case "sorcerer":
                        return Game.roles.Sorcerer;

                    case "tanner":
                        return Game.roles.Tanner;

                    case "traitor":
                        return Game.roles.Traitor;

                    case "villager":
                        return Game.roles.Villager;

                    case "werewolf":
                        return Game.roles.Werewolf;

                    case "wildchild":
                        return Game.roles.WildChild;

                    case "wolfcub":
                        return Game.roles.WolfCub;

                    default:
                        return Game.roles.Unknown;
                }
            }
        }


        private void writeAliasesFile()
        {
            if (!System.IO.Directory.Exists(basePath)) System.IO.Directory.CreateDirectory(basePath);
            System.IO.File.WriteAllText(aliasesPath, JsonConvert.SerializeObject(roleAliases));
        }

        private void getAliasesFromFile()
        {
            if (System.IO.File.Exists(aliasesPath))
            {
                roleAliases = JsonConvert.DeserializeObject<Dictionary<string, Game.roles>>(
                    System.IO.File.ReadAllText(aliasesPath));
            }
            else
            {
                if (!System.IO.Directory.Exists(basePath)) System.IO.Directory.CreateDirectory(basePath);
                System.IO.File.Create(aliasesPath);
            }
            if (roleAliases == null) roleAliases = new Dictionary<string, Game.roles>();
        }


        public override bool StartBot()
        {
            getAliasesFromFile();
            return base.StartBot();
        }

    }
}
