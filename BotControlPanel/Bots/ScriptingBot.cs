using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotControlPanel.Bots
{
    public class ScriptingBot : FlomBot
    {
        #region Constants
        public override string Name { get; } = "Scripting Bot";
        private static readonly string basePath = "C:\\Olfi01\\BotControlPanel\\ScriptingBot\\";
        private static readonly string startPath = Path.Combine(Environment.CurrentDirectory, "ScriptBot\\");
        private static readonly string scriptPath = Path.Combine(basePath, "Start.cs");
        private static readonly string scriptSecondPath = Path.Combine(basePath, "StartCopy.cs");
        private static readonly string compilePath = Path.Combine(basePath, "compile.bat");
        private static readonly string execPath = Path.Combine(basePath, "test.exe");
        private static readonly string dllPath = Path.Combine(basePath, "Telegram.Bot.dll");
        private const string tokenBasePath = "C:\\Olfi01\\BotControlPanel\\ScriptingBot\\";
        private const string tokenPath = tokenBasePath + "scriptedToken.token";
        #endregion
        #region Variables
        private string scriptedBotToken;
        #endregion

        #region Constructor
        public ScriptingBot() : base()
        {
            scriptedBotToken = getScriptedBotToken();
            if (!System.IO.File.Exists(scriptPath))
            {
                if (!Directory.Exists(basePath)) Directory.CreateDirectory(basePath);
                System.IO.File.Copy(startPath + "Start.cs", scriptPath);
                System.IO.File.Copy(startPath + "compile.bat", compilePath);
                System.IO.File.Copy(startPath + "Telegram.Bot.dll", dllPath);
            }
        }
        #endregion

        #region On Update
        protected override void Client_OnUpdate(object sender, UpdateEventArgs e)
        {
            try
            {
                if (e.Update.Type == UpdateType.MessageUpdate
                    && e.Update.Message.Type == MessageType.TextMessage)
                {
                    Message msg = e.Update.Message;
                    string text = msg.Text;
                    #region Commands
                    string cmd = text.Contains("@")
                        ? text.Remove(text.IndexOf('@')).ToLower()
                        : (text.Contains(" ")
                            ?text.Remove(text.IndexOf(' '))
                            :text.ToLower());
                    switch (cmd)
                    {
                        #region newcommand
                        case "/newcommand":
                            if (msg.From.Id == Flom)
                            {
                                string error = "";
                                if (!text.Contains(" ")) return;
                                string code = msg.ReplyToMessage == null
                                    ? text.Substring(text.IndexOf(' ') + 1)
                                    : msg.ReplyToMessage.Text;
                                if (newCommand(code, out error))
                                {
                                    client.SendTextMessageAsync(msg.Chat.Id, "Command successfully added");
                                }
                                else
                                {
                                    client.SendTextMessageAsync(msg.Chat.Id, "Failed to add command.\n"
                                        + error);
                                }
                            }
                            return;
                        #endregion
                        #region addusing
                        case "/addusing":
                            if (msg.From.Id == Flom)
                            {
                                string error = "";
                                if (!text.Contains(" ")) return;
                                string code = msg.ReplyToMessage == null
                                    ? text.Substring(text.IndexOf(' ') + 1)
                                    : msg.ReplyToMessage.Text;
                                if (addUsing(code, out error))
                                {
                                    client.SendTextMessageAsync(msg.Chat.Id,
                                        "Using-directive successfully added");
                                }
                                else
                                {
                                    client.SendTextMessageAsync(msg.Chat.Id,
                                        "Failed to add using-directive.\n"
                                        + error);
                                }
                            }
                            return;
                        #endregion
                        #region adddefinition
                        case "/adddefinition":
                            if (msg.From.Id == Flom)
                            {
                                string error = "";
                                if (!text.Contains(" ")) return;
                                string code = msg.ReplyToMessage == null
                                    ? text.Substring(text.IndexOf(' ') + 1)
                                    : msg.ReplyToMessage.Text;
                                if (addDefinition(code, out error))
                                {
                                    client.SendTextMessageAsync(msg.Chat.Id,
                                        "Definition successfully added");
                                }
                                else
                                {
                                    client.SendTextMessageAsync(msg.Chat.Id,
                                        "Failed to add definition.\n"
                                        + error);
                                }
                            }
                            return;
                        #endregion
                        #region getscript
                        case "/getscript":
                            client.SendTextMessageAsync(msg.Chat.Id, "`" + getScript() + "`",
                                parseMode: ParseMode.Markdown);
                            return;
                        #endregion
                        #region restart
                        case "/restart":
                            if (msg.From.Id == Flom)
                            {
                                Process[] pname = Process.GetProcessesByName("test.exe");
                                foreach (Process p in pname)
                                {
                                    p.CloseMainWindow();
                                    p.Close();
                                }
                                try
                                {
                                    Process.Start(execPath, scriptedBotToken);
                                    client.SendTextMessageAsync(msg.Chat.Id, "Bot (re)started.");
                                }
                                catch
                                {
                                    client.SendTextMessageAsync(msg.Chat.Id,
                                        "Could not start bot. Maybe the token is wrong.");
                                }
                            }
                            return;
                        #endregion
                        #region settoken
                        case "/settoken":
                            if (!text.Contains(" ")) return;
                            if (msg.From.Id == Flom)
                            {
                                setScriptedBotToken(text.Substring(text.IndexOf(' ') + 1));
                                client.SendTextMessageAsync(msg.Chat.Id, "Token set.");
                            }
                            return;
                        #endregion
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                client.SendTextMessageAsync(Flom, "An error has ocurred in ScriptingBot: " + ex.InnerException
                    + "\n" + ex.Message + "\n" + ex.StackTrace);
            }
        }
        #endregion

        #region Scripting stuff
        #region New Command
        private bool newCommand(string args, out string error)
        {
            if (!Directory.Exists(basePath)) Directory.CreateDirectory(basePath);
            if (args.Trim().StartsWith("case"))
            {
                string oldFile = System.IO.File.ReadAllText(scriptPath);
                string newFile = oldFile.Replace("//newcommand", args + "\n//newcommand");
                System.IO.File.WriteAllText(scriptPath, newFile);
                Process proc = new Process();
                ProcessStartInfo startinfo = new ProcessStartInfo("cmd.exe", "/C " + compilePath);
                startinfo.RedirectStandardOutput = true;
                startinfo.UseShellExecute = false;
                proc.StartInfo = startinfo;
                string output = "";
                proc.OutputDataReceived += (sender, text) => output += text.Data;
                proc.Start();
                proc.BeginOutputReadLine();
                proc.WaitForExit();
                if (output.ToLower().Contains("error"))
                {
                    System.IO.File.WriteAllText(scriptPath, oldFile);
                    error = output;
                    return false;
                }
                System.IO.File.WriteAllText(scriptSecondPath, newFile);
                error = "Success";
                return true;
            }
            else
            {
                error = "String needs to start with case";
                return false;
            }
        }
        #endregion
        #region Add Using
        private bool addUsing(string code, out string error)
        {
            if (!Directory.Exists(basePath)) Directory.CreateDirectory(basePath);
            string oldFile = System.IO.File.ReadAllText(scriptPath);
            string newFile = oldFile.Replace("//addusing", code + "\n//addusing");
            System.IO.File.WriteAllText(scriptPath, newFile);
            Process proc = new Process();
            ProcessStartInfo startinfo = new ProcessStartInfo("cmd.exe", "/C " + compilePath);
            startinfo.RedirectStandardOutput = true;
            startinfo.UseShellExecute = false;
            proc.StartInfo = startinfo;
            string output = "";
            proc.OutputDataReceived += (sender, text) => output += text.Data;
            proc.Start();
            proc.BeginOutputReadLine();
            proc.WaitForExit();
            if (output.ToLower().Contains("error"))
            {
                System.IO.File.WriteAllText(scriptPath, oldFile);
                error = output;
                return false;
            }
            System.IO.File.WriteAllText(scriptSecondPath, newFile);
            error = "Success";
            return true;
        }
        #endregion
        #region Add Definition
        private bool addDefinition(string code, out string error)
        {
            if (!Directory.Exists(basePath)) Directory.CreateDirectory(basePath);
            string oldFile = System.IO.File.ReadAllText(scriptPath);
            string newFile = oldFile.Replace("//adddefinition", code + "\n//adddefinition");
            System.IO.File.WriteAllText(scriptPath, newFile);
            Process proc = new Process();
            ProcessStartInfo startinfo = new ProcessStartInfo("cmd.exe", "/C " + compilePath);
            startinfo.RedirectStandardOutput = true;
            startinfo.UseShellExecute = false;
            proc.StartInfo = startinfo;
            string output = "";
            proc.OutputDataReceived += (sender, text) => output += text.Data;
            proc.Start();
            proc.BeginOutputReadLine();
            proc.WaitForExit();
            if (output.ToLower().Contains("error"))
            {
                System.IO.File.WriteAllText(scriptPath, oldFile);
                error = output;
                return false;
            }
            System.IO.File.WriteAllText(scriptSecondPath, newFile);
            error = "Success";
            return true;
        }
        #endregion
        #region Get Script
        private string getScript()
        {
            return System.IO.File.ReadAllText(scriptPath);
        }
        #endregion
        #region Get scripted bot token
        private string getScriptedBotToken()
        {
            if (System.IO.File.Exists(tokenPath))
            {
                return System.IO.File.ReadAllText(tokenPath);
            }
            return "none";
        }
        #endregion
        #region Set scripted Bot Token
        private void setScriptedBotToken(string token)
        {
            scriptedBotToken = token;
            if (!Directory.Exists(tokenBasePath)) Directory.CreateDirectory(tokenBasePath);
            System.IO.File.WriteAllText(tokenPath, token);
        }
        #endregion
        #endregion
    }
}
