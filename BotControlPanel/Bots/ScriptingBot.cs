using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using FlomBotFactory;

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
        private static readonly string execPath = Path.Combine(basePath, "Start.exe");
        private static readonly string dllPath3 = Path.Combine(startPath, "Telegram.Bot.dll");
        private static readonly string dllPath2 = Path.Combine(startPath, "Newtonsoft.Json.dll");
        private const string tokenBasePath = "C:\\Olfi01\\BotControlPanel\\ScriptingBot\\";
        private const string tokenPath = tokenBasePath + "scriptedToken.token";
        #endregion
        #region Variables
        private string scriptedBotToken;
        private MethodInfo botMainMethod;
        private MethodInfo botStopMethod;
        #endregion

        #region Constructor
        public ScriptingBot() : base()
        {
            scriptedBotToken = GetScriptedBotToken();
            if (!Directory.Exists(basePath)) Directory.CreateDirectory(basePath);
            if (!System.IO.File.Exists(scriptPath))
            {
                System.IO.File.Copy(startPath + "Start.cs", scriptPath);
            }
        }
        #endregion

        #region Start Bot
        public override bool StartBot()
        {
            bool b = base.StartBot();
            try
            {
                RestartBot(Flom);
            }
            catch
            {
                client.SendTextMessageAsync(Flom, "Failed to start bot");
            }
            return b;
        }
        #endregion

        #region Stop Bot
        public override bool StopBot()
        {
            bool b = base.StopBot();
            if (botStopMethod != null)
            {
                botStopMethod.Invoke(null, null);
                client.SendTextMessageAsync(Flom, "Scripted bot was stopped.");
            }
            return b;
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
                                if (!text.Contains(" ") && msg.ReplyToMessage == null) return;
                                string code = msg.ReplyToMessage == null
                                    ? text.Substring(text.IndexOf(' ') + 1)
                                    : msg.ReplyToMessage.Text;
                                string error;
                                if (NewCommand(code, out error))
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
                        #region delcommand
                        case "/delcommand":
                            if (!text.Contains(" ")) return;
                            DeleteCommand(text.Substring(text.IndexOf(' ') + 1));
                            client.SendTextMessageAsync(msg.Chat.Id, "Command deleted.");
                            return;
                        #endregion
                        #region addusing
                        case "/addusing":
                            if (msg.From.Id == Flom)
                            {
                                if (!text.Contains(" ") && msg.ReplyToMessage == null) return;
                                string code = msg.ReplyToMessage == null
                                    ? text.Substring(text.IndexOf(' ') + 1)
                                    : msg.ReplyToMessage.Text;
                                string error;
                                if (AddUsing(code, out error))
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
                                if (!text.Contains(" ") && msg.ReplyToMessage == null) return;
                                string code = msg.ReplyToMessage == null
                                    ? text.Substring(text.IndexOf(' ') + 1)
                                    : msg.ReplyToMessage.Text;
                                string error;
                                if (AddDefinition(code, out error))
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
                        #region addmethod
                        case "/addmethod":
                            if (msg.From.Id == Flom)
                            {
                                if (!text.Contains(" ") && msg.ReplyToMessage == null) return;
                                string code = msg.ReplyToMessage == null
                                    ? text.Substring(text.IndexOf(' ') + 1)
                                    : msg.ReplyToMessage.Text;
                                string error;
                                if (AddMethod(code, out error))
                                {
                                    client.SendTextMessageAsync(msg.Chat.Id, "Method successfully added");
                                }
                                else
                                {
                                    client.SendTextMessageAsync(msg.Chat.Id, "Failed to add method.\n"
                                        + error);
                                }
                            }
                            return;
                        #endregion
                        #region getscript
                        case "/getscript":
                            string script = GetScript();
                            List<string> list = new List<string>();
                            while (script.Length > 2000)
                            {
                                list.Add(script.Remove(2000));
                                script = script.Substring(2000);
                            }
                            list.Add(script);
                            foreach (string s in list)
                                client.SendTextMessageAsync(msg.Chat.Id, "`" + s + "`",
                                    parseMode: ParseMode.Markdown).Wait();
                            return;
                        #endregion
                        #region restart
                        case "/restart":
                            RestartBot(msg.Chat.Id);
                            return;
                        #endregion
                        #region settoken
                        case "/settoken":
                            if (!text.Contains(" ")) return;
                            if (msg.From.Id == Flom)
                            {
                                SetScriptedBotToken(text.Substring(text.IndexOf(' ') + 1));
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

        #region Control Methods
        #region Restart
        private void RestartBot(long idToSendAnswer)
        {
            botStopMethod?.Invoke(null, null);
            bool errored = false;
            CompileBot();
            try
            {
                botMainMethod.Invoke(null, new object[] { new String[] { scriptedBotToken } });
            }
            catch
            {
                client.SendTextMessageAsync(idToSendAnswer, "Failed to start. Maybe the token is missing.");
                errored = true;
            }
            if (!errored) client.SendTextMessageAsync(idToSendAnswer, "Bot (re)started.");
        }
        #endregion
        #endregion

        #region Scripting stuff
        #region Compile bot
        private void CompileBot()
        {
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters();
            AddReferencedAssemblys(parameters);
            parameters.GenerateInMemory = true;
            parameters.GenerateExecutable = true;
            CompilerResults results = provider.CompileAssemblyFromSource(parameters, GetScript());
            string error = "";
            if (results.Errors.HasErrors)
            {
                foreach (CompilerError e in results.Errors)
                {
                    error += e.ErrorNumber + " " + e.ErrorText + "\n";
                }
                throw new Exception(error);
            }
            Assembly assembly = results.CompiledAssembly;
            Type program = assembly.GetType("ScriptedBot.Program");
            string[] dummy = new string[1];
            botMainMethod = program.GetMethod("Main");
            botStopMethod = program.GetMethod("Stop");
        }
        #endregion
        #region New Command
        private bool NewCommand(string args, out string error)
        {
            args = AutoCodeCompletion(args);
            if (botStopMethod != null) botStopMethod.Invoke(null, null);
            if (!Directory.Exists(basePath)) Directory.CreateDirectory(basePath);
            if (args.Trim().StartsWith("case"))
            {
                string oldFile = System.IO.File.ReadAllText(scriptPath);
                string newFile = oldFile.Replace("//newcommand", args + "\n//end\n//newcommand");
                CSharpCodeProvider provider = new CSharpCodeProvider();
                CompilerParameters parameters = new CompilerParameters();
                AddReferencedAssemblys(parameters);
                parameters.GenerateInMemory = true;
                parameters.GenerateExecutable = true;
                CompilerResults results = provider.CompileAssemblyFromSource(parameters, newFile);
                error = "";
                if (results.Errors.HasErrors)
                {
                    foreach (CompilerError e in results.Errors)
                    {
                        error += e.ErrorNumber + " " + e.ErrorText + "\n";
                    }
                    CompileBot();
                    botMainMethod.Invoke(null, new object[] { new String[] { scriptedBotToken } });
                    return false;
                }
                Assembly assembly = results.CompiledAssembly;
                Type program = assembly.GetType("ScriptedBot.Program");
                botMainMethod = program.GetMethod("Main");
                botStopMethod = program.GetMethod("Stop");
                System.IO.File.WriteAllText(scriptPath, newFile);
                System.IO.File.WriteAllText(scriptSecondPath, newFile);
                error = "Success";
                botMainMethod.Invoke(null, new object[] { new String[] { scriptedBotToken } });
                return true;
            }
            else
            {
                error = "String needs to start with case";
                return false;
            }
        }
        #endregion
        #region Delete Command
        private void DeleteCommand(string name)
        {
            string script = GetScript();
            string toRemove = script.Substring(script.IndexOf("private static void Client_OnUpdate(object sender, UpdateEventArgs e)"));
            toRemove = toRemove.Substring(toRemove.IndexOf($"case \"" + name + "\""));
            toRemove = toRemove.Remove(toRemove.IndexOf("//end") + 5);
            script = script.Remove(script.IndexOf(toRemove), toRemove.Length);
            System.IO.File.WriteAllText(scriptPath, script);
            if (botStopMethod != null) botStopMethod.Invoke(null, null);
            CompileBot();
            botMainMethod.Invoke(null, new object[] { new String[] { scriptedBotToken } });
        }
        #endregion
        #region Add Using
        private bool AddUsing(string code, out string error)
        {
            if (botStopMethod != null) botStopMethod.Invoke(null, null);
            if (!Directory.Exists(basePath)) Directory.CreateDirectory(basePath);
            string oldFile = System.IO.File.ReadAllText(scriptPath);
            string newFile = oldFile.Replace("//addusing", code + "\n//addusing");
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters();
            AddReferencedAssemblys(parameters);
            parameters.GenerateInMemory = true;
            parameters.GenerateExecutable = true;
            CompilerResults results = provider.CompileAssemblyFromSource(parameters, newFile);
            error = "";
            if (results.Errors.HasErrors)
            {
                foreach (CompilerError e in results.Errors)
                {
                    error += e.ErrorNumber + " " + e.ErrorText + "\n";
                }
                CompileBot();
                botMainMethod.Invoke(null, new object[] { new String[] { scriptedBotToken } });
                return false;
            }
            Assembly assembly = results.CompiledAssembly;
            Type program = assembly.GetType("ScriptedBot.Program");
            botMainMethod = program.GetMethod("Main");
            botStopMethod = program.GetMethod("Stop");
            System.IO.File.WriteAllText(scriptPath, newFile);
            System.IO.File.WriteAllText(scriptSecondPath, newFile);
            error = "Success";
            botMainMethod.Invoke(null, new object[] { new String[] { scriptedBotToken } });
            return true;
        }
        #endregion
        #region Add Definition
        private bool AddDefinition(string code, out string error)
        {
            code = AutoCodeCompletion(code);
            if (botStopMethod != null) botStopMethod.Invoke(null, null);
            if (!Directory.Exists(basePath)) Directory.CreateDirectory(basePath);
            string oldFile = System.IO.File.ReadAllText(scriptPath);
            string newFile = oldFile.Replace("//adddefinition", code + "\n//adddefinition");
            newFile = AutoCodeCompletion(newFile);
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters();
            AddReferencedAssemblys(parameters);
            parameters.GenerateInMemory = true;
            parameters.GenerateExecutable = true;
            CompilerResults results = provider.CompileAssemblyFromSource(parameters, newFile);
            error = "";
            if (results.Errors.HasErrors)
            {
                foreach (CompilerError e in results.Errors)
                {
                    error += e.ErrorNumber + " " + e.ErrorText + "\n";
                }
                CompileBot();
                botMainMethod.Invoke(null, new object[] { new String[] { scriptedBotToken } });
                return false;
            }
            Assembly assembly = results.CompiledAssembly;
            Type program = assembly.GetType("ScriptedBot.Program");
            botMainMethod = program.GetMethod("Main");
            botStopMethod = program.GetMethod("Stop");
            System.IO.File.WriteAllText(scriptPath, newFile);
            System.IO.File.WriteAllText(scriptSecondPath, newFile);
            error = "Success";
            botMainMethod.Invoke(null, new object[] { new String[] { scriptedBotToken } });
            return true;
        }
        #endregion
        #region Add method
        private bool AddMethod(string code, out string error)
        {
            code = AutoCodeCompletion(code);
            if (botStopMethod != null) botStopMethod.Invoke(null, null);
            if (!Directory.Exists(basePath)) Directory.CreateDirectory(basePath);
            string oldFile = System.IO.File.ReadAllText(scriptPath);
            string newFile = oldFile.Replace("//addmethod", code + "\n//addmethod");
            newFile = AutoCodeCompletion(newFile);
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters();
            AddReferencedAssemblys(parameters);
            parameters.GenerateInMemory = true;
            parameters.GenerateExecutable = true;
            CompilerResults results = provider.CompileAssemblyFromSource(parameters, newFile);
            error = "";
            if (results.Errors.HasErrors)
            {
                foreach (CompilerError e in results.Errors)
                {
                    error += e.ErrorNumber + " " + e.ErrorText + "\n";
                }
                CompileBot();
                botMainMethod.Invoke(null, new object[] { new String[] { scriptedBotToken } });
                return false;
            }
            Assembly assembly = results.CompiledAssembly;
            Type program = assembly.GetType("ScriptedBot.Program");
            botMainMethod = program.GetMethod("Main");
            botStopMethod = program.GetMethod("Stop");
            System.IO.File.WriteAllText(scriptPath, newFile);
            System.IO.File.WriteAllText(scriptSecondPath, newFile);
            error = "Success";
            botMainMethod.Invoke(null, new object[] { new String[] { scriptedBotToken } });
            return true;
        }
        #endregion
        #region Get Script
        private string GetScript()
        {
            return System.IO.File.ReadAllText(scriptPath);
        }
        #endregion
        #region Get scripted bot token
        private string GetScriptedBotToken()
        {
            if (System.IO.File.Exists(tokenPath))
            {
                return System.IO.File.ReadAllText(tokenPath);
            }
            return "none";
        }
        #endregion
        #region Set scripted Bot Token
        private void SetScriptedBotToken(string token)
        {
            scriptedBotToken = token;
            if (!Directory.Exists(tokenBasePath)) Directory.CreateDirectory(tokenBasePath);
            System.IO.File.WriteAllText(tokenPath, token);
        }
        #endregion
        #region Intuitive code sequences
        private string AutoCodeCompletion(string codeToComplete)
        {
            List<string> lines = codeToComplete.Split('\n').ToList();
            List<string> newLines = new List<string>();
            foreach (string l in lines)
            {
                string newLine = l;
                string last = "";
                if (l.StartsWith("\\"))
                {
                    bool objBool = false;
                    string obj = "";
                    string baseName = "";
                    string baseAction = "No action recognized";
                    Dictionary<string, string> args = new Dictionary<string, string>();
                    Dictionary<string, string> defaultArgs = new Dictionary<string, string>();
                    foreach (string word in (l.Trim() + " $top").Substring(1).Split(' '))
                    {
                        #region Keyword handling
                        if (!objBool)
                        {
                            string nowword = word;
                            if (word.EndsWith(":"))
                            {
                                objBool = true;
                                nowword = word.Remove(word.Length - 1);
                            }
                            switch (last.ToLower())
                            {
                                #region Keyword Send
                                case "send":
                                    if (baseName != "") break;
                                    switch (nowword.ToLower())
                                    {
                                        case "message":
                                            baseAction = "client.SendTextMessageAsync({0}, \"{obj}\", parseMode: {1});";
                                            baseName = "message";
                                            defaultArgs.Add("{0}", "msg.Chat.Id");
                                            defaultArgs.Add("{1}", "ParseMode.Default");
                                            break;
                                    }
                                    break;
                                #endregion
                                #region Keyword Reply
                                case "reply":
                                    baseAction = "client.SendTextMessageAsync({0}, \"{obj}\", parseMode: {1}, " 
                                        + "replyToMessageId: msg.MessageId);";
                                    baseName = "message";
                                    defaultArgs.Add("{0}", "msg.Chat.Id");
                                    defaultArgs.Add("{1}", "ParseMode.Default");
                                    break;
                                #endregion
                                #region Keyword To
                                case "to":
                                    if (baseName != "message") break;
                                    if (args.ContainsKey("{0}")) break;
                                    switch (nowword.ToLower())
                                    {
                                        case "sender":
                                            args.Add("{0}", "msg.From.Id");
                                            break;
                                        case "chat":
                                            args.Add("{0}", "msg.Chat.Id");
                                            break;
                                        case "flom":
                                            args.Add("{0}", "flomsId");
                                            break;
                                        default:
                                            args.Add("{0}", nowword);
                                            break;
                                    }
                                    break;
                                #endregion
                                #region Keyword Parsemode
                                case "parsemode":
                                    if (args.ContainsKey("{1}")) break;
                                    if (baseName != "message") break;
                                    switch (nowword.ToLower())
                                    {
                                        case "markdown":
                                            args.Add("{1}", "ParseMode.Markdown");
                                            break;
                                        case "html":
                                            args.Add("{1}", "ParseMode.Html");
                                            break;
                                        case "default":
                                            args.Add("{1}", "ParseMode.Default");
                                            break;
                                        default:
                                            args.Add("{0}", nowword);
                                            break;
                                    }
                                    break;
                                #endregion
                                #region Keyword Leave
                                case "leave":
                                    if (baseName != "") break;
                                    baseName = "leave";
                                    baseAction = "client.LeaveChatAsync({0});";
                                    defaultArgs.Add("{0}", "msg.Chat.Id");
                                    if (nowword != "$top") args.Add("{0}", nowword);
                                    break;
                                #endregion
                            }
                        }
                        else
                        {
                            if (word != "$top")
                                obj += word + " ";
                        }
                        last = word;
                        #endregion
                    }
                    newLine = baseAction;
                    if (newLine.Contains("{obj}"))
                    {
                        newLine = newLine.Replace("{obj}", obj.Trim());
                    }
                    foreach (var kvp in defaultArgs)
                    {
                        if (!args.ContainsKey(kvp.Key)) args.Add(kvp.Key, kvp.Value);
                    }
                    foreach (var kvp in args)
                    {
                        if (newLine.Contains(kvp.Key))
                        {
                            newLine = newLine.Replace(kvp.Key, kvp.Value);
                        }
                    }
                }
                newLines.Add(newLine);
            }
            string r = "";
            foreach(string line in newLines)
            {
                r += line + "\n";
            }
            return r;
        }
        #endregion
        #region Add Referenced Assemblys
        private static void AddReferencedAssemblys(CompilerParameters parameters)
        {
            parameters.ReferencedAssemblies.Add("Telegram.Bot.dll");
            parameters.ReferencedAssemblies.Add("Newtonsoft.Json.dll");
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Net.Http.dll");
            parameters.ReferencedAssemblies.Add("Microsoft.CSharp.dll");
            parameters.ReferencedAssemblies.Add("System.Core.dll");
            parameters.ReferencedAssemblies.Add("System.Runtime.dll");
            parameters.ReferencedAssemblies.Add("System.Threading.Tasks.dll");
            parameters.ReferencedAssemblies.Add("System.Collections.dll");
            parameters.ReferencedAssemblies.Add("System.IO.dll");
            parameters.ReferencedAssemblies.Add("System.Data.SQLite.dll");
            parameters.ReferencedAssemblies.Add("System.Data.dll");
        }
        #endregion
        #endregion
    }
}
