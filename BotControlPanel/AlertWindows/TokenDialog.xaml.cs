using System.Windows;

namespace BotControlPanel.AlertWindows
{
    /// <summary>
    /// Interaktionslogik für TokenDialog.xaml
    /// </summary>
    public partial class TokenDialog : Window
    {
        public string result { get; set; }
        public TokenDialog(string currentToken)
        {
            InitializeComponent();
            textBox.Text = currentToken;
            result = currentToken;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            result = textBox.Text;
            DialogResult = true;
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
