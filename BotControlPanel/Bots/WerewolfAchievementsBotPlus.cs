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
        class BotUser
        {
            public string name { get; set; }
            public int id { get; }
            public string username { get; set; }
            public bool Subscribing { get; set; }

            public BotUser(string name, int id, string username = null, bool Subscribing = false)
            {
                this.name = name;
                this.id = id;
                this.username = username;
                this.Subscribing = Subscribing;
            }
        }


        class Player
        {
            public string name { get; set; }
            public int id { get; set; }
            public Game.roles role { get; set; }
            public bool love { get; set; }

            public Player(int id, string name)
            {
                this.id = id;
                this.name = name.Length <= 15 ? name : name.Remove(15);
            }
        }


        class Game
        {
            public Dictionary<int, Player> players = new Dictionary<int, Player>();

            public Message pinmessage { get; set; }
            public state gamestate { get; set; }
            private TelegramBotClient client;

            public string lynchorder = "";

            public Game(TelegramBotClient cl, Message pin)
            {
                client = cl;
                pinmessage = pin;
                UpdatePlayerlist();
            }

            public bool AddPlayer(int newplayer, string name)
            {
                if (!players.ContainsKey(newplayer) && gamestate == state.Joining)
                {
                    try
                    {
                        players.Add(newplayer, new Player(newplayer, name));
                        players[newplayer].role = roles.Unknown;
                        players[newplayer].love = false;
                        UpdatePlayerlist();
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
                return false;
            }

            public bool RemovePlayer(int oldplayer)
            {
                if (players.ContainsKey(oldplayer))
                {
                    try
                    {
                        players.Remove(oldplayer);
                        UpdatePlayerlist();
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
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

            public void UpdatePlayerlist()
            {
                string playerlist = gamestate == state.Running
                    ? $"<b>LYNCHORDER ({players.Count(x => x.Value.role != roles.Dead)} of {players.Count}):</b>\n"
                    : $"<b>Players ({players.Count}):</b>\n";

                foreach (var p in players.Values.Where(x => x.role != roles.Dead))
                {
                    if (gamestate == state.Joining) playerlist += p.name + "\n";
                    else if (gamestate == state.Running)
                    {
                        if (p.role != roles.Unknown) playerlist += "<b>" + p.name + "</b>: " + rolestring[p.role];
                        else playerlist += "<b>" + p.name + "</b>: " + rolestring[roles.Unknown];

                        if (p.love) playerlist += " ❤️";
                        playerlist += "\n";
                    }
                }

                if (gamestate == state.Running)
                {
                    playerlist += "\n\n<b>DEAD PLAYERS 💀:</b>";

                    foreach (var p in players.Values.Where(x => x.role == roles.Dead))
                    {
                        playerlist += "\n" + p.name;
                    }
                }

                switch (gamestate)
                {

                    case state.Joining:
                        client.EditMessageTextAsync(pinmessage.Chat.Id, pinmessage.MessageId, $"<b>Join this game!</b>\n\nJoin using the button and remember to use /addplayer after joining. Click the start button below as soon as the roles are assigned and the game begins. <b>DON'T PRESS START BEFORE THE ROLES ARE ASSIGNED!</b>\n\n{playerlist}", parseMode: ParseMode.Html, replyMarkup: InlineKeyboardStart.Get(pinmessage.Chat.Id)).Wait();
                        break;

                    case state.Running:
                        client.EditMessageTextAsync(pinmessage.Chat.Id, pinmessage.MessageId, $"<b>Game running!</b>\n\nPress stop <b>ONCE THE GAME STOPPED!</b>\n\n{playerlist}", parseMode: ParseMode.Html, replyMarkup: InlineKeyboardStop.Get(pinmessage.Chat.Id)).Wait();
                        break;

                    case state.Stopped:
                        client.EditMessageTextAsync(pinmessage.Chat.Id, pinmessage.MessageId, "<b>This game is finished!</b>", parseMode: ParseMode.Html).Wait();
                        break;

                    default: // fucked
                        return;
                }
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
                List<roles> gameroles = new List<roles>();

                foreach (var p in players.Values)
                {
                    gameroles.Add(p.role);
                }

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
                visitcount += gameroles.Contains(roles.Cultist) ? 1 : 0;
                visitcount += gameroles.Count(x => x == roles.SerialKiller);
                visitcount += gameroles.Count(x => x == roles.CultistHunter);
                visitcount += gameroles.Count(x => x == roles.Harlot);
                visitcount += gameroles.Count(x => x == roles.GuardianAngel);

                int maxkill = 0;
                maxkill += spawnableWolves >= 1 ? 1 : 0; // Wolves eat one person
                maxkill += gameroles.Contains(roles.WolfCub) && spawnableWolves >= 2 ? 1 : 0; // If there is a cub and at least one more wolf to spawn, they can eat two persons
                maxkill += gameroles.Count(x => x == roles.SerialKiller); // Serialkiller kills one person
                maxkill += gameroles.Contains(roles.CultistHunter) && (gameroles.Contains(roles.Cultist) || gameroles.Contains(roles.SerialKiller)) ? 1 : 0; // Cultist hunter visits one cult or the sk
                maxkill += gameroles.Contains(roles.Cultist) && (gameroles.Contains(roles.CultistHunter) || gameroles.Contains(roles.SerialKiller)) ? 1 : 0; // Cult visit the ch or sk
                maxkill += gameroles.Contains(roles.Cupid) ? 1 : 0; // If there is a couple, both can die the same night
                maxkill += gameroles.Contains(roles.Harlot) && (gameroles.Contains(roles.SerialKiller) || spawnableWolves >= 1) ? 1 : 0; // Harlot visit sk or wolf (or wolf/sk victim)

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
                        return players.Count >= 20;

                    case achievements.ISeeALackOfTrust:
                        return gameroles.Contains(roles.Seer) || gameroles.Contains(roles.SeerFool) || gameroles.Contains(roles.ApprenticeSeer);

                    case achievements.LoneWolf:
                        return wolves == 1 && !gameroles.Contains(roles.Traitor) && players.Count >= 10;

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
                        return maxkill >= 4;

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

            public static Dictionary<roles, string> rolestring = new Dictionary<roles, string>()
            {
                { roles.AlphaWolf, "Alpha Wolf 🐺⚡️" },
                { roles.ApprenticeSeer, "App Seer 🙇" },
                { roles.Beholder, "Beholder 👁" },
                { roles.Blacksmith, "Blacksmith ⚒" },
                { roles.ClumsyGuy, "Clumsy Guy 🤕" },
                { roles.Cultist, "Cultist 👤" },
                { roles.CultistHunter, "Cult Hunter 💂" },
                { roles.Cupid, "Cupid 🏹" },
                { roles.Cursed, "Cursed 😾" },
                { roles.Detective, "Detective 🕵️" },
                { roles.Doppelgänger, "Doppelgänger 🎭" },
                { roles.Drunk, "Drunk 🍻" },
                { roles.Fool, "Fool 🃏" },
                { roles.GuardianAngel, "Guardian Angel 👼" },
                { roles.Gunner, "Gunner 🔫" },
                { roles.Harlot, "Harlot 💋" },
                { roles.Hunter, "Hunter 🎯" },
                { roles.Mason, "Mason 👷" },
                { roles.Mayor, "Mayor 🎖" },
                { roles.Prince, "Prince 👑" },
                { roles.Seer, "Seer 👳" },
                { roles.SerialKiller, "Serial Killer 🔪" },
                { roles.Sorcerer, "Sorcerer 🔮" },
                { roles.Tanner, "Tanner 👺" },
                { roles.Traitor, "Traitor 🖕" },
                { roles.Villager, "Villager 👱" },
                { roles.Werewolf, "Werewolf 🐺" },
                { roles.WildChild, "Wild Child 👶" },
                { roles.WolfCub, "Wolf Cub 🐶" },
                { roles.SeerFool, "Seer OR Fool 👳🃏" },
                { roles.Dead, "DEAD 💀" },
                { roles.Unknown, "No role detected yet" },
            };

            public static Dictionary<achievements, string> achv = new Dictionary<achievements, string>()
            {
                { achievements.AlzheimersPatient, "Alzheimer's Patient" },
                { achievements.BlackSheep, "Black Sheep" },
                { achievements.ChangeSidesWorks, "Change Sides Works" },
                { achievements.CultistConvention, "Cultist Convention" },
                { achievements.CultistFodder, "Cultist Fodder" },
                { achievements.Dedicated, "Dedicated" },
                { achievements.Developer, "Developer" },
                { achievements.DoubleKill, "Double Kill" },
                { achievements.DoubleShifter, "Double Shifter" },
                { achievements.DoubleVision, "Double Vision" },
                { achievements.Enochlophobia, "Enochlophobia" },
                { achievements.EvenAStoppedClockIsRightTwiceADay, "Even A Stopped Clock Is Right Twice A Day" },
                { achievements.Explorer, "Explorer" },
                { achievements.ForbiddenLove, "Forbidden Love" },
                { achievements.HeresJohnny, "Here's Johnny" },
                { achievements.HeyManNiceShot, "Hey Man, Nice Shot!" },
                { achievements.IHaveNoIdeaWhatImDoing, "I Have No Idea What I'm Doing" },
                { achievements.Inconspicuous, "Inconspicuous" },
                { achievements.InForTheLongHaul, "In For The Long Haul" },
                { achievements.Introvert, "Introvert" },
                { achievements.ISeeALackOfTrust, "I See A Lack Of Trust" },
                { achievements.IveGotYourBack, "I've Got Your Back" },
                { achievements.Linguist, "Linguist" },
                { achievements.LoneWolf, "Lone Wolf" },
                { achievements.Masochist, "Masochist" },
                { achievements.MasonBrother, "Mason Brother" },
                { achievements.Naughty, "Naughty" },
                { achievements.Obsessed, "Obsessed" },
                { achievements.OHAIDER, "O HAI DER" },
                { achievements.OHSHI, "OH SHI-" },
                { achievements.PackHunter, "Pack Hunter" },
                { achievements.Promiscuous, "Promiscuous" },
                { achievements.SavedByTheBullet, "Saved By The Bullet" },
                { achievements.SelfLoving, "Self Loving" },
                { achievements.SerialSamaritan, "Serial Samaritan" },
                { achievements.ShouldHaveKnown, "Should Have Known" },
                { achievements.ShouldveSaidSomething, "Should've Said Something" },
                { achievements.SmartGunner, "Smart Gunner" },
                { achievements.SoClose, "So Close" },
                { achievements.SpeedDating, "Speed Dating" },
                { achievements.SpyVsSpy, "Spy Vs Spy" },
                { achievements.Streetwise, "Streetwise" },
                { achievements.SundayBloodySunday, "Sunday Bloody Sunday" },
                { achievements.Survivalist, "Survivalist" },
                { achievements.TannerOverkill, "Tanner Overkill" },
                { achievements.ThatsWhyYouDontStayHome, "That's Why You Don't Stay Home" },
                { achievements.TheFirstStone, "The First Stone" },
                { achievements.Veteran, "Veteran" },
                { achievements.WelcomeToHell, "Welcome To Hell" },
                { achievements.WelcomeToTheAsylum, "Welcome To The Asylum" },
                { achievements.WobbleWobble, "Wobble Wobble" },


                // NEW ACHIEVEMENTS
                { achievements.NoSorcery, "No Sorcery!" },
                { achievements.WuffieCult, "Wuffie-Cult" },
                { achievements.ThreeLittleWolvesAndABigBadPig, "Three Little Wolves And A Big Bad Pig" },
                { achievements.IHelped, "I Helped!" },
                { achievements.CultistTracker, "Cultist Tracker" },
                { achievements.ImNotDrunBurppp, "I'M NOT DRUN-- *BURPPP*" },
                { achievements.DidYouGuardYourself, "Did You Guard Yourself?" },
                { achievements.SpoiledRichBrat, "Spoiled Rich Brat" },
                { achievements.President, "President" },
                { achievements.ItWasABusyNight, "It Was A Busy Night!" },
            };
        } // End of class Game

        public override string Name { get; } = "Werewolf Achievements Bot";
        private const string basePath = "C:\\Olfi01\\BotControlPanel\\AchievementsBot\\";
        private const string aliasesPath = basePath + "aliases.dict";
        private const string usersPath = basePath + "users.dict";
        private const string version = "3.5.0";
        private readonly DateTime starttime = DateTime.UtcNow;

        private Dictionary<int, BotUser> users = new Dictionary<int, BotUser>();
        private Dictionary<long, Game> games = new Dictionary<long, Game>();
        private Dictionary<long, Message> pinmessages = new Dictionary<long, Message>();
        private Dictionary<string, Game.roles> roleAliases = new Dictionary<string, Game.roles>();
        private DateTime lastping = DateTime.MinValue;

        List<long> justCalledStop = new List<long>();
        public bool maint = true;

        private readonly List<long> allowedgroups = new List<long>() { -1001070844778, -1001078561643 }; // [0] is testing group, [1] is achv group
        private const string achvLink = "https://t.me/joinchat/AAAAAEBJi2uYsVBF2fVwBg";
        private readonly List<int> adminIds = new List<int>() { 267376056, 295152997 }; // [0] is Florian, [1] is Ludwig

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
                        if (games[id].players.Count >= 5 || id == allowedgroups[0]) // player limit disabled for test group
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
                            if (maint && id != allowedgroups[0])
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
                client.SendTextMessageAsync(allowedgroups[0], $"Error in achievements callback: {ex.Message}\n{ex.StackTrace}").Wait();
            }
        }

        private Message ReplyToMessage(string text, Update u, IReplyMarkup replyMarkup = null)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                if (u.Message != null)
                {
                    var chatid = u.Message.Chat.Id;
                    var messageid = u.Message.MessageId;

                    try
                    {
                        var task = client.SendTextMessageAsync(chatid, text, replyToMessageId: messageid, parseMode: ParseMode.Html, replyMarkup: replyMarkup);
                        task.Wait();
                        var msg = task.Result;
                        return msg;
                    }
                    catch (Exception ex)
                    {
                        client.SendTextMessageAsync(allowedgroups[0], $"Error in ReplyToMessage Method: {ex.Message}\n{ex.StackTrace}").Wait();
                        client.SendTextMessageAsync(u.Message.Chat.Id, "Tried to send something to this chat but failed! The devs were informed! Sorry!").Wait();
                        return null;
                    }
                }
            }
            return null;
        }

        private Message EditMessage(string newtext, Message m, IReplyMarkup replyMarkup = null)
        {
            if (!m.Text.Equals(newtext))
            {
                return EditMessage(newtext, m.Chat.Id, m.MessageId, replyMarkup);
            }
            else return m;
        }

        private Message EditMessage(string newtext, long chatid, int messageid, IReplyMarkup replyMarkup = null)
        {
            
            try
            {
                var t = client.EditMessageTextAsync(chatid, messageid, newtext, ParseMode.Html, replyMarkup: replyMarkup);
                t.Wait();
                return t.Result;
            }
            catch (Exception e)
            {
                client.SendTextMessageAsync(chatid, "Tried to edit a message in this chat but failed! Sorry! The devs were informed.");
                client.SendTextMessageAsync(allowedgroups[0], $"{e.StackTrace}\n{e.Message}\n{e.InnerException?.Message}");
                return null;
            }
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
                    if (e.Update.Message.Type == MessageType.TextMessage)
                    {
                        var u = e.Update;
                        var msg = u.Message;
                        var text = msg.Text;

                        if (!maint || msg.Chat.Id == allowedgroups[0] || games.ContainsKey(msg.Chat.Id) || adminIds.Contains(msg.From.Id))
                        {
                            if (!users.ContainsKey(msg.From.Id)) AddUser(msg.From);
                            else if (users[msg.From.Id].name != msg.From.FirstName || users[msg.From.Id].username != msg.From.Username)
                            {
                                users[msg.From.Id].name = msg.From.FirstName;
                                users[msg.From.Id].username = msg.From.Username;
                            }

                            if (msg.Chat.Type != ChatType.Private) switch (text.Split(' ')[0].ToLower().Replace("@werewolfbot", "").Replace('!', '/').Replace("@werewolfwolfachievementbot", ""))
                            {
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
                                                m = EditMessage("Initializing new game...", pinmessages[msg.Chat.Id]);
                                                ReplyToMessage($"The new game starts in the pin message! If there is none, please ask an admin for help.", u);
                                            }
                                            catch
                                            {
                                                m = ReplyToMessage("Initializing new game...", u);
                                                pinmessages.Remove(msg.Chat.Id);
                                                client.SendTextMessageAsync(allowedgroups[0], $"Removed pinmessage of group {msg.Chat.Title} ({msg.Chat.Id}) because it seems it is deleted").Wait();
                                            }

                                        }
                                        else
                                        {
                                            m = ReplyToMessage("Initializing new game...", u);
                                        }
                                        if (m == null) return;
                                        var game = new Game(client, m);
                                        games.Add(msg.Chat.Id, game);
                                    }
                                    return;

                                case "/addplayer":
                                    if (games.ContainsKey(msg.Chat.Id) && games[msg.Chat.Id].gamestate == Game.state.Joining)
                                    {
                                        int newplayer = GetUserId(u);
                                        if (newplayer == 0)
                                        {
                                            ReplyToMessage("Couldn't find that user in my database!", u);
                                            return;
                                        }
                                        string name = GetName(newplayer);

                                        if (!games[msg.Chat.Id].AddPlayer(newplayer, name))
                                        {
                                            ReplyToMessage("Failed to add <b>" + name + "</b> to the players!", u);
                                        }
                                        else ReplyToMessage($"Player <b>{name}</b> was successfully added to the game.", u);
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
                                            if (maint && msg.Chat.Id != allowedgroups[0])
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

                                        int dead = GetUserId(u);
                                        if (dead == 0 || !g.players.ContainsKey(dead) || g.players[dead].role == Game.roles.Dead) return;
                                        string name = GetName(dead);

                                        switch (g.gamestate)
                                        {
                                            case Game.state.Joining:
                                                if (!g.RemovePlayer(dead))
                                                {
                                                    ReplyToMessage($"Failed to remove player <b>{name}</b> from the game.", u);
                                                }
                                                else ReplyToMessage($"Player <b>{name}</b> was successfully removed from the game.", u);
                                                break;

                                            case Game.state.Running:
                                                g.players[dead].role = Game.roles.Dead;
                                                g.UpdatePlayerlist();
                                                ReplyToMessage($"Player <b>{name}</b> was marked as dead.", u);
                                                break;
                                        }

                                    }
                                    return;

                                case "/listachv":
                                    if (games.ContainsKey(msg.Chat.Id))
                                    {
                                        Game g = games[msg.Chat.Id];
                                        string possible = "<b>POSSIBLE ACHIEVEMENTS:</b>\n";

                                        foreach (var achv in Game.achv.Keys)
                                        {
                                            possible += g.isAchievable(achv)
                                                ? Game.achv[achv] + "\n"
                                                : "";
                                        }
                                        ReplyToMessage(possible, u);
                                    }
                                    return;

                                case "/love":
                                    if(games.ContainsKey(msg.Chat.Id))
                                    {
                                        Game g = games[msg.Chat.Id];

                                        int lover = GetUserId(u);
                                        if (lover == 0 || !g.players.ContainsKey(lover)) return;
                                        string name = GetName(lover);

                                        g.players[lover].love = !g.players[lover].love;
                                        ReplyToMessage($"The love status of <b>{name}</b> was updated.", u);
                                        g.UpdatePlayerlist();
                                    }
                                    return;

                                case "/genpin":
                                    if (adminIds.Contains(msg.From.Id))
                                    {
                                        var t = client.SendTextMessageAsync(msg.Chat.Id, $"This is a pinmessage generated by {msg.From.FirstName}");
                                        t.Wait();
                                        var m = t.Result;

                                        if (pinmessages.ContainsKey(msg.Chat.Id)) pinmessages.Remove(msg.Chat.Id);
                                        pinmessages.Add(msg.Chat.Id, m);
                                    }
                                    else ReplyToMessage("You are not a bot admin!", u);
                                    return;

                                case "/setpin":
                                    if (adminIds.Contains(msg.From.Id))
                                    {
                                        if (msg.ReplyToMessage != null && msg.ReplyToMessage.From.Id == 245445220)
                                        {
                                            var m = EditMessage($"This is a pinmessage generated by <b>{msg.From.FirstName}</b>.", msg.ReplyToMessage);

                                            if (pinmessages.ContainsKey(msg.Chat.Id)) pinmessages.Remove(msg.Chat.Id);
                                            pinmessages.Add(msg.Chat.Id, m);
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
                                        client.SendTextMessageAsync(msg.Chat.Id, "Here is the pin message.", replyToMessageId: pinmessages[msg.Chat.Id].MessageId);
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

                                            foreach (var x in g.players.Values)
                                            {
                                                if (x.role != Game.roles.Dead) alives.Add(x.name);
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

                                case "#ping":
                                    var difference = DateTime.UtcNow - lastping;
                                    if (difference.TotalMinutes >= 10)
                                    {
                                        if (games.ContainsKey(msg.Chat.Id) && games[msg.Chat.Id].gamestate == Game.state.Joining)
                                        {
                                            ReplyToMessage("<b>🔔 Ping! 🔔</b>\n\nAchievement hunters are called!\n\nIf you want to be notified by this command, use the subscribe button below! To no longer be notified, use the unsubscribe button. You will be sent to our private chat, where you need to start me, and we are done :D\n\n<b>Have fun hunting achievements!</b>", u, InlineKeyboardSubscribe.Get());
                                            string group = msg.Chat.Id == allowedgroups[1] ? $"<a href=\"{achvLink}\">{msg.Chat.Title}</a>" : $"<b>{msg.Chat.Title}</b>";
                                            foreach (var pinguser in users.Values.Where(x => x.Subscribing && msg.From.Id != x.id && !games[msg.Chat.Id].players.ContainsKey(x.id))) client.SendTextMessageAsync(pinguser.id, $"<b>🔔 Ping! 🔔</b>\n\nAchievement hunters are called in {group}!", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                            lastping = DateTime.UtcNow;
                                        }
                                            else ReplyToMessage("You can only ping while a game is in joining phase! To subscribe (or unsubscribe) to the pinglist, you can use the following buttons:", u, InlineKeyboardSubscribe.Get());
                                    }
                                    else
                                    {
                                        var waittime = TimeSpan.FromMinutes(10) - difference;
                                        ReplyToMessage($"Only one ping in ten minutes is allowed! You need to wait {waittime.Minutes}:{waittime.Seconds} minutes until you can ping again! To subscribe (or unsubscribe) to the pinglist, you can use the following buttons:", u, InlineKeyboardSubscribe.Get());
                                    }
                                    return;
                                }


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

                                case "/ping":
                                    ReplyToMessage("<b>PENG!</b>", u);
                                    return;

                                case "/version":
                                    ReplyToMessage($"Werewolf Achievements Manager.\n <b>Version {version}.</b>", u);
                                    return;

                                case "/listalias":
                                    var rolestrings = Game.rolestring;
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

                                case "/knownusers": // this command will be removed again, just for testing purposes
                                    if (adminIds.Contains(msg.From.Id))
                                    {
                                        var adminsT = client.GetChatAdministratorsAsync(allowedgroups[1]);
                                        adminsT.Wait();
                                        var admins = adminsT.Result;


                                        string knownusers = "☝️ Bot admins:\n\n";
                                        foreach (BotUser user in users.Values.Where(x => adminIds.Contains(x.id)))
                                        {
                                            knownusers += $"{user.name}\n  - @{user.username}\n  - {user.id}\n  - Subscribing:" + (user.Subscribing  ? " ✅" : " ❌") + "\n\n";
                                        }


                                        knownusers += "\n\n👮‍♀️ Group admins:\n\n";
                                        foreach (BotUser user in users.Values.Where(y => admins.Count(x => x.User.Id == y.id) == 1 && !adminIds.Contains(y.id)))
                                        {
                                            knownusers += $"{user.name}\n  - @{user.username}\n  - {user.id}\n  - Subscribing:" + (user.Subscribing ? " ✅" : " ❌") + "\n\n";
                                        }

                                        knownusers += "\n\n👨‍ Members:\n\n";
                                        foreach (BotUser user in users.Values.Where(x => admins.Count(y => y.User.Id == x.id) == 0))
                                        {
                                            knownusers += $"{user.name}\n  - @{user.username}\n  - {user.id}\n  - Subscribing:" + (user.Subscribing ? " ✅" : " ❌") + "\n\n";
                                        }

                                        
                                        List<string> knownuserlist = new List<string>();

                                        while (knownusers.Length >= 2000)
                                        {
                                            knownuserlist.Add(knownusers.Substring(0, 2000));
                                            knownusers = knownusers.Remove(0, 2000);
                                        }
                                        if (!string.IsNullOrEmpty(knownusers)) knownuserlist.Add(knownusers);

                                        foreach (string s in knownuserlist)
                                        {
                                            client.SendTextMessageAsync(msg.Chat.Id, s, parseMode: ParseMode.Html).Wait();
                                        }
                                        ReplyToMessage("Finished!", u);
                                    }
                                    return;


                                case "whois":
                                    if (adminIds.Contains(msg.From.Id))
                                    {
                                        int id = GetUserId(u);
                                        if (id == 0) ReplyToMessage("Couln't find that user.", u);
                                        else
                                        {
                                            string name = GetName(id);
                                            string username = users.Values.FirstOrDefault(x => x.id == id)?.username;
                                            username = string.IsNullOrEmpty(username) ? "no username" : $"@{username}";
                                            ReplyToMessage($"Name: {name}\nId: {id}\nUsername: {username}", u);
                                        }
                                    }
                                    return;
                            }

                            if (msg.Chat.Type == ChatType.Private)
                            {
                                if (msg.Text?.Split(' ')[0] == "/start")
                                {
                                    bool subscriber = false;
                                    if (msg.Text.Split(' ').Length == 2)
                                    {
                                        switch (msg.Text.Split(' ')[1])
                                        {
                                            case "subscribe":
                                                subscriber = true;
                                                if (!users[msg.From.Id].Subscribing)
                                                {
                                                    users[msg.From.Id].Subscribing = true;
                                                    ReplyToMessage("You successfully subscribed to the ping list! Once someone sends #ping in the achievement group, I'll inform you.", u);
                                                }
                                                else ReplyToMessage("You were already subscribed to the ping list!", u);
                                                return;

                                            case "unsubscribe":
                                                subscriber = true;
                                                if (users[msg.From.Id].Subscribing)
                                                {
                                                    users[msg.From.Id].Subscribing = false;
                                                    ReplyToMessage("You successfully stopped subscribing to the ping list!", u);
                                                }
                                                else ReplyToMessage("You weren't even subscribing!", u);
                                                return;
                                        }
                                    }

                                    if (!subscriber) ReplyToMessage("Hi! I am <b>Achievement Manager Bot</b>, the bot to make it easy to farm achievements in the telegram werewolf game.\n\nI was designed by @Olgabrezel and @Olfi01 and I am especially made for the @wwachievement group - I won't work anywhere else.\n\n<b>My features:</b>\n - /startgame: I will listen when a game is started so I can manage it for you.\n - /addplayer: You can use this command to tell me that you joined the game. I can't know any other way.\n - /dead: Mark yourself as dead\n - /love: Mark yourself as lover\n - [your role]: Send a message that contains only your role and I will show it in the pinned message.\n - Now [your role]: If your role changes, use this to tell me.", u);
                                }
                            }

                            if (games.ContainsKey(msg.Chat.Id))
                            {
                                if (games[msg.Chat.Id].gamestate == Game.state.Running)
                                {
                                    Game g = games[msg.Chat.Id];

                                    int player = 0;
                                    if (msg.ReplyToMessage != null)
                                    {
                                        if (g.players.Keys.Contains(msg.ReplyToMessage.From.Id)) player = msg.ReplyToMessage.From.Id;
                                    }
                                    else if (g.players.Keys.Contains(msg.From.Id))
                                    {
                                        player = msg.From.Id;
                                    }

                                    if (player == 0) return;

                                    if (g.players[player].role == Game.roles.Unknown && roleAliases.Keys.Contains(text.ToLower()))
                                    {
                                        var role = GetRoleByAlias(text.ToLower());
                                        if (role != Game.roles.Unknown)
                                        {
                                            g.players[player].role = role;
                                            g.UpdatePlayerlist();
                                            ReplyToMessage($"Role was set to: <b>{Game.rolestring[role]}</b>", u);
                                        }
                                    }
                                    if (text.ToLower().StartsWith("now ") && roleAliases.Keys.Contains(text.ToLower().Substring(4)))
                                    {
                                        var role = GetRoleByAlias(text.ToLower().Substring(4));
                                        if (role != Game.roles.Unknown)
                                        {
                                            var oldRole = g.players[player].role;
                                            if (oldRole != role)
                                            {
                                                g.players[player].role = role;
                                                g.UpdatePlayerlist();
                                                ReplyToMessage($"Role was updated to: <b>{Game.rolestring[role]}</b>.", u);
                                            }
                                            else ReplyToMessage($"The role was already <b>{Game.rolestring[role]}</b>!", u);
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
                client.SendTextMessageAsync(allowedgroups[0], $"Error in Achievements Bot: {ex.InnerException}\n{ex.Message}\n{ex.StackTrace}").Wait();
            }
        }

        public int GetUserId(Update u)
        {
            if (u.Message.ReplyToMessage != null && u.Message.ReplyToMessage.From.Id != 245445220)
            {
                return u.Message.ReplyToMessage.From.Id;
            }
            if (u.Message.Text.Split(' ').Length > 1)
            {
                {
                    if (users.Any(x => x.Value.id.ToString().Equals(u.Message.Text.Split(' ')[1])))
                        return int.Parse(u.Message.Text.Split(' ')[1]);

                    if (users.Any(x => x.Value.username == u.Message.Text.Split(' ')[1].Replace("@", "")))
                        return users.First(x => x.Value.username == u.Message.Text.Split(' ')[1].Replace("@", "")).Value.id;
                }
            }
            return u.Message.From.Id;
        }

        public string GetName(int id)
        {
            var u = users.Values.FirstOrDefault(x => x.id == id);
            if (u == null) return null;
            return u.name;
        }

        private void AddUser(User u)
        {
            users.Add(u.Id, new BotUser(u.FirstName, u.Id, u.Username));
            if (!System.IO.Directory.Exists(basePath)) System.IO.Directory.CreateDirectory(basePath);
            System.IO.File.WriteAllText(usersPath, JsonConvert.SerializeObject(users));
        }

        private void RemoveUser(int id)
        {
            users.Remove(id);
            if (!System.IO.Directory.Exists(basePath)) System.IO.Directory.CreateDirectory(basePath);
            System.IO.File.WriteAllText(usersPath, JsonConvert.SerializeObject(users));
        }

        private void ReadUsers()
        {
            if (System.IO.File.Exists(usersPath))
            {
                users = JsonConvert.DeserializeObject<Dictionary<int, BotUser>>(System.IO.File.ReadAllText(usersPath));
            }
            else
            {
                if (!System.IO.Directory.Exists(basePath)) System.IO.Directory.CreateDirectory(basePath);
                System.IO.File.Create(usersPath);
            }
            if (users == null) users = new Dictionary<int, BotUser>();
        }

        private Game.roles GetRoleByAlias(string alias)
        {
            if (roleAliases.ContainsKey(alias)) return roleAliases[alias];
            return Game.roles.Unknown;
        }


        private void writeAliasesFile()
        {
            Dictionary<string, Game.roles> defaultaliasses = new Dictionary<string, Game.roles>()
            {
                { "alphawolf", Game.roles.AlphaWolf },
                { "apprenticeseer", Game.roles.ApprenticeSeer },
                { "beholder", Game.roles.Beholder },
                { "blacksmith", Game.roles.Blacksmith },
                { "clumsyguy", Game.roles.ClumsyGuy },
                { "cultist", Game.roles.Cultist },
                { "cultisthunter", Game.roles.CultistHunter },
                { "cupid", Game.roles.Cupid },
                { "cursed", Game.roles.Cursed },
                { "detective", Game.roles.Detective },
                { "doppelgänger", Game.roles.Doppelgänger },
                { "drunk", Game.roles.Drunk },
                { "fool", Game.roles.Fool },
                { "guardianangel", Game.roles.GuardianAngel },
                { "gunner", Game.roles.Gunner },
                { "harlot", Game.roles.Harlot },
                { "hunter", Game.roles.Hunter },
                { "mason", Game.roles.Mason },
                { "mayor", Game.roles.Mayor },
                { "prince", Game.roles.Prince },
                { "seer", Game.roles.Seer },
                { "seerfool", Game.roles.SeerFool },
                { "serialkiller", Game.roles.SerialKiller },
                { "sorcerer", Game.roles.Sorcerer },
                { "tanner", Game.roles.Tanner },
                { "traitor", Game.roles.Traitor },
                { "villager", Game.roles.Villager },
                { "werewolf", Game.roles.Werewolf },
                { "wildchild", Game.roles.WildChild },
                { "wolfcub", Game.roles.WolfCub },
            };

            foreach (var r in defaultaliasses) roleAliases.Remove(r.Key);

            if (!System.IO.Directory.Exists(basePath)) System.IO.Directory.CreateDirectory(basePath);
            System.IO.File.WriteAllText(aliasesPath, JsonConvert.SerializeObject(roleAliases));

            foreach (var r in defaultaliasses) roleAliases.Add(r.Key, r.Value);
        }

        private void getAliasesFromFile()
        {
            Dictionary<string, Game.roles> defaultaliasses = new Dictionary<string, Game.roles>()
            {
                { "alphawolf", Game.roles.AlphaWolf },
                { "apprenticeseer", Game.roles.ApprenticeSeer },
                { "beholder", Game.roles.Beholder },
                { "blacksmith", Game.roles.Blacksmith },
                { "clumsyguy", Game.roles.ClumsyGuy },
                { "cultist", Game.roles.Cultist },
                { "cultisthunter", Game.roles.CultistHunter },
                { "cupid", Game.roles.Cupid },
                { "cursed", Game.roles.Cursed },
                { "detective", Game.roles.Detective },
                { "doppelgänger", Game.roles.Doppelgänger },
                { "drunk", Game.roles.Drunk },
                { "fool", Game.roles.Fool },
                { "guardianangel", Game.roles.GuardianAngel },
                { "gunner", Game.roles.Gunner },
                { "harlot", Game.roles.Harlot },
                { "hunter", Game.roles.Hunter },
                { "mason", Game.roles.Mason },
                { "mayor", Game.roles.Mayor },
                { "prince", Game.roles.Prince },
                { "seer", Game.roles.Seer },
                { "seerfool", Game.roles.SeerFool },
                { "serialkiller", Game.roles.SerialKiller },
                { "sorcerer", Game.roles.Sorcerer },
                { "tanner", Game.roles.Tanner },
                { "traitor", Game.roles.Traitor },
                { "villager", Game.roles.Villager },
                { "werewolf", Game.roles.Werewolf },
                { "wildchild", Game.roles.WildChild },
                { "wolfcub", Game.roles.WolfCub },
            };

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

            foreach (KeyValuePair<string, Game.roles> r in defaultaliasses)
            {
                roleAliases.Add(r.Key, r.Value);
            }
        }


        public override bool StartBot()
        {
            getAliasesFromFile();
            ReadUsers();
            return base.StartBot();
        }

    }
}
