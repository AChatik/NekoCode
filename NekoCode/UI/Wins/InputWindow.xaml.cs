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

namespace NekoCode
{
    /// <summary>
    /// Логика взаимодействия для InputWindow.xaml
    /// </summary>
    public partial class InputWindow : Window
    {
        public string result = "";

        void init()
        {

            InitializeComponent();
            InputTextBox.Text = "";
            MessageLabel.Content = "Please input line";
            InputTextBox.Focus();
            Focus();
            ApplyTheme(Properties.Settings.Default.Theme);
        }
        public InputWindow(string message="", string defaultText="")
        {
            init();
            MessageLabel.Content = message;
            InputTextBox.Text = defaultText;
        }
        public InputWindow()
        {
            init();
        }
        public void ApplyTheme(string ThemeName)
        {
            Uri uri = new Uri($"/UI/Wins/Themes/{ThemeName}Theme/theme.xaml", UriKind.Relative);
            ResourceDictionary rd = new ResourceDictionary();
            rd.Source = uri;
            CurrentTheme.Source = rd.Source;
            Properties.Settings.Default.Theme = ThemeName;
        }
        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                result = InputTextBox.Text;
                Close();
            }
            if (e.Key == Key.Escape)
            {
                result = "";
                Close();
            }
        }
    }
}
