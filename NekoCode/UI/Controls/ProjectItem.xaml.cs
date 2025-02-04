using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Security.Policy;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Media.Animation;

namespace NekoCode.UI.Controls
{
    /// <summary>
    /// Логика взаимодействия для ProjectItem.xaml
    /// </summary>
    public partial class ProjectItem : UserControl
    {
        public string ProjectSource = "Folder/Name.neko";
        int maxSourceLen = 35;
        string currentThemeName = "";
        DateTime DeleteTime = DateTime.MinValue;

        public ProjectItem(string source = "Folder/Name.neko")
        {
            InitializeComponent();
            ProjectSource = source;
            ProjectPath.Content = source.Length > maxSourceLen ? CutString(source,0, maxSourceLen/2) +"\\...\\"+CutString(source, source.Length-maxSourceLen/2, source.Length) : source;
            ProjectName.Content = System.IO.Path.GetFileNameWithoutExtension(ProjectSource);
        }
        public static string CutString(string s, int start, int end)
        {
            string r = "";
            for (int i = start; i < end && i < s.Length; i++)
            {
                r += s[i];
            }
            return r;
        }
        public void ApplyTheme(string ThemeName)
        {
            Uri uri = new Uri($"/UI/Wins/Themes/{ThemeName}Theme/theme.xaml", UriKind.Relative);
            ResourceDictionary rd = new ResourceDictionary();
            rd.Source = uri;
            CurrentTheme.Source = rd.Source;
            currentThemeName = ThemeName;
        }
        private void UserControl_LayoutUpdated(object sender, EventArgs e)
        {
            
            if (IsLoaded && currentThemeName != Properties.Settings.Default.Theme)
            {
                ApplyTheme(Properties.Settings.Default.Theme);
            }
            if (DeleteTime != DateTime.MinValue && DateTime.Now.Subtract(DeleteTime).TotalSeconds > 0.5)
            {
                Delete(this, new EventArgs());
                DeleteTime = DateTime.MinValue;
            }
        }

        private void HideCommand_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
            
            DoubleAnimation a1 = new DoubleAnimation( 270, 0, new Duration(TimeSpan.FromSeconds(0.5)),FillBehavior.HoldEnd) { EasingFunction = new QuarticEase() };
            DoubleAnimation a2 = new DoubleAnimation( 100, 0, new Duration(TimeSpan.FromSeconds(0.5)), FillBehavior.HoldEnd) { EasingFunction = new QuarticEase() };
            DoubleAnimation a3 = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(0.5)), FillBehavior.HoldEnd) { EasingFunction = new QuarticEase()};
            a2.Completed += Delete;
            Body.BeginAnimation(WidthProperty, a1);
            Body.BeginAnimation(HeightProperty, a2);
            Body.BeginAnimation(OpacityProperty, a3);
            DeleteTime = DateTime.Now;
        }

        private void Delete(object sender, EventArgs e)
        {
            
            Properties.Settings.Default.LastProjects.Remove(ProjectSource);
            (Application.Current.MainWindow as MainWindow).UpdateLastProjects();
            
        }

        private void EditCommand_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            (Application.Current.MainWindow as MainWindow).OpenFileInNekoEditor(new Uri(ProjectSource));
        }

        private void RunCommand_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            (Application.Current.MainWindow as MainWindow).RunFile(ProjectSource);
        }
    }
}
