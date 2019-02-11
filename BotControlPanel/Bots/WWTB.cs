using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BotControlPanel.Bots.WWTBCustomKeyboards;
using System.Net;
using System.IO;
using System.Windows;
using Telegraph.Net.Models;
using Telegraph.Net;
using System.Globalization;
using System.Text.RegularExpressions;
using FlomBotFactory;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Args;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotControlPanel.Bots
{
    public class Wwtb : FlomBot
    {
        #region Constants
        public override string Name { get; } = "Werewolf Translation Bot";
        private const string BotUsername = "@werewufftransbot";
        private const string StartMessage = "You've just sucessfully started the WereWuff Tranlation Bot!\n" +
            "It's still under development though xD";
        private const string TelegraphToken = "6f903e8307b7e8535ec93352f3f7edab2f5ea3f4c9c72ec1f74d770543e2";
        private const string TelegraphPagePath = "Changelog-05-22";
        #region Php urls
        private const string ClosedlistPhpUrl = "http://localhost/getClosedlist.php";
        private const string UnderdevPhpUrl = "http://localhost/getUnderdev.php";
        private const string AddClosedlistPhpUrl = "http://localhost/addClosedlist.php";
        private const string EditClosedlistPhpUrl = "http://localhost/editClosedlist.php";
        private const string RemoveFromClosedlistPhpUrl = "http://localhost/removeFromClosedlist.php";
        private const string AddUnderdevPhpUrl = "http://localhost/addUnderdev.php";
        private const string EditUnderdevPhpUrl = "http://localhost/editUnderdev.php";
        private const string RemoveFromUnderdevPhpUrl = "http://localhost/removeFromUnderdev.php";
        #endregion
#if DEBUG
        private const string ChannelUsername = "@werewufftranstestchannel";
        private const int MessageIdClosedlist = 4;
        private const int MessageIdUnderdev = 5;
#else
        private const string ChannelUsername = "@werewolftranslation";
        private const int MessageIdClosedlist = 15;
        private const int MessageIdUnderdev = 16;
#endif
        private const string BasePath = "C:\\Olfi01\\BotControlPanel\\WWTB\\";
        private const string AdminIdsPath = BasePath + "adminIds.txt";
        private const string ClosedlistHeader = "▶️ <b>LIST OF CLOSED LANGFILES</b> ◀️\n" +
                                                "<i>(by alphabetical order)</i>\n" +
                                                "\n";
        private const string UnderdevHeader = "▶️ <b>LANGFILES UNDER DEVELOPMENT</b> ◀️\n" +
                                              "\n";
        #endregion
        #region Variables
        private User me;
        private Dictionary<long, string> waitingFor = new Dictionary<long, string>();
        private Dictionary<long, string> chosenElement = new Dictionary<long, string>();
        private Page changelogPage;
        private TelegraphClient tClient;
        private ITokenClient ttClient;
        private List<string> AdminIds { get; set; } = new List<string>();
        #endregion

        public Wwtb(string token) : base(token)
        {
            Initialize().ConfigureAwait(false);
        }

        #region Control Methods
        #region Init
        private async Task Initialize()
        {
            #region Initializing stuff
            if (!Directory.Exists(BasePath)) Directory.CreateDirectory(BasePath);
            try
            {
                me = await client.GetMeAsync();
            }
            catch (Exception e)
            {
                MessageBox.Show(e + e.Message + e.StackTrace);
            }
            /*Task<Update[]> t = client.GetUpdatesAsync();
            t.Wait();
            if (t.Result.Length > 0) client.GetUpdatesAsync(t.Result[t.Result.Length - 1].Id);*/
            tClient = new TelegraphClient();
            ttClient = tClient.GetTokenClient(TelegraphToken);
            if (System.IO.File.Exists(AdminIdsPath))
            {
                foreach (string s in System.IO.File.ReadAllLines(AdminIdsPath))
                {
                    AdminIds.Add(s);
                }
            }
            else
            {
                AdminIds.Add(Flom.ToString());
                AdminIds.Add("133748469");
                System.IO.File.WriteAllLines(AdminIdsPath, AdminIds);
            }
            #endregion
        }
        #endregion
        #region Get Link Header
        private NodeElement GetLinkHeader()
        {
            var newNode = new NodeElement("p", null);
            foreach (var node in changelogPage.Content)
            {
                if (node.Tag == "h3")
                {
                    newNode.Children.Add(new NodeElement("a",
                        new Dictionary<string, string>() { { "href", $"#{node.Children[0].Attributes["value"].Replace(" ", "-")}" } },
                        node.Children[0].Attributes["value"]));
                    newNode.Children.Add(", ");
                }
            }
            return newNode;
        }
        #endregion
        #endregion

        #region Handlers
        #region Update Handler
        protected override void Client_OnUpdate(object sender, UpdateEventArgs e)
        {
            if (!AdminIds.Contains(e.Update.Message.From.Id.ToString())) return;
            try
            {
                Update u = e.Update;
                #region Message Updates
                if (e.Update.Type == UpdateType.Message)
                {
                    #region Text messages
                    if (u.Message.Type == MessageType.Text
                    && u.Message.Chat.Type != ChatType.Channel)
                    {
                        #region Messages containing entities
                        if (u.Message.Entities.Length > 0)
                        {
                            #region Commands
                            if (u.Message.Entities[0]?.Type == MessageEntityType.BotCommand
                                && u.Message.Entities[0].Offset == 0)
                            {
                                #region Commands only
                                if (u.Message.Entities[0].Length == u.Message.Text.Length)
                                {
                                    HandleCommandOnly(msg: u.Message, cmd: u.Message.Text);
                                }
                                #endregion
                                #region Commands with arguments
                                else
                                {
                                    HandleCommandArgs(msg: u.Message, cmd: u.Message.Text.Split(' ')[0]);
                                }
                                #endregion
                            }
                            #endregion
                        }
                        #endregion

                        #region Text messages handling
                        HandleText(u.Message);
                        #endregion
                    }
                    #endregion

                    #region System messages
                    #region New member
                    if (u.Message.Type == MessageType.ChatMembersAdded && u.Message.NewChatMembers != null)
                    {
                        #region Bot added to group
                        if (u.Message.NewChatMembers[0].Id == me.Id)
                        {
                            HandleBotJoinedGroup(u.Message);
                        }
                        #endregion
                    }
                    #endregion
                    #endregion
                }
                #endregion
            }
#if DEBUG
            catch (IOException ex)
#else
            catch (Exception ex)
#endif
            {
                client.SendTextMessageAsync(Flom, "An error has occurred: \n" + ex.ToString() + "\n"
                    + ex.Message + "\n" + ex.StackTrace);
                return;
            }
        }
        #endregion

        #region Commands
        #region Commands Only
        private void HandleCommandOnly(Message msg, string cmd)
        {
            switch (cmd)
            {
                case "/start":
                case "/start" + BotUsername:
                    ReplyMarkupBase rm = StartKeyboard.Markup;
                    client.SendTextMessageAsync(msg.Chat.Id, StartMessage, replyMarkup: rm);
                    break;
            }
        }
        #endregion

        #region Commands with arguments
        private void HandleCommandArgs(Message msg, string cmd)
        {
            switch (cmd)
            {
                case "/addadmin":
                    if (!AdminIds.Contains(msg.From.Id.ToString())) return;
                    string id = msg.Text.Substring(cmd.Length).Trim();
                    AdminIds.Add(id);
                    System.IO.File.WriteAllLines(AdminIdsPath, AdminIds);
                    client.SendTextMessageAsync(msg.Chat.Id, "Added as administrator.");
                    break;
                default:
                    break;
            }
        }
        #endregion
        #endregion

        #region Text messages
        private void HandleText(Message msg)
        {
            if (waitingFor.ContainsKey(msg.Chat.Id))
            {
                #region Waiting for something
                if (msg.Text == CancelKeyboard.CancelButtonString)
                {
                    #region Return to old keyboard
                    switch (waitingFor[msg.Chat.Id])
                    {
                        case ClosedlistKeyboard.ClosedlistAddButtonString:
                        case ClosedlistKeyboard.ClosedlistEditButtonString:
                        case ClosedlistKeyboard.ClosedlistEditButtonString + "_second":
                        case ClosedlistKeyboard.ClosedlistRemoveButtonString:
                            client.SendTextMessageAsync(msg.Chat.Id, GetCurrentClosedlist(),
                                replyMarkup: ClosedlistKeyboard.Markup, parseMode: ParseMode.Html);
                            break;
                        case UnderdevKeyboard.UnderdevAddButtonString:
                        case UnderdevKeyboard.UnderdevEditButtonString:
                        case UnderdevKeyboard.UnderdevEditButtonString + "_second":
                        case UnderdevKeyboard.UnderdevRemoveButtonString:
                            client.SendTextMessageAsync(msg.Chat.Id, GetCurrentUnderdev(),
                                replyMarkup: UnderdevKeyboard.Markup, parseMode: ParseMode.Html);
                            break;
                        case ChangelogKeyboard.AddPostToChangelogString:
                            client.SendTextMessageAsync(msg.Chat.Id, "You can edit the changelog here",
                                replyMarkup: ChangelogKeyboard.Markup);
                            break;
                    }
                    #endregion
                    waitingFor.Remove(msg.Chat.Id);
                    if (chosenElement.ContainsKey(msg.Chat.Id)) chosenElement.Remove(msg.Chat.Id);
                    return;
                }
                switch (waitingFor[msg.Chat.Id])
                {
                    #region Closedlist
                    case ClosedlistKeyboard.ClosedlistAddButtonString:
                        string error;
                        if (AddToClosedlist(msg.Text, out error))
                        {
                            client.SendTextMessageAsync(msg.Chat.Id, "Language added.");
                            client.SendTextMessageAsync(msg.Chat.Id, GetCurrentClosedlist(),
                                replyMarkup: ClosedlistKeyboard.Markup, parseMode: ParseMode.Html);
                            waitingFor.Remove(msg.Chat.Id);
                            RefreshMessages(msg);
                        }
                        else client.SendTextMessageAsync(msg.Chat.Id,
                            error);
                        break;
                    case ClosedlistKeyboard.ClosedlistEditButtonString:
                        Dictionary<string, string> dict = GetCurrentClosedlistDict();
                        if (dict.ContainsKey(msg.Text))
                        {
                            chosenElement.Add(msg.Chat.Id, msg.Text);
                            client.SendTextMessageAsync(msg.Chat.Id,
                                "Please enter the new value in the following format:\n" +
                                "Language name - Information", replyMarkup: CancelKeyboard.Markup);
                            waitingFor.Remove(msg.Chat.Id);
                            waitingFor.Add(msg.Chat.Id,
                                ClosedlistKeyboard.ClosedlistEditButtonString + "_second");
                        }
                        else
                        {
                            client.SendTextMessageAsync(msg.Chat.Id, "This language doesn't exist. Try again.");
                        }
                        break;
                    case ClosedlistKeyboard.ClosedlistEditButtonString + "_second":
                        string error2;
                        if (EditClosedlist(chosenElement[msg.Chat.Id], msg.Text, out error2))
                        {
                            client.SendTextMessageAsync(msg.Chat.Id, "Language sucessfully edited.");
                            client.SendTextMessageAsync(msg.Chat.Id, GetCurrentClosedlist(),
                                replyMarkup: ClosedlistKeyboard.Markup, parseMode: ParseMode.Html);
                            waitingFor.Remove(msg.Chat.Id);
                            chosenElement.Remove(msg.Chat.Id);
                            RefreshMessages(msg);
                        }
                        else
                        {
                            client.SendTextMessageAsync(msg.Chat.Id, error2);
                        }
                        break;
                    case ClosedlistKeyboard.ClosedlistRemoveButtonString:
                        if (GetCurrentClosedlistDict().ContainsKey(msg.Text))
                        {
                            string error3;
                            if (RemoveFromClosedlist(msg.Text, out error3))
                            {
                                client.SendTextMessageAsync(msg.Chat.Id, "Language sucessfully removed.");
                                client.SendTextMessageAsync(msg.Chat.Id, GetCurrentClosedlist(),
                                    replyMarkup: ClosedlistKeyboard.Markup, parseMode: ParseMode.Html);
                                waitingFor.Remove(msg.Chat.Id);
                                RefreshMessages(msg);
                            }
                            else
                            {
                                client.SendTextMessageAsync(msg.Chat.Id, error3);
                            }
                        }
                        else
                        {
                            client.SendTextMessageAsync(msg.Chat.Id, "That language does't exist. Try again.");
                        }
                        break;
                    #endregion

                    #region Underdev
                    case UnderdevKeyboard.UnderdevAddButtonString:
                        string error4;
                        if (AddToUnderdev(msg.Text, out error4))
                        {
                            client.SendTextMessageAsync(msg.Chat.Id, "Language added.");
                            client.SendTextMessageAsync(msg.Chat.Id, GetCurrentUnderdev(),
                                replyMarkup: UnderdevKeyboard.Markup, parseMode: ParseMode.Html);
                            waitingFor.Remove(msg.Chat.Id);
                            RefreshMessages(msg);
                        }
                        else client.SendTextMessageAsync(msg.Chat.Id,
                            error4);
                        break;
                    case UnderdevKeyboard.UnderdevEditButtonString:
                        if (GetCurrentUnderdevDict().ContainsKey(msg.Text))
                        {
                            client.SendTextMessageAsync(msg.Chat.Id,
                                "Send me the new information in the following format:\n" +
                                "Language name - Information", replyMarkup: CancelKeyboard.Markup);
                            waitingFor.Remove(msg.Chat.Id);
                            waitingFor.Add(msg.Chat.Id, UnderdevKeyboard.UnderdevEditButtonString + "_second");
                            chosenElement.Add(msg.Chat.Id, msg.Text);
                        }
                        else
                        {
                            client.SendTextMessageAsync(msg.Chat.Id, "That language doesn't exist. Try again.");
                        }
                        break;
                    case UnderdevKeyboard.UnderdevEditButtonString + "_second":
                        string error5;
                        if (EditUnderdev(chosenElement[msg.Chat.Id], msg.Text, out error5))
                        {
                            client.SendTextMessageAsync(msg.Chat.Id, "Language edited.");
                            client.SendTextMessageAsync(msg.Chat.Id, GetCurrentUnderdev(),
                                replyMarkup: UnderdevKeyboard.Markup, parseMode: ParseMode.Html);
                            waitingFor.Remove(msg.Chat.Id);
                            chosenElement.Remove(msg.Chat.Id);
                            RefreshMessages(msg);
                        }
                        else
                        {
                            client.SendTextMessageAsync(msg.Chat.Id, error5);
                        }
                        break;
                    case UnderdevKeyboard.UnderdevRemoveButtonString:
                        if (GetCurrentUnderdevDict().ContainsKey(msg.Text))
                        {
                            string error6;
                            if (RemoveFromUnderdev(msg.Text, out error6))
                            {
                                client.SendTextMessageAsync(msg.Chat.Id, "Laguage removed.");
                                client.SendTextMessageAsync(msg.Chat.Id, GetCurrentUnderdev(),
                                replyMarkup: UnderdevKeyboard.Markup, parseMode: ParseMode.Html);
                                waitingFor.Remove(msg.Chat.Id);
                                RefreshMessages(msg);
                            }
                            else
                            {
                                client.SendTextMessageAsync(msg.Chat.Id, error6);
                            }
                        }
                        break;
                    #endregion

                    #region Changelog
                    case ChangelogKeyboard.AddPostToChangelogString:
#if DEBUG
                        DateTime now = new DateTime(2017, 5, 21);
#else
                                DateTime now = DateTime.Now;
#endif
                        var t = tClient.GetPageAsync(TelegraphPagePath, true);
                        t.Wait();
                        changelogPage = t.Result;
                        CultureInfo enUS = new CultureInfo("en-US");
                        var node = new NodeElement("p", null, $"{now.ToString("dd MMM", enUS)} - ");
                        Regex regex = new Regex(@"_(\S)+_");
                        string text = msg.Text;
                        foreach (var match in regex.Matches(text))
                        {
                            int index = text.IndexOf(match.ToString());
                            node.Children.Add(text.Remove(index));
                            text = text.Substring(index);
                            node.Children.Add(new NodeElement("code", null, match.ToString().Trim('_')));
                            text = text.Substring(match.ToString().Length);
                        }
                        node.Children.Add(text);
                        NodeElement found =
                            changelogPage.Content.Find(x => x.Tag == "h3"
                            && x.Children.Find(y => y.Tag == "_text" && y.Attributes.ContainsKey("value") &&
                            y.Attributes["value"] == $"{now.ToString("MMMM", enUS)} {now.Year}") != null);
                        if (found != null)
                        {
                            changelogPage.Content.Insert(changelogPage.Content.IndexOf(found) + 1,
                                node);
                        }
                        else
                        {
                            found = changelogPage.Content.Find(x => x.Tag == "h3");
                            int index = changelogPage.Content.IndexOf(found);
                            changelogPage.Content.Insert(index,
                                new NodeElement("h3", null, $"{now.ToString("MMMM", enUS)} {now.Year}"));
                            changelogPage.Content.Insert(index + 1,
                                node);
                        }
                        changelogPage.Content.RemoveAt(0);
                        changelogPage.Content.Insert(0, GetLinkHeader());
                        ttClient.EditPageAsync(TelegraphPagePath, changelogPage.Title, changelogPage.Content.ToArray(),
                            changelogPage.AuthorName, changelogPage.AuthorUrl).Wait();
                        client.SendTextMessageAsync(msg.Chat.Id, "Added.", replyMarkup: ChangelogKeyboard.Markup);
                        waitingFor.Remove(msg.Chat.Id);
                        break;
                        #endregion
                }
                return;
                #endregion
            }
            switch (msg.Text)
            {
                #region Start keyboard
                case StartKeyboard.ClosedlistButtonString:
                    client.SendTextMessageAsync(msg.Chat.Id, GetCurrentClosedlist(),
                        replyMarkup: ClosedlistKeyboard.Markup, parseMode: ParseMode.Html);
                    break;
                case StartKeyboard.UnderdevButtonString:
                    client.SendTextMessageAsync(msg.Chat.Id, GetCurrentUnderdev(),
                        replyMarkup: UnderdevKeyboard.Markup, parseMode: ParseMode.Html);
                    break;
                case StartKeyboard.RefreshChannelMessageButtonString:
                    RefreshMessages(msg);
                    break;
                case StartKeyboard.BackToStartKeyboardButtonString:
                    client.SendTextMessageAsync(msg.Chat.Id, "Main menu", replyMarkup: StartKeyboard.Markup);
                    break;
                case StartKeyboard.EditChangelogString:
                    client.SendTextMessageAsync(msg.Chat.Id,
                        $"You can view the changelog at telegra.ph/{TelegraphPagePath}",
                        replyMarkup: ChangelogKeyboard.Markup);
                    break;
                #endregion

                #region Closedlist keyboard
                case ClosedlistKeyboard.ClosedlistAddButtonString:
                    client.SendTextMessageAsync(msg.Chat.Id,
                        "Send me the language you want to add in the following format: \n" +
                        "Language name - Information",
                        replyMarkup: CancelKeyboard.Markup);
                    waitingFor.Add(msg.Chat.Id, ClosedlistKeyboard.ClosedlistAddButtonString);
                    break;
                case ClosedlistKeyboard.ClosedlistEditButtonString:
                    ReplyKeyboardMarkup rkm = GetClosedlistChooselangMarkup();
                    client.SendTextMessageAsync(msg.Chat.Id, "Choose a language to edit", replyMarkup: rkm);
                    waitingFor.Add(msg.Chat.Id, ClosedlistKeyboard.ClosedlistEditButtonString);
                    break;
                case ClosedlistKeyboard.ClosedlistRemoveButtonString:
                    ReplyKeyboardMarkup rkm2 = GetClosedlistChooselangMarkup();
                    client.SendTextMessageAsync(msg.Chat.Id, "Choose a language to remove", replyMarkup: rkm2);
                    waitingFor.Add(msg.Chat.Id, ClosedlistKeyboard.ClosedlistRemoveButtonString);
                    break;
                #endregion

                #region Underdev keyboard
                case UnderdevKeyboard.UnderdevAddButtonString:
                    client.SendTextMessageAsync(msg.Chat.Id,
                        "Send me the language you want to add in the following format: \n" +
                        "Language name - Information",
                        replyMarkup: CancelKeyboard.Markup);
                    waitingFor.Add(msg.Chat.Id, UnderdevKeyboard.UnderdevAddButtonString);
                    break;
                case UnderdevKeyboard.UnderdevEditButtonString:
                    ReplyKeyboardMarkup rkm3 = GetUnderdevChooselangMarkup();
                    client.SendTextMessageAsync(msg.Chat.Id, "Choose a language to edit", replyMarkup: rkm3);
                    waitingFor.Add(msg.Chat.Id, UnderdevKeyboard.UnderdevEditButtonString);
                    break;
                case UnderdevKeyboard.UnderdevRemoveButtonString:
                    ReplyKeyboardMarkup rkm4 = GetUnderdevChooselangMarkup();
                    client.SendTextMessageAsync(msg.Chat.Id, "Choose a language to remove", replyMarkup: rkm4);
                    waitingFor.Add(msg.Chat.Id, UnderdevKeyboard.UnderdevRemoveButtonString);
                    break;
                #endregion

                #region Changelog Keyboard
                case ChangelogKeyboard.AddPostToChangelogString:
                    client.SendTextMessageAsync(msg.Chat.Id, "Send me the new entry for the changelog.\n"
                                                             + "Date will be added automatically.",
                                                             replyMarkup: CancelKeyboard.Markup);
                    if (waitingFor.ContainsKey(msg.Chat.Id)) waitingFor.Remove(msg.Chat.Id);
                    waitingFor.Add(msg.Chat.Id, ChangelogKeyboard.AddPostToChangelogString);
                    break;
                    #endregion
            }
        }
        #endregion

        #region System messages
        #region Bot joined Group
        private void HandleBotJoinedGroup(Message msg)
        {
            client.SendTextMessageAsync(msg.Chat.Id, "Please do not add me to any groups!").Wait();
            client.LeaveChatAsync(msg.Chat.Id);
        }
        #endregion
        #endregion
        #endregion

        #region Processing Methods
        private void RefreshMessages(Message msg)
        {
            client.EditMessageTextAsync(ChannelUsername, MessageIdClosedlist, GetCurrentClosedlist(),
                        parseMode: ParseMode.Html);
            client.EditMessageTextAsync(ChannelUsername, MessageIdUnderdev, GetCurrentUnderdev(),
                parseMode: ParseMode.Html);
            client.SendTextMessageAsync(msg.Chat.Id, "Message refreshed");
        }

        private ReplyKeyboardMarkup GetClosedlistChooselangMarkup()
        {
            Dictionary<string, string> dict = GetCurrentClosedlistDict();
            KeyboardButton[][] arrayarray = new KeyboardButton[dict.Count + 1][];
            int i = 0;
            foreach (KeyValuePair<string, string> kvp in dict)
            {
                KeyboardButton b = new KeyboardButton(kvp.Key);
                KeyboardButton[] row = { b };
                arrayarray[i] = row;
                i++;
            }
            KeyboardButton b2 = CancelKeyboard.CancelButton;
            KeyboardButton[] row2 = { b2 };
            arrayarray[i] = row2;
            return new ReplyKeyboardMarkup(arrayarray);
        }

        private ReplyKeyboardMarkup GetUnderdevChooselangMarkup()
        {
            Dictionary<string, string> dict = GetCurrentUnderdevDict();
            KeyboardButton[][] arrayarray = new KeyboardButton[dict.Count + 1][];
            int i = 0;
            foreach (KeyValuePair<string, string> kvp in dict)
            {
                KeyboardButton b = new KeyboardButton(kvp.Key);
                KeyboardButton[] row = { b };
                arrayarray[i] = row;
                i++;
            }
            KeyboardButton b2 = CancelKeyboard.CancelButton;
            KeyboardButton[] row2 = { b2 };
            arrayarray[i] = row2;
            return new ReplyKeyboardMarkup(arrayarray);
        }
        #endregion

        #region SQL methods
        #region Getters
        #region Dict getters
        private Dictionary<string, string> GetCurrentClosedlistDict()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ClosedlistPhpUrl);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream resStream = response.GetResponseStream();
            using (StreamReader sr = new StreamReader(resStream ?? Stream.Null))
            {
                string[] result = sr.ReadToEnd().Replace("<br>", "\n").Split('\n');
                Dictionary<string, string> dict = new Dictionary<string, string>();
                foreach (string s in result)
                {
                    string[] a = s.Split(':');
                    if (a.Length != 2) continue;
                    dict.Add(a[0], a[1]);
                }
                return dict;
            }
        }

        private Dictionary<string, string> GetCurrentUnderdevDict()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UnderdevPhpUrl);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream resStream = response.GetResponseStream();
            using (StreamReader sr = new StreamReader(resStream ?? Stream.Null))
            {
                string[] result = sr.ReadToEnd().Replace("<br>", "\n").Split('\n');
                Dictionary<string, string> dict = new Dictionary<string, string>();
                foreach (string s in result)
                {
                    string[] a = s.Split(':');
                    if (a.Length != 2) continue;
                    dict.Add(a[0], a[1]);
                }
                return dict;
            }
        }
        #endregion

        private string GetCurrentClosedlist()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ClosedlistPhpUrl);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream resStream = response.GetResponseStream();
            using (StreamReader sr = new StreamReader(resStream ?? Stream.Null))
            {
                string result = sr.ReadToEnd().Replace("<br>", "\n");
                result = result.Replace(":", ": ");
                result = ClosedlistHeader + result;
                if (result != ClosedlistHeader) return result;
                else return "No entries in #closedlist yet";
            }
        }

        private string GetCurrentUnderdev()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UnderdevPhpUrl);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream resStream = response.GetResponseStream();
            using (StreamReader sr = new StreamReader(resStream ?? Stream.Null))
            {
                string result = sr.ReadToEnd().Replace("<br>", "\n");
                result = result.Replace(":", ": ");
                result = UnderdevHeader + result;
                if (result != UnderdevHeader) return result;
                else return "No entries in #underdev yet";
            }
        }
        #endregion

        #region Closedlist
        #region Add
        private bool AddToClosedlist(string process, out string error)
        {
            string[] proc = process.Split('-');
            if (proc.Length != 2)
            {
                error = "Failed to add string, check format";
                return false;
            }
            string lang = proc[0].Trim();
            string info = proc[1].Trim();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(AddClosedlistPhpUrl + "?lang=" + lang
                + "&info=" + info);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream resStream = response.GetResponseStream();
            using (StreamReader sr = new StreamReader(resStream ?? Stream.Null))
            {
                string res = sr.ReadToEnd();
                if (res == "true")
                {
                    error = null;
                    return true;
                }
                else
                {
                    string[] ret = res.Replace("<br>", "\n").Split('\n');
                    if (ret[0] == "1062")
                    {
                        error = "Failed to add language entry, it is already present. Try again.";
                    }
                    else
                    {
                        error = ret[1];
                    }
                    return false;
                }
            }
        }
        #endregion

        #region Edit
        private bool EditClosedlist(string lang, string process, out string error)
        {
            string[] proc = process.Split('-');
            if (proc.Length != 2)
            {
                error = "Failed to edit string, check format";
                return false;
            }
            string newLang = proc[0].Trim();
            string info = proc[1].Trim();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(EditClosedlistPhpUrl + "?lang=" + lang
                + "&newlang=" + newLang + "&info=" + info);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream resStream = response.GetResponseStream();
            using (StreamReader sr = new StreamReader(resStream ?? Stream.Null))
            {
                string res = sr.ReadToEnd();
                if (res == "true")
                {
                    error = null;
                    return true;
                }
                else
                {
                    string[] ret = res.Replace("<br>", "\n").Split('\n');
                    error = ret[1];
                    return false;
                }
            }
        }
        #endregion

        #region Remove
        private bool RemoveFromClosedlist(string process, out string error)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(RemoveFromClosedlistPhpUrl
                + "?lang=" + process);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream resStream = response.GetResponseStream();
            using (StreamReader sr = new StreamReader(resStream ?? Stream.Null))
            {
                string res = sr.ReadToEnd();
                if (res == "true")
                {
                    error = null;
                    return true;
                }
                else
                {
                    string[] ret = res.Replace("<br>", "\n").Split('\n');
                    error = ret[1];
                    return false;
                }
            }
        }
        #endregion
        #endregion

        #region Underdev
        #region Add
        private bool AddToUnderdev(string process, out string error)
        {
            string[] proc = process.Split('-');
            if (proc.Length != 2)
            {
                error = "Failed to add string, check format";
                return false;
            }
            string lang = proc[0].Trim();
            string info = proc[1].Trim();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(AddUnderdevPhpUrl + "?lang=" + lang
                + "&info=" + info);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream resStream = response.GetResponseStream();
            using (StreamReader sr = new StreamReader(resStream ?? Stream.Null))
            {
                string res = sr.ReadToEnd();
                if (res == "true")
                {
                    error = null;
                    return true;
                }
                else
                {
                    string[] ret = res.Replace("<br>", "\n").Split('\n');
                    if (ret[0] == "1062")
                    {
                        error = "Failed to add language entry, it is already present. Try again.";
                    }
                    else
                    {
                        error = ret[1];
                    }
                    return false;
                }
            }
        }
        #endregion

        #region Edit
        private bool EditUnderdev(string lang, string process, out string error)
        {
            string[] proc = process.Split('-');
            if (proc.Length != 2)
            {
                error = "Failed to edit string, check format";
                return false;
            }
            string newLang = proc[0].Trim();
            string info = proc[1].Trim();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(EditUnderdevPhpUrl + "?lang=" + lang
                + "&newlang=" + newLang + "&info=" + info);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream resStream = response.GetResponseStream();
            using (StreamReader sr = new StreamReader(resStream ?? Stream.Null))
            {
                string res = sr.ReadToEnd();
                if (res == "true")
                {
                    error = null;
                    return true;
                }
                else
                {
                    string[] ret = res.Replace("<br>", "\n").Split('\n');
                    error = ret[1];
                    return false;
                }
            }
        }
        #endregion

        #region Remove
        private bool RemoveFromUnderdev(string process, out string error)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(RemoveFromUnderdevPhpUrl
                + "?lang=" + process);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream resStream = response.GetResponseStream();
            using (StreamReader sr = new StreamReader(resStream ?? Stream.Null))
            {
                string res = sr.ReadToEnd();
                if (res == "true")
                {
                    error = null;
                    return true;
                }
                else
                {
                    string[] ret = res.Replace("<br>", "\n").Split('\n');
                    error = ret[1];
                    return false;
                }
            }
        }
        #endregion
        #endregion
        #endregion
    }
}
