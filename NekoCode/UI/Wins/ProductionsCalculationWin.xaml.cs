using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Логика взаимодействия для ProgressWin.xaml
    /// </summary>
    public partial class ProductionsCalculationWin : Window
    {
        public double Progress = 0;
        public bool IsCanceled = false;
        public double timeout;
        NekodeTask t;
        List<double> StaminaProduction = new List<double>();
        List<double> AffintyProduction = new List<double>();
        public List<double>[] result;
        NekoConsole c;
        NekoEngine e;
        DateTime startTime = DateTime.Now;
        bool IsEnded = false;
        double lastStamina;
        double lastAffinity;
        DateTime LastUpdate = DateTime.Now;
        double p = 1;
        void init()
        {
            result = new List<double>[] { StaminaProduction, AffintyProduction };
            c = new NekoConsole(new System.Windows.Controls.StackPanel(), new MainWindow()) { IsEnabled = false };
            e = new NekoEngine(c, t);
            lastStamina = e.Stamina;
            lastAffinity = e.Affinity;
            ApplyTheme(Properties.Settings.Default.Theme);
        }
        public ProductionsCalculationWin(NekodeTask t, double timeout)
        {
            this.t = t;
            this.timeout = timeout;
            InitializeComponent();
            MainProgressBar.Value = 0;


            MessageText.Text = Translator.GetLanguage().CalculatingProductionsProgressMessage;
            init();
        }
        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            IsCanceled = true;
        }
        public void ApplyTheme(string ThemeName)
        {
            Uri uri = new Uri($"/UI/Wins/Themes/{ThemeName}Theme/theme.xaml", UriKind.Relative);
            ResourceDictionary rd = new ResourceDictionary();
            rd.Source = uri;
            CurrentTheme.Source = rd.Source;
            Properties.Settings.Default.Theme = ThemeName;
        }
        private void Window_LayoutUpdated(object sender, EventArgs _)
        {
            if (IsEnded)
            {
                return;
            }
            if (IsLoaded) //&& DateTime.Now.Subtract(LastUpdate).TotalSeconds > 0.1)
            {
                LastUpdate = DateTime.Now;

                t.Next();
                
                Progress = p / t.Script.Count;
                if (DateTime.Now.Subtract(startTime).TotalSeconds > timeout || IsCanceled || t.IsFinished || t.IsStopped)
                {
                    Progress = 1;
                    end();
                    return;
                }
                p += 1;
                result[0].Add(e.Stamina - lastStamina);
                result[1].Add(e.Affinity - lastAffinity);
                lastStamina = e.Stamina;
                lastAffinity = e.Affinity;
            }
        }

        void end()
        {
            result[0].Add(t.StaminaProducted);
            result[1].Add(t.AffinityProducted);
            IsEnded = true;
            this.Close();
        }

        private void MainProgressBar_LayoutUpdated(object sender, EventArgs e)
        {
            
            MainProgressBar.Value = (Progress * 100);
        }
    }
}
