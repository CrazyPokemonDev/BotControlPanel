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
            public string lynchorder = "";
            

            private const string joinMessageText = "<b>Join this game!</b>\n\nJoin using the button and remember to use /addplayer after joining. Click the start button below as soon as the roles are assigned and the game begins. <b>DON'T PRESS START BEFORE THE ROLES ARE ASSIGNED!</b>";
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
                Developer,

                
                // NEW ACHIEVEMENTS
                NoSorcery,
                CultistTracker,
                ImNotDrunBurppp,
                WuffieCult,
                DidYouGuardYourself,
                SpoiledRichBrat,
                ThreeLittleWolvesAndABigBadPig,
                President,
                IHelped,
                ItWasABusyNight,
                
                
                
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

                int visitcount = 0;
                visitcount += spawnableWolves >= 1 ? 1 : 0;
                visitcount += gameroles.Contains(roles.SerialKiller) ? 1 : 0;
                visitcount += gameroles.Contains(roles.Cultist) ? 1 : 0;
                visitcount += gameroles.Contains(roles.CultistHunter) ? 1 : 0;
                visitcount += gameroles.Contains(roles.Harlot) ? 1 : 0;
                visitcount += gameroles.Contains(roles.GuardianAngel) ? 1 : 0;

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
                        return gameroles.Contains(roles.Detective) && (spawnableWolves + gameroles.Count(x => x == roles.SerialKiller || x == roles.Cultist) >= 4);

                    case achievements.SundayBloodySunday:
                        return false; // TOO HARD YET, GOTTA BE FIXED!

                    case achievements.TannerOverkill:
                        return gameroles.Contains(roles.Tanner);

                    case achievements.ThatsWhyYouDontStayHome:
                        return gameroles.Contains(roles.Harlot) && (spawnableWolves >= 1 || gameroles.Contains(roles.Cultist));

                    case achievements.WobbleWobble:
                        return gameroles.Contains(roles.Drunk) && gameroles.Count >= 10;

                    
                    // NEW ACHIEVEMENTS
                    case achievements.NoSorcery:
                        return spawnableWolves >= 1 && gameroles.Contains(roles.Sorcerer);

                    case achievements.CultistTracker:
                        return gameroles.Contains(roles.CultistHunter) && gameroles.Contains(roles.Cultist);

                    case achievements.ImNotDrunBurppp:
                        return gameroles.Contains(roles.ClumsyGuy);

                    case achievements.WuffieCult:
                        return gameroles.Contains(roles.AlphaWolf);

                    case achievements.DidYouGuardYourself:
                        return gameroles.Contains(roles.GuardianAngel) && spawnableWolves >= 1;

                    case achievements.SpoiledRichBrat:
                        return gameroles.Contains(roles.Prince);

                    case achievements.ThreeLittleWolvesAndABigBadPig:
                        return gameroles.Contains(roles.Sorcerer) && spawnableWolves >= 3;

                    case achievements.President:
                        return gameroles.Contains(roles.Mayor);
                    
                    case achievements.IHelped:
                        return gameroles.Contains(roles.WolfCub) && spawnableWolves >= 2;

                    case achievements.ItWasABusyNight:
                        return visitcount >= 3;
                      
                     

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

                
                // NEW ACHIEVEMENTS
                dict.Add(achievements.NoSorcery, "No Sorcery!");
                dict.Add(achievements.WuffieCult, "Wuffie-Cult");
                dict.Add(achievements.ThreeLittleWolvesAndABigBadPig, "Three Little Wolves And A Big Bad Pig");
                dict.Add(achievements.IHelped, "I Helped!");
                dict.Add(achievements.CultistTracker, "Cultist Tracker");
                dict.Add(achievements.ImNotDrunBurppp, "I'M NOT DRUN-- *BURPPP*");
                dict.Add(achievements.DidYouGuardYourself, "Did You Guard Yourself?");
                dict.Add(achievements.SpoiledRichBrat, "Spoiled Rich Brat");
                dict.Add(achievements.President, "President");
                dict.Add(achievements.ItWasABusyNight, "It Was A Busy Night!");
                
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

                        if (love[p]) playerlist += " ❤️";
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
        } // End of class Group

        public override string Name { get; } = "Werewolf Achievements Bot";
        private const string basePath = "C:\\Olfi01\\BotControlPanel\\AchievementsBot\\";
        private const string aliasesPath = basePath + "aliases.dict";
        private const string version = "3.3.9";
        private readonly DateTime starttime = DateTime.UtcNow;

        private Dictionary<long, Game> games = new Dictionary<long, Game>();
        private Dictionary<long, int> pinmessages = new Dictionary<long, int>();
        private Dictionary<string, Game.roles> roleAliases = new Dictionary<string, Game.roles>();

        List<long> justCalledStop = new List<long>();
        public bool maint = false;

        private readonly List<long> allowedgroups = new List<long>() { -1001070844778, -1001078561643 }; // [0] is testing group, [1] is achv group
        private readonly List<long> adminIds = new List<long>() { 267376056, 295152997 }; // [0] is Florian, [1] is Ludwig

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
                            if (maint)
                            {
                                client.SendTextMessageAsync(id, "<b>The bot is in maintenance mode, so you can't start any further games for now!</b>", parseMode: ParseMode.Html);
                                client.SendTextMessageAsync(allowedgroups[0], $"1 Game just finished. There are {games.Count} more games running.");
                            }
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

                        if (!maint || msg.Chat.Id == allowedgroups[0] || games.ContainsKey(msg.Chat.Id))
                        {
                            switch (text.Split(' ')[0].ToLower().Replace("@werewolfbot", "").Replace('!', '/').Replace("@werewolfwolfachievementbot", ""))
                            {
                                case "/announce":
                                    if (adminIds.Contains(msg.From.Id))
                                    {
                                        client.SendTextMessageAsync(allowedgroups[1], text.Remove(0, text.IndexOf(' ')), parseMode: ParseMode.Html).Wait();
                                        ReplyToMessage("Successfully announced!", u);
                                    }
                                    else ReplyToMessage("You are not a bot admin!", u);

                                    return;

                                case "/startgame":
                                case "/startchaos":
                                    if (games.ContainsKey(msg.Chat.Id))
                                    {
                                        switch (games[msg.Chat.Id].gamestate)
                                        {
                                            case Game.state.Joining:
                                                ReplyToMessage("It seems there is already a game in joining phase! Join that one!", u);
                                                break;

                                            case Game.state.Running:
                                                ReplyToMessage("It seems there is already a game running in here! Stop that before you start a new one!", u);
                                                break;
                                        }
                                    
                                    }
                                    else
                                    {
                                        Message m;

                                        if (pinmessages.ContainsKey(msg.Chat.Id))
                                        {
                                            try
                                            {
                                                var t = client.EditMessageTextAsync(msg.Chat.Id, pinmessages[msg.Chat.Id], "Initializing new game...");
                                                t.Wait();
                                                m = t.Result;
                                                ReplyToMessage($"The new game starts in the pin message! If there is none, please ask an admin for help.", u);
                                            }
                                            catch
                                            {
                                                m = ReplyToMessage("Initializing new game...", u);
                                                pinmessages.Remove(msg.Chat.Id);
                                                client.SendTextMessageAsync(adminIds[0], $"Removed pinmessage of group {msg.Chat.Title} ({msg.Chat.Id}) because it seems it is deleted");
                                                client.SendTextMessageAsync(adminIds[1], $"Removed pinmessage of group {msg.Chat.Title} ({msg.Chat.Id}) because it seems it is deleted");
                                            }

                                        }
                                        else
                                        {
                                            m = ReplyToMessage("Initializing new game...", u);
                                        }
                                        if (m == null) return;
                                        var game = new Game(client, msg.Chat.Id, m);
                                        games.Add(msg.Chat.Id, game);
                                    }
                                    return;

                                case "/addplayer":
                                    if (games.ContainsKey(msg.Chat.Id) && games[msg.Chat.Id].gamestate == Game.state.Joining)
                                    {
                                        User newplayer = msg.ReplyToMessage == null ? msg.From : msg.ReplyToMessage.From;

                                        if (!games[msg.Chat.Id].AddPlayer(newplayer))
                                        {
                                            ReplyToMessage("Failed to add <b>" + newplayer.FirstName + "</b> to the players!", u);
                                        }
                                        else ReplyToMessage($"Player <b>{newplayer.FirstName}</b> was successfully added to the game.", u);
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
                                            if (maint)
                                            {
                                                client.SendTextMessageAsync(msg.Chat.Id, "<b>The bot is in maintenance mode, so you can't start any further games for now!</b>", parseMode: ParseMode.Html);
                                                client.SendTextMessageAsync(allowedgroups[0], $"1 Game just finished. There are {games.Count} more games running.");
                                            }
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
                                        if (dead == null || !g.names.ContainsKey(dead.Id) || g.role[dead.Id] == Game.roles.Dead) return;

                                        switch (g.gamestate)
                                        {
                                            case Game.state.Joining:
                                                if (!g.RemovePlayer(dead))
                                                {
                                                    ReplyToMessage($"Failed to remove player <b>{dead.FirstName}</b> from the game.", u);
                                                }
                                                else ReplyToMessage($"Player <b>{dead.FirstName}</b> was successfully removed from the game.", u);
                                                break;

                                            case Game.state.Running:
                                                g.role.Remove(dead.Id);
                                                g.role.Add(dead.Id, Game.roles.Dead);
                                                g.UpdatePlayerlist();
                                                ReplyToMessage($"Player <b>{dead.FirstName}</b> was marked as dead.", u);
                                                break;
                                        }

                                    }
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

                                case "/listachv":
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
                                        ReplyToMessage($"The love status of <b>{lover.FirstName}</b> was updated.", u);
                                        g.UpdatePlayerlist();
                                    }
                                    return;

                                case "/addalias":
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
                                    return;

                                case "/delalias":
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
                                    return;

                                case "/genpin":
                                    if (adminIds.Contains(msg.From.Id))
                                    {
                                        var m = client.SendTextMessageAsync(msg.Chat.Id, $"This is a pinmessage generated by {msg.From.FirstName}");
                                        m.Wait();
                                        var mID = m.Result.MessageId;

                                        if (pinmessages.ContainsKey(msg.Chat.Id)) pinmessages.Remove(msg.Chat.Id);
                                        pinmessages.Add(msg.Chat.Id, mID);
                                    }
                                    else ReplyToMessage("You are not a bot admin!", u);
                                    return;

                                case "/setpin":
                                    if (adminIds.Contains(msg.From.Id))
                                    {
                                        if (msg.ReplyToMessage != null && msg.ReplyToMessage.From.Id == 245445220)
                                        {
                                            var m = client.EditMessageTextAsync(msg.Chat.Id, msg.ReplyToMessage.MessageId, $"This is a pinmessage generated by {msg.From.FirstName}");
                                            m.Wait();
                                            var mID = m.Result.MessageId;

                                            if (pinmessages.ContainsKey(msg.Chat.Id)) pinmessages.Remove(msg.Chat.Id);
                                            pinmessages.Add(msg.Chat.Id, mID);
                                            ReplyToMessage("That message was successfully set as pin message!", u);
                                        }
                                        else ReplyToMessage("You need to reply to a message of mine!", u);
                                    }
                                    else ReplyToMessage("You are not a bot admin!", u);
                                    return;

                                case "/delpin":
                                    if (adminIds.Contains(msg.From.Id))
                                    {
                                        if (pinmessages.ContainsKey(msg.Chat.Id))
                                        {
                                            pinmessages.Remove(msg.Chat.Id);
                                            ReplyToMessage("Done", u);
                                        }
                                        else ReplyToMessage("No pin message found", u);
                                    }
                                    else ReplyToMessage("You are not a bot admin!", u);
                                    return;

                                case "/getpin":
                                    if (pinmessages.ContainsKey(msg.Chat.Id))
                                    {
                                        client.SendTextMessageAsync(msg.Chat.Id, "Here is the pin message.", replyToMessageId: pinmessages[msg.Chat.Id]);
                                    }
                                    else if (games.ContainsKey(msg.Chat.Id))
                                    {
                                        client.SendTextMessageAsync(msg.Chat.Id, "Here is the game message.", replyToMessageId: games[msg.Chat.Id].pinmessage.MessageId);
                                    }
                                    else
                                    {
                                        ReplyToMessage("No pin message found.", u);
                                    }
                                    return;

                                case "/lynchorder":
                                    if (games.ContainsKey(msg.Chat.Id) && games[msg.Chat.Id].gamestate == Game.state.Running)
                                    {
                                        Game g = games[msg.Chat.Id];

                                        if (!string.IsNullOrEmpty(g.lynchorder))
                                        {
                                            string order = g.lynchorder.Replace("<-->", "↔️").Replace("<->", "↔️").Replace("<>", "↔️").Replace("-->", "➡️").Replace("->", "➡️").Replace(">", "➡️");
                                            ReplyToMessage("<b>Lynchorder:</b>\n" + order, u);
                                        }
                                        else
                                        {
                                            string order = "";

                                            List<string> alives = new List<string>();

                                            foreach (var x in g.names)
                                            {
                                                if (g.role[x.Key] != Game.roles.Dead) alives.Add(x.Value);
                                            }

                                            foreach (var n in alives)
                                            {
                                                order += n + "\n";
                                            }
                                            order += alives[0];

                                            ReplyToMessage("<b>Lynchorder:</b>\n" + order, u);
                                        }
                                    }
                                    else ReplyToMessage("This command can only be used while a game is running.", u);
                                    return;

                                case "/setlynchorder":
                                    if (games.ContainsKey(msg.Chat.Id) && games[msg.Chat.Id].gamestate == Game.state.Running)
                                    {
                                        Game g = games[msg.Chat.Id];

                                        if (msg.ReplyToMessage != null && msg.ReplyToMessage.Type == MessageType.TextMessage && !string.IsNullOrEmpty(msg.ReplyToMessage.Text))
                                        {
                                            if (msg.ReplyToMessage.From.Id == 245445220) // The bot itself
                                            {
                                                g.lynchorder = "";
                                                ReplyToMessage($"The lynchorder was reset by <b>{msg.From.FirstName}</b>.", u);
                                                return;
                                            }
                                            else
                                            {
                                                string t = msg.ReplyToMessage.Text;
                                                g.lynchorder = t;
                                                ReplyToMessage($"The lynchorder was set by <b>{msg.From.FirstName}</b>. Get it with the /lynchorder command.", u);
                                            }
                                        }
                                        else if (text.Split(' ').Count() > 1)
                                        {
                                            string t = msg.Text.Substring(msg.Text.IndexOf(' '));
                                            g.lynchorder = t;
                                            ReplyToMessage($"The lynchorder was set by <b>{msg.From.FirstName}</b>. Get it with the /lynchorder command.", u);
                                        }
                                        else ReplyToMessage("Either use \n\n<code>/setlynchorder [lynchorder here]</code>\n\nor reply to the lynchorder with <code>/setlynchorder</code>.", u);
                                    }
                                    else ReplyToMessage("This command can only be used while a game is running.", u);
                                    return;

                                case "/resetlynchorder":
                                    if (games.ContainsKey(msg.Chat.Id) && games[msg.Chat.Id].gamestate == Game.state.Running)
                                    {
                                        Game g = games[msg.Chat.Id];
                                        g.lynchorder = "";
                                        ReplyToMessage($"The lynchorder was reset by <b>{msg.From.FirstName}</b>.", u);
                                    }
                                    else ReplyToMessage("This command can only be used while a game is running.", u);
                                    return;

                                case "/maint":
                                    if (adminIds.Contains(msg.From.Id))
                                    {
                                        maint = !maint;
                                        if (maint) // We just enabled maintenance
                                        {
                                            if (games.Count > 0)
                                            {
                                                foreach (var g in games)
                                                {
                                                    if (g.Value.gamestate == Game.state.Joining)
                                                    {
                                                        client.SendTextMessageAsync(g.Key, "<b>The bot is shutting down for maintenance and this game was cancelled! You can play it, but I won't manage it!</b>", parseMode: ParseMode.Html);
                                                        g.Value.Stop();
                                                        g.Value.UpdatePlayerlist();
                                                        games.Remove(g.Key);
                                                    }
                                                    else
                                                    {
                                                        client.SendTextMessageAsync(g.Key, "<b>After this game, the bot is shutting down for maintenance!</b>", parseMode: ParseMode.Html);
                                                    }
                                                }
                                                if (!games.Keys.Contains(allowedgroups[1])) client.SendTextMessageAsync(allowedgroups[1], "<b>After this game, the bot is shutting down for maintenance!</b>", parseMode: ParseMode.Html);
                                                ReplyToMessage($"There are still {games.Count} games running. The maintenance mode was enabled.", u);
                                            }
                                            else
                                            {
                                                client.SendTextMessageAsync(allowedgroups[1], "<b>The bot is now going down for maintenance!</b>", parseMode: ParseMode.Html);
                                                ReplyToMessage("There are no games running. The maintenance mode was enabled", u);
                                            }
                                        }
                                        else // We just disabled maintenance
                                        {
                                            client.SendTextMessageAsync(allowedgroups[1], "<b>The bot is no longer under maintenance! You can now play games!</b>", parseMode: ParseMode.Html);
                                            ReplyToMessage("The maintenance mode was disabled!", u);
                                        }
                                    }
                                    else ReplyToMessage("You aren't a bot admin!", u);
                                    return;

                                case "/runinfo":
                                    string infomessage = "<b>RUNTIME INFO:</b>\n";
                                    infomessage += "Running for: <b>" + (DateTime.UtcNow - starttime).ToString().Remove((DateTime.UtcNow - starttime).ToString().LastIndexOf('.') + 2) + "</b>\n";
                                    infomessage += "Running games: <b>" + games.Count + "</b>\n";
                                    ReplyToMessage(infomessage, u);
                                    return;
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
                                            g.role.Add(player, role);
                                            g.UpdatePlayerlist();
                                            ReplyToMessage($"Role was set to: <b>{g.rolestring[role]}</b>", u);
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
                                                ReplyToMessage($"Role was updated to: <b>{g.rolestring[role]}</b>.", u);
                                            }
                                            else ReplyToMessage($"The role was already <b>{g.rolestring[role]}</b>!", u);
                                        }
                                    }
                                }
                            }

                            if (msg.ForwardFrom != null && (msg.ForwardFrom.Id == 175844556 || msg.ForwardFrom.Id == 19862752) && (msg.Text.ToLower().Contains("unlock") || msg.Text.ToLower().Contains("achievement")))
                            {
                                ReplyToMessage("👍🏻", u);
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
