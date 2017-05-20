using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using BotControlPanel.Bots.WWTBCustomKeyboards;
using System.Net;
using System.IO;
using System.Windows;

namespace BotControlPanel.Bots
{
    public class WWTB : FlomBot
    {
        #region Constants
        public override string Name { get; } = "Werewolf Translation Bot";
        private readonly string botApiToken;
        private const string botUsername = "@werewufftransbot";
        private const string startMessage = "You've just sucessfully started the WereWuff Tranlation Bot!\n" +
            "It's still under development though xD";
        private const int flomsId = 267376056;
        #region Php urls
        private const string closedlistPhpUrl = "http://88.198.66.60/getClosedlist.php";
        private const string underdevPhpUrl = "http://88.198.66.60/getUnderdev.php";
        private const string addClosedlistPhpUrl = "http://88.198.66.60/addClosedlist.php";
        private const string editClosedlistPhpUrl = "http://88.198.66.60/editClosedlist.php";
        private const string removeFromClosedlistPhpUrl = "http://88.198.66.60/removeFromClosedlist.php";
        private const string addUnderdevPhpUrl = "http://88.198.66.60/addUnderdev.php";
        private const string editUnderdevPhpUrl = "http://88.198.66.60/editUnderdev.php";
        private const string removeFromUnderdevPhpUrl = "http://88.198.66.60/removeFromUnderdev.php";
        #endregion
#if DEBUG
        private const string channelUsername = "@werewufftranstestchannel";
        private const int messageIdClosedlist = 4;
        private const int messageIdUnderdev = 5;
#else
        private const string channelUsername = "@werewolftranslation";
        private const int messageIdClosedlist = 51;
        private const int messageIdUnderdev = 52;
#endif
        private const string adminIdsPath = "adminIds.txt";
        private const string closedlistHeader = "▶️ <b>LIST OF CLOSED LANGFILES</b> ◀️\n" +
                                                "<i>(by alphabetical order)</i>\n" +
                                                "\n";
        private const string underdevHeader = "▶️ <b>LANGFILES UNDER DEVELOPMENT</b> ◀️\n" +
                                              "\n";
        #endregion
        #region Variables
        private User me;
        private Dictionary<long, string> waitingFor = new Dictionary<long, string>();
        private Dictionary<long, string> chosenElement = new Dictionary<long, string>();
        #endregion

        public WWTB(string token) : base(token)
        {
            botApiToken = token;
            initialize();
        }

        #region Control Methods
        #region Init
        private void initialize()
        {
            #region Initializing stuff
            try
            {
                Task<User> ut = client.GetMeAsync();
                ut.Wait();
                me = ut.Result;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString() + e.Message + e.StackTrace);
            }
            //MessageBox.Show("Nullifying offline updates (bug source)");
            Task<Update[]> t = client.GetUpdatesAsync();
            t.Wait();
            if (t.Result.Length > 0) client.GetUpdatesAsync(t.Result[t.Result.Length - 1].Id);
            #endregion
        }
        #endregion
        #endregion

        #region Handlers
        #region Update Handler
        protected override void Client_OnUpdate(object sender, Telegram.Bot.Args.UpdateEventArgs e)
        {
            try
            {
                Update u = e.Update;
                #region Message Updates
                UpdateType workaround = UpdateType.UnkownUpdate;
                try
                {
                    workaround = u.Type;
                }
                catch(ArgumentOutOfRangeException)
                {
                    workaround = UpdateType.UnkownUpdate;
                }
                if (workaround == UpdateType.MessageUpdate)
                {
                    #region Text messages
                    if (u.Message.Type == MessageType.TextMessage
                    && u.Message.Chat.Type != ChatType.Channel)
                    {
                        #region Messages containing entities
                        if (u.Message.Entities.Count != 0)
                        {
                            #region Commands
                            if (u.Message.Entities[0].Type == MessageEntityType.BotCommand
                                && u.Message.Entities[0].Offset == 0)
                            {
                                #region Commands only
                                if (u.Message.Entities[0].Length == u.Message.Text.Length)
                                {
                                    handleCommandOnly(msg: u.Message, cmd: u.Message.Text);
                                }
                                #endregion
                                #region Commands with arguments
                                else
                                {
                                    handleCommandArgs(msg: u.Message, cmd: u.Message.Text.Split(' ')[0]);
                                }
                                #endregion
                            }
                            #endregion
                        }
                        #endregion

                        #region Text messages handling
                        handleTextMessage(u.Message);
                        #endregion
                    }
                    #endregion

                    #region System messages
                    #region New member
                    if (u.Message.Type == MessageType.ServiceMessage && u.Message.NewChatMember != null)
                    {
                        #region Bot added to group
                        if (u.Message.NewChatMember.Id == me.Id)
                        {
                            handleBotJoinedGroup(u.Message);
                        }
                        #endregion
                    }
                    #endregion
                    #endregion
                }
                #endregion
            }
            catch (Exception ex)
            {
#if DEBUG
                throw ex;
#else
                client.SendTextMessageAsync(flomsId, "An error has occurred: \n" + ex.ToString() + "\n"
                    + ex.Message + "\n" + ex.StackTrace);
                return;
#endif
            }
        }
#endregion

#region Commands
#region Commands Only
        private  void handleCommandOnly(Message msg, string cmd)
        {
            switch (cmd)
            {
                case "/start":
                case "/start" + botUsername:
                    IReplyMarkup rm = StartKeyboard.Markup;
                    client.SendTextMessageAsync(msg.Chat.Id, startMessage, replyMarkup: rm);
                    break;
            }
        }
#endregion

#region Commands with arguments
        private  void handleCommandArgs(Message msg, string cmd)
        {
            switch (cmd)
            {
                default:
                    break;
            }
        }
#endregion
#endregion

#region Text messages
        private  void handleTextMessage(Message msg)
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
                            client.SendTextMessageAsync(msg.Chat.Id, getCurrentClosedlist(),
                                replyMarkup: ClosedlistKeyboard.Markup, parseMode: ParseMode.Html);
                            break;
                        case UnderdevKeyboard.UnderdevAddButtonString:
                        case UnderdevKeyboard.UnderdevEditButtonString:
                        case UnderdevKeyboard.UnderdevEditButtonString + "_second":
                        case UnderdevKeyboard.UnderdevRemoveButtonString:
                            client.SendTextMessageAsync(msg.Chat.Id, getCurrentUnderdev(),
                                replyMarkup: UnderdevKeyboard.Markup, parseMode: ParseMode.Html);
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
                        if (addToClosedlist(msg.Text, out error))
                        {
                            client.SendTextMessageAsync(msg.Chat.Id, "Language added.");
                            client.SendTextMessageAsync(msg.Chat.Id, getCurrentClosedlist(),
                                replyMarkup: ClosedlistKeyboard.Markup, parseMode: ParseMode.Html);
                            waitingFor.Remove(msg.Chat.Id);
                            refreshMessages(msg);
                        }
                        else client.SendTextMessageAsync(msg.Chat.Id,
                            error);
                        break;
                    case ClosedlistKeyboard.ClosedlistEditButtonString:
                        Dictionary<string, string> dict = getCurrentClosedlistDict();
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
                        if (editClosedlist(chosenElement[msg.Chat.Id], msg.Text, out error2))
                        {
                            client.SendTextMessageAsync(msg.Chat.Id, "Language sucessfully edited.");
                            client.SendTextMessageAsync(msg.Chat.Id, getCurrentClosedlist(),
                                replyMarkup: ClosedlistKeyboard.Markup, parseMode: ParseMode.Html);
                            waitingFor.Remove(msg.Chat.Id);
                            chosenElement.Remove(msg.Chat.Id);
                            refreshMessages(msg);
                        }
                        else
                        {
                            client.SendTextMessageAsync(msg.Chat.Id, error2);
                        }
                        break;
                    case ClosedlistKeyboard.ClosedlistRemoveButtonString:
                        if (getCurrentClosedlistDict().ContainsKey(msg.Text))
                        {
                            string error3;
                            if (removeFromClosedlist(msg.Text, out error3))
                            {
                                client.SendTextMessageAsync(msg.Chat.Id, "Language sucessfully removed.");
                                client.SendTextMessageAsync(msg.Chat.Id, getCurrentClosedlist(),
                                    replyMarkup: ClosedlistKeyboard.Markup, parseMode: ParseMode.Html);
                                waitingFor.Remove(msg.Chat.Id);
                                refreshMessages(msg);
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
                        if (addToUnderdev(msg.Text, out error4))
                        {
                            client.SendTextMessageAsync(msg.Chat.Id, "Language added.");
                            client.SendTextMessageAsync(msg.Chat.Id, getCurrentUnderdev(),
                                replyMarkup: UnderdevKeyboard.Markup, parseMode: ParseMode.Html);
                            waitingFor.Remove(msg.Chat.Id);
                            refreshMessages(msg);
                        }
                        else client.SendTextMessageAsync(msg.Chat.Id,
                            error4);
                        break;
                    case UnderdevKeyboard.UnderdevEditButtonString:
                        if (getCurrentUnderdevDict().ContainsKey(msg.Text))
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
                        if (editUnderdev(chosenElement[msg.Chat.Id], msg.Text, out error5))
                        {
                            client.SendTextMessageAsync(msg.Chat.Id, "Language edited.");
                            client.SendTextMessageAsync(msg.Chat.Id, getCurrentUnderdev(),
                                replyMarkup: UnderdevKeyboard.Markup, parseMode: ParseMode.Html);
                            waitingFor.Remove(msg.Chat.Id);
                            chosenElement.Remove(msg.Chat.Id);
                            refreshMessages(msg);
                        }
                        else
                        {
                            client.SendTextMessageAsync(msg.Chat.Id, error5);
                        }
                        break;
                    case UnderdevKeyboard.UnderdevRemoveButtonString:
                        if (getCurrentUnderdevDict().ContainsKey(msg.Text))
                        {
                            string error6;
                            if (removeFromUnderdev(msg.Text, out error6))
                            {
                                client.SendTextMessageAsync(msg.Chat.Id, "Laguage removed.");
                                client.SendTextMessageAsync(msg.Chat.Id, getCurrentUnderdev(),
                                replyMarkup: UnderdevKeyboard.Markup, parseMode: ParseMode.Html);
                                waitingFor.Remove(msg.Chat.Id);
                                refreshMessages(msg);
                            }
                            else
                            {
                                client.SendTextMessageAsync(msg.Chat.Id, error6);
                            }
                        }
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
                    client.SendTextMessageAsync(msg.Chat.Id, getCurrentClosedlist(),
                        replyMarkup: ClosedlistKeyboard.Markup, parseMode: ParseMode.Html);
                    break;
                case StartKeyboard.UnderdevButtonString:
                    client.SendTextMessageAsync(msg.Chat.Id, getCurrentUnderdev(),
                        replyMarkup: UnderdevKeyboard.Markup, parseMode: ParseMode.Html);
                    break;
                case StartKeyboard.RefreshChannelMessageButtonString:
                    refreshMessages(msg);
                    break;
                case StartKeyboard.BackToStartKeyboardButtonString:
                    client.SendTextMessageAsync(msg.Chat.Id, "Main menu", replyMarkup: StartKeyboard.Markup);
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
                    ReplyKeyboardMarkup rkm = getClosedlistChooselangMarkup();
                    client.SendTextMessageAsync(msg.Chat.Id, "Choose a language to edit", replyMarkup: rkm);
                    waitingFor.Add(msg.Chat.Id, ClosedlistKeyboard.ClosedlistEditButtonString);
                    break;
                case ClosedlistKeyboard.ClosedlistRemoveButtonString:
                    ReplyKeyboardMarkup rkm2 = getClosedlistChooselangMarkup();
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
                    ReplyKeyboardMarkup rkm3 = getUnderdevChooselangMarkup();
                    client.SendTextMessageAsync(msg.Chat.Id, "Choose a language to edit", replyMarkup: rkm3);
                    waitingFor.Add(msg.Chat.Id, UnderdevKeyboard.UnderdevEditButtonString);
                    break;
                case UnderdevKeyboard.UnderdevRemoveButtonString:
                    ReplyKeyboardMarkup rkm4 = getUnderdevChooselangMarkup();
                    client.SendTextMessageAsync(msg.Chat.Id, "Choose a language to remove", replyMarkup: rkm4);
                    waitingFor.Add(msg.Chat.Id, UnderdevKeyboard.UnderdevRemoveButtonString);
                    break;
#endregion
            }
        }
#endregion

#region System messages
#region Bot joined Group
        private  void handleBotJoinedGroup(Message msg)
        {
            client.SendTextMessageAsync(msg.Chat.Id, "Please do not add me to any groups!").Wait();
            client.LeaveChatAsync(msg.Chat.Id);
        }
#endregion
#endregion
#endregion

#region Processing Methods
        private  void refreshMessages(Message msg)
        {
            client.EditMessageTextAsync(channelUsername, messageIdClosedlist, getCurrentClosedlist(),
                        parseMode: ParseMode.Html);
            client.EditMessageTextAsync(channelUsername, messageIdUnderdev, getCurrentUnderdev(),
                parseMode: ParseMode.Html);
            client.SendTextMessageAsync(msg.Chat.Id, "Message refreshed");
        }

        private  ReplyKeyboardMarkup getClosedlistChooselangMarkup()
        {
            Dictionary<string, string> dict = getCurrentClosedlistDict();
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

        private  ReplyKeyboardMarkup getUnderdevChooselangMarkup()
        {
            Dictionary<string, string> dict = getCurrentUnderdevDict();
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
        private  Dictionary<string, string> getCurrentClosedlistDict()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(closedlistPhpUrl);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream resStream = response.GetResponseStream();
            using (StreamReader sr = new StreamReader(resStream))
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

        private  Dictionary<string, string> getCurrentUnderdevDict()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(underdevPhpUrl);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream resStream = response.GetResponseStream();
            using (StreamReader sr = new StreamReader(resStream))
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

        private  string getCurrentClosedlist()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(closedlistPhpUrl);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream resStream = response.GetResponseStream();
            using (StreamReader sr = new StreamReader(resStream))
            {
                string result = sr.ReadToEnd().Replace("<br>", "\n");
                result = result.Replace(":", ": ");
                result = closedlistHeader + result;
                if (result != closedlistHeader) return result;
                else return "No entries in #closedlist yet";
            }
        }

        private  string getCurrentUnderdev()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(underdevPhpUrl);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream resStream = response.GetResponseStream();
            using (StreamReader sr = new StreamReader(resStream))
            {
                string result = sr.ReadToEnd().Replace("<br>", "\n");
                result = result.Replace(":", ": ");
                result = underdevHeader + result;
                if (result != underdevHeader) return result;
                else return "No entries in #underdev yet";
            }
        }
#endregion

#region Closedlist
#region Add
        private  bool addToClosedlist(string process, out string error)
        {
            string[] proc = process.Split('-');
            if (proc.Length != 2)
            {
                error = "Failed to add string, check format";
                return false;
            }
            string lang = proc[0].Trim();
            string info = proc[1].Trim();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(addClosedlistPhpUrl + "?lang=" + lang
                + "&info=" + info);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream resStream = response.GetResponseStream();
            using (StreamReader sr = new StreamReader(resStream))
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
        private  bool editClosedlist(string lang, string process, out string error)
        {
            string[] proc = process.Split('-');
            if (proc.Length != 2)
            {
                error = "Failed to edit string, check format";
                return false;
            }
            string newLang = proc[0].Trim();
            string info = proc[1].Trim();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(editClosedlistPhpUrl + "?lang=" + lang
                + "&newlang=" + newLang + "&info=" + info);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream resStream = response.GetResponseStream();
            using (StreamReader sr = new StreamReader(resStream))
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
        private  bool removeFromClosedlist(string process, out string error)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(removeFromClosedlistPhpUrl
                + "?lang=" + process);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream resStream = response.GetResponseStream();
            using (StreamReader sr = new StreamReader(resStream))
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
        private  bool addToUnderdev(string process, out string error)
        {
            string[] proc = process.Split('-');
            if (proc.Length != 2)
            {
                error = "Failed to add string, check format";
                return false;
            }
            string lang = proc[0].Trim();
            string info = proc[1].Trim();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(addUnderdevPhpUrl + "?lang=" + lang
                + "&info=" + info);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream resStream = response.GetResponseStream();
            using (StreamReader sr = new StreamReader(resStream))
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
        private  bool editUnderdev(string lang, string process, out string error)
        {
            string[] proc = process.Split('-');
            if (proc.Length != 2)
            {
                error = "Failed to edit string, check format";
                return false;
            }
            string newLang = proc[0].Trim();
            string info = proc[1].Trim();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(editUnderdevPhpUrl + "?lang=" + lang
                + "&newlang=" + newLang + "&info=" + info);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream resStream = response.GetResponseStream();
            using (StreamReader sr = new StreamReader(resStream))
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
        private  bool removeFromUnderdev(string process, out string error)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(removeFromUnderdevPhpUrl
                + "?lang=" + process);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream resStream = response.GetResponseStream();
            using (StreamReader sr = new StreamReader(resStream))
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
