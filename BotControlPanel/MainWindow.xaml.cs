using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BotControlPanel.Bots;
using System.IO;
using BotControlPanel.AlertWindows;
using FlomBotFactory.Panel;
using FlomBotFactory;
using System.Security.Permissions;
using System;
using BotControlPanel.Helpers;
using System.ComponentModel;
using Telegram.Bot;

namespace BotControlPanel
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Constants
        private static readonly Brush activeBackground = (Brush)Application.Current.Resources["activeBackground"];
        private static readonly Brush inactiveBackground = (Brush)Application.Current.Resources["inactiveBackground"];
        private static readonly Brush erroredBackground = (Brush)Application.Current.Resources["erroredBackground"];
        private const string basePath = "C:\\Olfi01\\BotControlPanel\\";
        private const string wwtbTokenPath = basePath + ".Tokens\\wwtb.token";
        //private const string achvTokenPath = "C:\\Olfi01\\BotControlPanel\\.Tokens\\achvbot.token";
        private const string tokenBasePath = basePath + ".Tokens\\";
        private const string myTokenPath = basePath + ".Tokens\\myToken.token";
        private static readonly string logFilePath = basePath + "Logs\\" + $"log{DateTime.Now.ToFileNameString()}.txt";
        private const string erroredMessage = "Lege zuerst ein korrektes Token unter Einstellungen fest!\n" +
                    "Falls du das bereits getan hast, ist ein anderer Fehler aufgetreten.";
        private const string tokenSuffix = ".token";
        private const long TestGroup = -1001070844778;
        #endregion
        #region Variables
        private Wwtb wwtb;
        //private WerewolfAchievementsBotPlus achBot;
        private List<FlomBot> bots = new List<FlomBot>() { new ScriptingBot() };
        private string wwtbToken = "";
        private string myToken = "";
        //private string achToken = "";
        private bool terminate = false;
        #endregion
        #region Constructor
        public MainWindow()
        {
            Construct();
        }

        private void Construct()
        {
            SetLogger();
            InitializeComponent();
            GetTokens();
            /*try {*/
            wwtb = new Wwtb(wwtbToken); /*}
            catch { textBlockWWTB.Background = erroredBackground; }*/
            /*try { achBot = new WerewolfAchievementsBotPlus(achToken); }
            catch { textBlockAchv.Background = erroredBackground; }*/
            foreach (FlomBot b in bots)
            {
                InitializeFlomBot(b);
            }
            Log("BCP started");
        }
        #endregion

        #region Logging
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        private void SetLogger()
        {
            AppDomain.CurrentDomain.UnhandledException += ExceptionLogger;
        }

        private void ExceptionLogger(object sender, UnhandledExceptionEventArgs e)
        {
            string d = Path.GetDirectoryName(logFilePath);
            if (!Directory.Exists(d))
            {
                Directory.CreateDirectory(d);
            }
            if (!File.Exists(logFilePath))
            {
                File.Create(logFilePath);
            }
            Exception ex = (Exception)e.ExceptionObject;
            string msg = "";
            msg += $"{ex.Message} - {ex.StackTrace}";
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
                msg += $"{ex.Message} - {ex.StackTrace}";
            }
            Log(msg);
            if (e.IsTerminating)
            {
                Log("Terminating");
                Log("Sending restart message");
                new TelegramBotClient(myToken).SendTextMessageAsync(TestGroup, "BCP crashed. Please restart ASAP.");
            }
        }

        private static void Log(string msg)
        {
            msg = DateTime.Now.ToString() + ": " + msg;
            Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));
            File.AppendAllText(logFilePath, msg + Environment.NewLine);
        }
        #endregion

        #region Random Methods
        #region Get Tokens
        private void GetTokens()
        {
            #region WWTB Token
            if (File.Exists(wwtbTokenPath))
            {
                wwtbToken = File.ReadAllText(wwtbTokenPath);
            }
            if (File.Exists(myTokenPath)) myToken = File.ReadAllText(myTokenPath);
            #endregion
            #region AchBot Token
            /*if (File.Exists(achvTokenPath))
            {
                achToken = File.ReadAllText(achvTokenPath);
            }*/
            #endregion
        }
        #endregion
        #region Set Tokens
        #region WWTB Token
        private void SetWWTBToken(string token)
        {
            if (!Directory.Exists(tokenBasePath))
            {
                DirectoryInfo di = Directory.CreateDirectory(tokenBasePath);
                di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
            File.WriteAllText(wwtbTokenPath, token);
            wwtbToken = token;
            textBlockWWTB.Background = inactiveBackground;
            try { wwtb = new Wwtb(wwtbToken); }
            catch { textBlockWWTB.Background = erroredBackground; }
        }
        #endregion
        #region AchBot Token
        /*private void SetAchvToken(string token)
        {
            if (!Directory.Exists(tokenBasePath))
            {
                DirectoryInfo di = Directory.CreateDirectory(tokenBasePath);
                di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
            File.WriteAllText(achvTokenPath, token);
            achToken = token;
            textBlockAchv.Background = inactiveBackground;
            try { achBot = new WerewolfAchievementsBotPlus(achToken); }
            catch { textBlockAchv.Background = erroredBackground; }
        }*/
        #endregion
        #endregion
        #region Get Token By Name
        private string GetTokenByName(string name)
        {
            string path = tokenBasePath + name + tokenSuffix;
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }
            else
            {
                return "Failed";
            }
        }
        #endregion
        #region Initialize Flom Bot
        private void InitializeFlomBot(FlomBot b)
        {
            b.Token = GetTokenByName(b.Name);
            RowDefinition rd = new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) };
            grid.RowDefinitions.Add(rd);
            TextBlock tb = new TextBlock()
            {
                Text = b.Name,
                Background = inactiveBackground,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(10, 10, 10, 10)
            };
            Grid.SetRow(tb, grid.RowDefinitions.Count - 1);
            Button b1 = new Button()
            {
                Content = "Starten",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(10, 10, 10, 10),
                Width = 75
            };
            Grid.SetRow(b1, grid.RowDefinitions.Count - 1);
            Button b2 = new Button()
            {
                Content = "Einstellungen",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(10, 10, 10, 10),
                Width = 75
            };
            Grid.SetRow(b2, grid.RowDefinitions.Count - 1);
            b.Panel = new BotPanelPart(tb, b1, b2);
            b.Panel.StartButton.Click += (object sender, RoutedEventArgs e) => Start(b);
            #region FlomBot Settings Button
            b.Panel.SettingsButton.Click += delegate (object sender, RoutedEventArgs e)
            {
                if (!b.IsRunning)
                {
                    TokenDialog td = new TokenDialog(b.Token);
                    td.ShowDialog();
                    SetFlomBotToken(b, td.result);
                    Log($"{b.Name} token set");
                }
                else
                {
                    MessageBox.Show("Don't edit the token while the bot is running!");
                }
            };
            #endregion
            grid.Children.Add(tb);
            grid.Children.Add(b1);
            grid.Children.Add(b2);
            if (b.BotState == FlomBot.State.Errored)
            {
                b.Panel.TextBlock.Background = erroredBackground;
            }
            b.BotStarted += (sender, e) => Started(e.Bot);
            b.BotStopped += (sender, e) => Stopped(e.Bot);
        }
        #endregion
        #region Start Bots
        public void StartBots(params string[] names)
        {
            foreach (var n in names)
            {
                if (wwtb != null && n == wwtb.Name)
                {
                    StartButtonWWTB_Click(this, new RoutedEventArgs());
                }
                if (bots.Exists(x => x.Name == n)) Start(bots.Find(x => x.Name == n));
            }
        }
        #endregion
        #region Started and stopped
        private void Started(FlomBot bot)
        {
            bot.Panel.TextBlock.Background = activeBackground;
            bot.Panel.StartButton.Content = "Stoppen";
            Log($"{bot.Name} started");
        }
        private void Stopped(FlomBot bot)
        {
            bot.Panel.TextBlock.Background = inactiveBackground;
            bot.Panel.StartButton.Content = "Starten";
            Log($"{bot.Name} stopped");
        }
        #endregion
        #region Start Flom Bot
        private void Start(FlomBot b)
        {
            if (b.Panel.TextBlock.Background == erroredBackground)
            {
                MessageBox.Show(erroredMessage);
                return;
            }
            else if (b.Panel.TextBlock.Background == inactiveBackground)
            {
                bool started = b.IsRunning;
                if (!started)
                {
                    started = b.StartBot();
                }
                if (started)
                {
                    b.Panel.TextBlock.Background = activeBackground;
                    b.Panel.StartButton.Content = "Stoppen";
                }
                else if (b.BotState == FlomBot.State.Errored)
                {
                    b.Panel.TextBlock.Background = erroredBackground;
                }
            }
            else if (b.Panel.TextBlock.Background == activeBackground)
            {
                if (b.IsRunning)
                {
                    b.StopBot();
                }
                b.Panel.TextBlock.Background = inactiveBackground;
                b.Panel.StartButton.Content = "Starten";
            }
        }
        #endregion
        #endregion

        #region Button
        #region WWTB
        #region Start WWTB
        private void StartButtonWWTB_Click(object sender, RoutedEventArgs e)
        {
            if (textBlockWWTB.Background == erroredBackground)
            {
                MessageBox.Show(erroredMessage);
                return;
            }
            bool started = wwtb.IsRunning;
            if (!wwtb.IsRunning)
            {
                started = wwtb.StartBot();
            }
            if (started)
            {
                textBlockWWTB.Background = activeBackground;
                startButtonWWTB.Content = "Stoppen";
                startButtonWWTB.Click -= StartButtonWWTB_Click;
                startButtonWWTB.Click += StopButtonWWTB_Click;
                Log("WWTB started");
            }
        }
        #endregion
        #region Stop WWTB
        private void StopButtonWWTB_Click(object sender, RoutedEventArgs e)
        {
            if (wwtb.IsRunning)
            {
                wwtb.StopBot();
            }
            textBlockWWTB.Background = inactiveBackground;
            startButtonWWTB.Content = "Starten";
            startButtonWWTB.Click -= StopButtonWWTB_Click;
            startButtonWWTB.Click += StartButtonWWTB_Click;
            Log("WWTB stopped");
        }
        #endregion
        #region Settings WWTB
        private void SettingsButtonWWTB_Click(object sender, RoutedEventArgs e)
        {
            if (wwtb.IsRunning)
            {
                TokenDialog td = new TokenDialog(wwtbToken);
                td.ShowDialog();
                SetWWTBToken(td.result);
                Log("WWTB token set");
            }
            else
            {
                MessageBox.Show("Don't edit the token while the bot is running!");
            }
        }
        #endregion
        #endregion
        #region AchBot
        #region Start AchBot
        /*private void ButtonStartAchievements_Click(object sender, RoutedEventArgs e)
        {
            if (textBlockAchv.Background == erroredBackground)
            {
                MessageBox.Show(erroredMessage);
                return;
            }
            if (!achBot.IsRunning)
            {
                achBot.StartBot();
            }
            textBlockAchv.Background = activeBackground;
            buttonStartAchievements.Content = "Stoppen";
            buttonStartAchievements.Click -= ButtonStartAchievements_Click;
            buttonStartAchievements.Click += ButtonStopAchievements_Click;
        }*/
        #endregion
        #region Stop AchBot
        /*private void ButtonStopAchievements_Click(object sender, RoutedEventArgs e)
        {
            if (achBot.IsRunning)
            {
                achBot.StopBot();
            }
            textBlockAchv.Background = inactiveBackground;
            buttonStartAchievements.Content = "Starten";
            buttonStartAchievements.Click -= ButtonStopAchievements_Click;
            buttonStartAchievements.Click += ButtonStartAchievements_Click;
        }*/
        #endregion
        #region Achievement Settings
        /*private void ButtonSettingsAchievements_Click(object sender, RoutedEventArgs e)
        {
            if (!achBot.IsRunning)
            {
                TokenDialog td = new TokenDialog(achToken);
                td.ShowDialog();
                SetAchvToken(td.result);
            }
            else
            {
                MessageBox.Show("Don't edit the token while the bot is running!");
            }
        }*/
        #endregion
        #endregion
        #region FlomBot
        #region FlomBot Settings
        private void SetFlomBotToken(FlomBot b, string token)
        {
            b.Token = token;
            if (!Directory.Exists(tokenBasePath))
            {
                DirectoryInfo di = Directory.CreateDirectory(tokenBasePath);
                di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
            string path = tokenBasePath + b.Name + tokenSuffix;
            File.WriteAllText(path, token);
            if (b.BotState == FlomBot.State.Functionable)
            {
                b.Panel.TextBlock.Background = inactiveBackground;
            }
            else
            {
                b.Panel.TextBlock.Background = erroredBackground;
            }
        }
        #endregion
        #endregion
        #endregion

        #region Close Window
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            bool botRunning = false;
            try
            {
                if (wwtb.IsRunning /*|| achBot.IsRunning*/) botRunning = true;
            }
            catch { }
            foreach (FlomBot b in bots)
            {
                if (b.IsRunning) botRunning = true;
            }
            if (botRunning)
            {
                if (!terminate)
                {
                    MessageBoxResult res = MessageBox.Show(
                        "Bot(s) are still running. They will be stopped upon closing.",
                        "Close?", MessageBoxButton.OKCancel);
                    if (res == MessageBoxResult.Cancel)
                    {
                        e.Cancel = true;
                    }
                    else
                    {
                        wwtb.StopBot();
                        //achBot.StopBot();
                        foreach (FlomBot b in bots)
                        {
                            b.StopBot();
                        }
                        Log("Stopping");
                    }
                }
                else
                {
                    wwtb.StopBot();
                    //achBot.StopBot();
                    foreach (FlomBot b in bots)
                    {
                        b.StopBot();
                    }
                }
            }
        }
        #endregion

        #region Window loaded
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            StartButtonWWTB_Click(this, new RoutedEventArgs());
            bots.ForEach(x => Start(x));
        }
        #endregion
    }
}
