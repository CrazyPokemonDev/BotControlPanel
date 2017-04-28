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
        private static readonly string dllPath1 = Path.Combine(startPath, "Telegram.Bot.dll");
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
                                if (NewCommand(code, out string error))
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
                                if (!text.Contains(" ")) return;
                                string code = msg.ReplyToMessage == null
                                    ? text.Substring(text.IndexOf(' ') + 1)
                                    : msg.ReplyToMessage.Text;
                                if (AddUsing(code, out string error))
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
                                if (!text.Contains(" ")) return;
                                string code = msg.ReplyToMessage == null
                                    ? text.Substring(text.IndexOf(' ') + 1)
                                    : msg.ReplyToMessage.Text;
                                if (AddDefinition(code, out string error))
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
                            client.SendTextMessageAsync(msg.Chat.Id, "`" + GetScript() + "`",
                                parseMode: ParseMode.Markdown);
                            return;
                        #endregion
                        #region restart
                        case "/restart":
                            if (botStopMethod != null) botStopMethod.Invoke(null, null);
                            bool errored = false;
                            CompileBot();
                            try
                            {
                                botMainMethod.Invoke(null, new object[] { new String[] { scriptedBotToken } });
                            }
                            catch
                            {
                                client.SendTextMessageAsync(msg.Chat.Id, "Failed to start. Maybe the token is missing.");
                                errored = true;
                            }
                            if (!errored) client.SendTextMessageAsync(msg.Chat.Id, "Bot (re)started.");
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

        #region Scripting stuff
        #region Compile bot
        private void CompileBot()
        {
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters();
            parameters.ReferencedAssemblies.Add(dllPath1);
            parameters.ReferencedAssemblies.Add(dllPath2);
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
            if (botStopMethod != null) botStopMethod.Invoke(null, null);
            if (!Directory.Exists(basePath)) Directory.CreateDirectory(basePath);
            if (args.Trim().StartsWith("case"))
            {
                string oldFile = System.IO.File.ReadAllText(scriptPath);
                string newFile = oldFile.Replace("//newcommand", args + "\n//newcommand");
                CSharpCodeProvider provider = new CSharpCodeProvider();
                CompilerParameters parameters = new CompilerParameters();
                parameters.ReferencedAssemblies.Add(dllPath1);
                parameters.ReferencedAssemblies.Add(dllPath2);
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
        #region Add Using
        private bool AddUsing(string code, out string error)
        {
            if (botStopMethod != null) botStopMethod.Invoke(null, null);
            if (!Directory.Exists(basePath)) Directory.CreateDirectory(basePath);
            string oldFile = System.IO.File.ReadAllText(scriptPath);
            string newFile = oldFile.Replace("//addusing", code + "\n//addusing");
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters();
            parameters.ReferencedAssemblies.Add(dllPath1);
            parameters.ReferencedAssemblies.Add(dllPath2);
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
            if (botStopMethod != null) botStopMethod.Invoke(null, null);
            if (!Directory.Exists(basePath)) Directory.CreateDirectory(basePath);
            string oldFile = System.IO.File.ReadAllText(scriptPath);
            string newFile = oldFile.Replace("//adddefinition", code + "\n//adddefinition");
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters();
            parameters.ReferencedAssemblies.Add(dllPath1);
            parameters.ReferencedAssemblies.Add(dllPath2);
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
        #endregion
    }
}
