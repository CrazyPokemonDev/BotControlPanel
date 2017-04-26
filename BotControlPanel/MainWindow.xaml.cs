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
                Directory.CreateDirectory(tokenBasePath);
            }
            File.WriteAllText(wwtbTokenPath, token);
            wwtbToken = token;
            textBlockWWTB.Background = inactiveBackground;
            try { wwtb = new WWTB(wwtbToken); }
#pragma warning disable CS0168 // Variable ist deklariert, wird jedoch niemals verwendet
            catch (ArgumentException e) { textBlockWWTB.Background = erroredBackground; }
#pragma warning restore CS0168 // Variable ist deklariert, wird jedoch niemals verwendet
        }
        #endregion
        #region AchBot Token
        private void setAchvToken(string token)
        {
            if (!Directory.Exists(tokenBasePath))
            {
                Directory.CreateDirectory(tokenBasePath);
            }
            File.WriteAllText(achvTokenPath, token);
            achToken = token;
            textBlockAchv.Background = inactiveBackground;
            try { achBot = new WerewolfAchievementsBotPlus(achToken); }
#pragma warning disable CS0168 // Variable ist deklariert, wird jedoch niemals verwendet
            catch (ArgumentException e) { textBlockAchv.Background = erroredBackground; }
#pragma warning restore CS0168 // Variable ist deklariert, wird jedoch niemals verwendet
        }
        #endregion
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
            TokenDialog td = new TokenDialog(wwtbToken);
            td.ShowDialog();
            setWWTBToken(td.result);
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
            TokenDialog td = new TokenDialog(achToken);
            td.ShowDialog();
            setAchvToken(td.result);
        }
        #endregion
        #endregion
        #endregion

        #region Close Window
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool botRunning = false;
            if (wwtb.IsRunning || achBot.IsRunning) botRunning = true;
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
