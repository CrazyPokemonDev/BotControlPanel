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
        private const string tokenBasePath = "C:\\Olfi01\\BotControlPanel\\.Tokens\\";
        #endregion
        #region Variables
        private WWTB wwtb;
        private string wwtbToken = "";
        #endregion
        #region Constructor
        public MainWindow()
        {
            InitializeComponent();
            getTokens();
            try { wwtb = new WWTB(wwtbToken); }
#pragma warning disable CS0168 // Variable ist deklariert, wird jedoch niemals verwendet
            catch (ArgumentException e) { textBlockWWTB.Background = erroredBackground; }
#pragma warning restore CS0168 // Variable ist deklariert, wird jedoch niemals verwendet
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
        #endregion
        #endregion

        #region Button 
        #region Start WWTB
        private void startButtonWWTB_Click(object sender, RoutedEventArgs e)
        {
            if (textBlockWWTB.Background == erroredBackground)
            {
                MessageBox.Show("Lege zuerst ein korrektes Token unter Einstellungen fest!");
                return;
            }
            if (!wwtb.isRunning)
            {
                wwtb.startBot();
            }
            textBlockWWTB.Background = activeBackground;
            startButtonWWTB.Content = "Stoppen";
            startButtonWWTB.Click -= startButtonWWTB_Click;
            startButtonWWTB.Click += stopButtonWWTB_Click;
        }
        #endregion
        #region Stop WWTB
        private void stopButtonWWTB_Click(object sender, RoutedEventArgs e)
        {
            if (wwtb.isRunning)
            {
                wwtb.stopBot();
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
    }
}
