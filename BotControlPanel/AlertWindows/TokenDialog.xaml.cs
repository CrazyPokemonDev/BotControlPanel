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
using System.Windows.Shapes;

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
