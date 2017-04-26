using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BotControlPanel.Bots;
using System.IO;
using BotControlPanel.AlertWindows;
using System.Security.AccessControl;
using BotControlPanel.Panel;

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
        private const string wwtbTokenPath = "C:\\Olfi01\\BotControlPanel\\.Tokens\\wwtb.token";
        private const string achvTokenPath = "C:\\Olfi01\\BotControlPanel\\.Tokens\\achvbot.token";
        private const string tokenBasePath = "C:\\Olfi01\\BotControlPanel\\.Tokens\\";
        private const string erroredMessage = "Lege zuerst ein korrektes Token unter Einstellungen fest!\n" +
                    "Falls du das bereits getan hast, ist ein anderer Fehler aufgetreten.";
        private const string tokenSuffix = ".token";
        #endregion
        #region Variables
        private WWTB wwtb;
        private WerewolfAchievementsBotPlus achBot;
        private List<FlomBot> bots = new List<FlomBot>();
        private string wwtbToken = "";
        private string achToken = "";
        #endregion
        #region Constructor
        public MainWindow()
        {
            InitializeComponent();
            getTokens();
            try { wwtb = new WWTB(wwtbToken); }
            catch { textBlockWWTB.Background = erroredBackground; }
            try { achBot = new WerewolfAchievementsBotPlus(achToken); }
            catch { textBlockAchv.Background = erroredBackground; }
            foreach (FlomBot b in bots)
            {
                initializeFlomBot(b);
            }
        }
        #endregion

        #region Random Methods
        #region Get Tokens
        private void getTokens()
        {
            #region WWTB Token
            if (File.Exists(wwtbTokenPath))
            {
                wwtbToken = File.ReadAllText(wwtbTokenPath);
            }
            #endregion
            #region AchBot Token
            if (File.Exists(achvTokenPath))
            {
                achToken = File.ReadAllText(achvTokenPath);
            }
            #endregion
        }
        #endregion
        #region Set Tokens
        #region WWTB Token
        private void setWWTBToken(string token)
        {
            if (!Directory.Exists(tokenBasePath))
            {
                DirectoryInfo di = Directory.CreateDirectory(tokenBasePath);
                di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
            File.WriteAllText(wwtbTokenPath, token);
            wwtbToken = token;
            textBlockWWTB.Background = inactiveBackground;
            try { wwtb = new WWTB(wwtbToken); }
            catch { textBlockWWTB.Background = erroredBackground; }
        }
        #endregion
        #region AchBot Token
        private void setAchvToken(string token)
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
        }
        #endregion
        #endregion
        #region Get Token By Name
        private string getTokenByName(string name)
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
        private void initializeFlomBot(FlomBot b)
        {
            b.Token = getTokenByName(b.Name);
            RowDefinition rd = new RowDefinition();
            rd.Height = new GridLength(1, GridUnitType.Star);
            grid.RowDefinitions.Add(rd);
            TextBlock tb = new TextBlock();
            tb.Text = b.Name;
            tb.Background = inactiveBackground;
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Left;
            tb.Margin = new Thickness(10, 10, 10, 10);
            Grid.SetRow(tb, grid.RowDefinitions.Count - 1);
            Button b1 = new Button();
            b1.Content = "Starten";
            b1.VerticalAlignment = VerticalAlignment.Center;
            b1.HorizontalAlignment = HorizontalAlignment.Center;
            b1.Margin = new Thickness(10, 10, 10, 10);
            b1.Width = 75;
            Grid.SetRow(b1, grid.RowDefinitions.Count - 1);
            Button b2 = new Button();
            b2.Content = "Einstellungen";
            b2.VerticalAlignment = VerticalAlignment.Center;
            b2.HorizontalAlignment = HorizontalAlignment.Right;
            b2.Margin = new Thickness(10, 10, 10, 10);
            b2.Width = 75;
            Grid.SetRow(b2, grid.RowDefinitions.Count - 1);
            b.Panel = new BotPanelPart(tb, b1, b2);
            #region FlomBot Start Button
            b.Panel.StartButton.Click += delegate (object sender, RoutedEventArgs e)
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
                    else if(b.BotState == FlomBot.State.Errored)
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
            };
            #endregion
            #region FlomBot Settings Button
            b.Panel.SettingsButton.Click += delegate (object sender, RoutedEventArgs e)
            {
                if (!b.IsRunning)
                {
                    TokenDialog td = new TokenDialog(b.Token);
                    td.ShowDialog();
                    setFlomBotToken(b, td.result);
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
        }
        #endregion
        #endregion

        #region Button
        #region WWTB
        #region Start WWTB
        private void startButtonWWTB_Click(object sender, RoutedEventArgs e)
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
                startButtonWWTB.Click -= startButtonWWTB_Click;
                startButtonWWTB.Click += stopButtonWWTB_Click;
            }
        }
        #endregion
        #region Stop WWTB
        private void stopButtonWWTB_Click(object sender, RoutedEventArgs e)
        {
            if (wwtb.IsRunning)
            {
                wwtb.StopBot();
            }
            textBlockWWTB.Background = inactiveBackground;
            startButtonWWTB.Content = "Starten";
            startButtonWWTB.Click -= stopButtonWWTB_Click;
            startButtonWWTB.Click += startButtonWWTB_Click;
        }
        #endregion
        #region Settings WWTB
        private void settingsButtonWWTB_Click(object sender, RoutedEventArgs e)
        {
            if (wwtb.IsRunning)
            {
                TokenDialog td = new TokenDialog(wwtbToken);
                td.ShowDialog();
                setWWTBToken(td.result);
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
        private void buttonStartAchievements_Click(object sender, RoutedEventArgs e)
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
            buttonStartAchievements.Click -= buttonStartAchievements_Click;
            buttonStartAchievements.Click += buttonStopAchievements_Click;
        }
        #endregion
        #region Stop AchBot
        private void buttonStopAchievements_Click(object sender, RoutedEventArgs e)
        {
            if (achBot.IsRunning)
            {
                achBot.StopBot();
            }
            textBlockAchv.Background = inactiveBackground;
            buttonStartAchievements.Content = "Starten";
            buttonStartAchievements.Click -= buttonStopAchievements_Click;
            buttonStartAchievements.Click += buttonStartAchievements_Click;
        }
        #endregion
        #region Achievement Settings
        private void buttonSettingsAchievements_Click(object sender, RoutedEventArgs e)
        {
            if (!achBot.IsRunning)
            {
                TokenDialog td = new TokenDialog(achToken);
                td.ShowDialog();
                setAchvToken(td.result);
            }
            else
            {
                MessageBox.Show("Don't edit the token while the bot is running!");
            }
        }
        #endregion
        #endregion
        #region FlomBot
        #region FlomBot Settings
        private void setFlomBotToken(FlomBot b, string token)
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
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool botRunning = false;
            try
            {
                if (wwtb.IsRunning || achBot.IsRunning) botRunning = true;
            }
            catch { }
            foreach (FlomBot b in bots)
            {
                if (b.IsRunning) botRunning = true;
            }
            if (botRunning)
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
                    achBot.StopBot();
                    foreach (FlomBot b in bots)
                    {
                        b.StopBot();
                    }
                }
            }
        }
        #endregion
    }
}
