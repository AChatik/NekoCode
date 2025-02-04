using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Security.Policy;
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
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NekoCode
{
    /// <summary>
    /// Логика взаимодействия для CodeEditor.xaml
    /// </summary>
    public partial class CodeEditor : Window
    {
        Uri SourcePath = null;
        public double? StaminaProduction = null, AffinityProduction = null;
        public List<Key> PressedKeys = new List<Key>();
        public bool IsCodeHighlightEnabled = true;
        string LastText = "";
        public static RoutedCommand InsertNewSkillShortcut = new RoutedCommand();
        public static RoutedCommand SaveShortcut = new RoutedCommand();
        public static RoutedCommand RunShortcut = new RoutedCommand();
        public static RoutedCommand CalculateProductionsShortcut = new RoutedCommand();
        bool isSaved = false;
        public MainWindow InitMainWindow = null;
        
        void init()
        {
            InitializeComponent();
            CodeTextBox.Focus();
            //UpdateCodeHighlights();
            InsertNewSkillShortcut.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Alt));
            SaveShortcut.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            RunShortcut.InputGestures.Add(new KeyGesture(Key.F5));
            CalculateProductionsShortcut.InputGestures.Add(new KeyGesture(Key.R, ModifierKeys.Control));
            StaminaCodeLines.Text = "";
            AffinityCodeLines.Text = "";
            CodeTextBox.CaretBrush = CurrentTheme["SpecialColor"] as SolidColorBrush;
            CodeTextBox.SelectionBrush = CurrentTheme["SpecialColor"] as SolidColorBrush;
            ApplyTheme(Properties.Settings.Default.Theme);
            this.PreviewKeyDown += CodeTextBox_KeyDown;
        }
    public void ApplyTheme(string ThemeName)
        {
            Uri uri = new Uri($"/UI/Wins/Themes/{ThemeName}Theme/theme.xaml", UriKind.Relative);
            ResourceDictionary rd = new ResourceDictionary();
            rd.Source = uri;
            CurrentTheme.Source = rd.Source;
            Properties.Settings.Default.Theme = ThemeName;
        }
        public CodeEditor()
        {
            init();
            SetText("@meet me\n");
            CodeTextBox.CaretIndex = 9;
        }
        public CodeEditor(Uri file)
        {
            init();
            try
            {
                StreamReader sr = new StreamReader(file.OriginalString, Encoding.UTF8);
                SetText(sr.ReadToEnd());
                sr.Close();
                SourcePath = file;
                isSaved = true;
            }
            catch
            {
                SetText("#Open file error");
            }
        }
        public CodeEditor(string script)
        {
            init();
            SetText(script);
        }
        public void SetText(string t)
        {
            CodeTextBox.Text = t;
            UpdateCodeHighlights();
        }
        public string GetText()
        {
            return CodeTextBox.Text;
        }
        public bool IsHighlightLineExists(int id)
        {
            foreach (dynamic child in CodeArea.Children)
            {
                if (child.GetType() == typeof(TextBlock) && (child as TextBlock).Name == $"HighlightLine_{id}") return true;
            }
            return false;
        }
        public TextBlock GetHighlinghtLineById(int id)
        {
            foreach (dynamic child in CodeArea.Children)
            {
                if (child.GetType() == typeof(TextBlock) && (child as TextBlock).Name == $"HighlightLine_{id}") return child;
            }
            return null;
        }

        public static List<Run> GetHighlightsRuns(string line, ResourceDictionary CurrentTheme)
        {
            bool isString = false;
            char stringChar = '"';
            bool isSpecialWord = false;
            bool isComment = false;
            List<Run> inlines = new List<Run>();
            for (int i = 0; i < line.Length; i++)
            {
                bool paststring = false;
                char ch = line[i];
                Run run;
                if ((ch == '"' || ch == '\'') && (i == 0 || line[i - 1] != '\\'))
                {
                    if (!isString)
                    {
                        stringChar = ch;
                        isString = !isString;
                    }
                    else if (ch == stringChar)
                    {
                        isString = !isString;
                        paststring = true;
                    }
                }
                if (ch == '@')
                {
                    if (!isString)
                    {
                        isSpecialWord = true;
                    }
                }
                if (ch == '#' && !isString)
                {
                    isComment = !isComment;
                }

                if (ch == ' ') isSpecialWord = false;
                if (isComment)
                {
                    run = new Run(ch.ToString());
                    run.Foreground = CurrentTheme["HighLightsCommentColor"] as SolidColorBrush;
                    inlines.Add(run);
                    isSpecialWord = false;
                }
                else if (ch == '\t' && !isString)
                {
                    run = new Run(ch.ToString());
                    run.Background = (CurrentTheme["HighLightsSpecialColor"] as SolidColorBrush).Clone();
                    run.Background.Opacity = 0;
                    inlines.Add(run);
                }
                else if (isString || paststring)
                {
                    run = new Run(ch.ToString());
                    run.Foreground = CurrentTheme["HighLightsStringColor"] as SolidColorBrush;
                    inlines.Add(run);
                }

                else if (Char.IsDigit(ch))
                {
                    run = new Run(ch.ToString());
                    run.Foreground = CurrentTheme["HighLightsDigitColor"] as SolidColorBrush;
                    inlines.Add(run);
                    isSpecialWord = false;
                }
                else if ((Char.IsLetter(ch) || "_(){}[]".Contains(ch)) && !isSpecialWord)
                {
                    run = new Run(ch.ToString());
                    run.Foreground = CurrentTheme["HighLightsBaseColor"] as SolidColorBrush;
                    inlines.Add(run);
                }
                else
                {
                    run = new Run(ch.ToString());
                    run.Foreground = CurrentTheme["HighLightsSpecialColor"] as SolidColorBrush;
                    inlines.Add(run);
                }
            }
            return inlines;
        }
        public void HighlightLine(string line, int lineid)
        {
            InlineCollection inlines; //= CodeHighlights.Inlines;
            TextBlock tb;
            if (!IsHighlightLineExists(lineid))
            {
                tb = new TextBlock();
                tb.Name = $"HighlightLine_{lineid}";
                tb.FontStyle = CodeTextBox.FontStyle;
                tb.FontFamily = CodeTextBox.FontFamily;
                tb.FontSize = CodeTextBox.FontSize;
                //tb.Text = line;

                tb.FontWeight = CodeTextBox.FontWeight;

                //Console.WriteLine((int)(2 * (lineid + 1) + tb.ActualHeight * lineid));
                //21.25
                tb.Margin = new Thickness(3, (1 * (lineid + 1) + tb.FontSize * 1.33 * lineid), 0, 0);
                CodeArea.Children.Remove(CodeTextBox);

                CodeArea.Children.Add(tb);
                CodeArea.Children.Add(CodeTextBox);
                inlines = tb.Inlines;
            }
            else
            {
                tb = GetHighlinghtLineById(lineid);
                inlines = tb.Inlines;
            }

            if (line != tb.Text)
            {
                inlines.Clear();
                foreach (Run r in GetHighlightsRuns(line, CurrentTheme))
                {
                    inlines.Add(r);
                }
            }
            //tb.Text = line;
        }
        public void UpdateCodeLines()
        {
            CodeLines.FontFamily = CodeTextBox.FontFamily;
            CodeLines.FontSize = CodeTextBox.FontSize;
            CodeLines.FontStyle = CodeTextBox.FontStyle;
            CodeLines.FontStretch = CodeTextBox.FontStretch;
            CodeLines.Text = "";
            CodeLines.Foreground = CurrentTheme["HighLightsLineIdColor"] as SolidColorBrush;
            for (int i = 1; i <= CodeTextBox.Text.Split('\n').Length; i++)
            {
                CodeLines.Text += $"{i}\n";
            }
        }
        public void ClearCodeHighlights()
        {
            CodeArea.Children.Clear();
            CodeArea.Children.Add(CodeTextBox);
        }
        public void UpdateCodeHighlights()
        {
            if (!IsCodeHighlightEnabled) return;
            //CodeHighlights.Text = GetText();
            //InlineCollection inlines = CodeHighlights.Inlines;
            //inlines.Clear();
            for (int i = 0; i < CodeTextBox.Text.Split('\n').Length; i++)// (string line in CodeTextBox.Text.Split('\n'))
            {
                string line = CodeTextBox.Text.Split('\n')[i];
                HighlightLine(line + '\n', i);
            }

            UIElementCollection children = new UIElementCollection(CodeArea, CodeArea);

            for (int i = 0; i < CodeArea.Children.Count; i++)
            {
                dynamic item = CodeArea.Children[i];
                if (item.Name.Contains("HighlightLine_"))
                {
                    int lineId = Convert.ToInt32(item.Name.Split('_')[1]);
                    if (lineId >= CodeTextBox.Text.Split('\n').Length)
                    {
                        CodeArea.Children.Remove(item);
                    }
                }
            }
            UpdateCodeLines();
        }
        void SaveTo(string path)
        {
            StreamWriter sw = new System.IO.StreamWriter(path, false, Encoding.UTF8);
            sw.Write(GetText());
            sw.Close();
            isSaved = true;
        }
        public void SaveAs()
        {
            SaveFileDialog openFileDialog = new SaveFileDialog();
            openFileDialog.Filter = "NekoCode Files | *.neko";
            openFileDialog.DefaultExt = ".neko";
            openFileDialog.ShowDialog();
            if (openFileDialog.FileName != string.Empty)
            {
                System.IO.File.Create(openFileDialog.FileName).Close();
                SaveTo(openFileDialog.FileName);
                SourcePath = new Uri(openFileDialog.FileName);
            }
        }
        private void SaveAs(object sender, RoutedEventArgs e)
        {
            SaveAs();
        }
        private void Save(object sender, RoutedEventArgs e)
        {
            if (SourcePath==null)
            {
                SaveAs();
            }
            else
            {
                SaveTo(SourcePath.OriginalString);
            }
        }

        private void Window_LayoutUpdated(object sender, EventArgs e)
        {
            string ap = (AffinityProduction == null) ? "?" : $"{Math.Round((double)AffinityProduction, 1)}";
            string sp = (StaminaProduction == null) ? "?" : $"{Math.Round((double)StaminaProduction, 1)}";

            AffinityProductionMenuItem.Header = Translator.GetLanguage().AffinityProduction+ap;
            StaminaProductionMenuItem.Header = Translator.GetLanguage().StaminaProduction+sp;
        }

        public void InsertTextToCode(string text, int? addCaretIndex=null)
        {
            int i = CodeTextBox.CaretIndex;
            string r = "";
            if (i == 0)
            {
                r = text + CodeTextBox.Text;
            }
            else
            {
                for (int j = 0; j < CodeTextBox.Text.Length; j++)
                {
                    if (j == i - 1 || (i == 0 && j == 0))
                    {
                        r += "" + CodeTextBox.Text[j] + text;
                    }
                    else
                    {
                        r += CodeTextBox.Text[j];
                    }
                }
            }

            CodeTextBox.Text = r;

            if (addCaretIndex == null) addCaretIndex = text.Length;
            CodeTextBox.CaretIndex = i+(int)addCaretIndex;
        }
        
        public void InsertFeeder(object sender, RoutedEventArgs e)
        {
            InsertTextToCode("@skill feed\r\n\t@skill food\r\n\t\t\"food\" >> \"f1\"\r\n\t@return __THIS__\r\n\tfood()>>\"Food\"\r\n me.eat(Food)\r\n@return __THIS__\r\n\r\nfeed()");
        }
        public void InsertNewSkill(object sender, RoutedEventArgs e)
        {
            InsertTextToCode("@skill \n\n@return __THIS__\n",7);
        }

        private void CodeTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            isSaved = false;
            PressedKeys.Add(e.Key);

            if (e.Key == Key.Enter)
            {
                InsertTextToCode("\n");
                e.Handled = true;
            }
            else if (e.Key == Key.Tab)
            {
                InsertTextToCode("\t");
                e.Handled = true;
            }
            
            
        }

        private void CodeTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            PressedKeys.Remove(e.Key);
        }

        private void CodeHighlights_GotFocus(object sender, RoutedEventArgs e)
        {
            CodeTextBox.Focus();
        }

        private void CodeTextBox_LayoutUpdated(object sender, EventArgs e)
        {
            //UpdateCodeHighlights();
        }

        private void CodeHighlights_LayoutUpdated(object sender, EventArgs e)
        {
            //UpdateCodeHighlights();
        }

        private void CodeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            //CodeTextBox.Focus();
        }

        private void CodeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoaded)
            {
                UpdateCodeHighlights();
                LastText = CodeTextBox.Text;
            }
            
        }

        public bool AskToSave()
        {

            MessageBoxResult r = MessageBox.Show(Translator.GetLanguage().SaveChanges, "Save changes", MessageBoxButton.YesNoCancel);
            if (r.Equals(MessageBoxResult.Yes))
            {
                Save(this, new RoutedEventArgs());
            }
            if (r.Equals(MessageBoxResult.Cancel))
            {
                return true;
            }
            return false;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) 
        { 
            if (!isSaved)
            {
                if (AskToSave())
                {
                    e.Cancel = true;
                }
            }
            
        }

        private void RunMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (InitMainWindow == null)
            {
                MessageBox.Show(Translator.GetLanguage().NekoEditorIsNotPlugged,"Run error",MessageBoxButton.OK);
                return;
            } 
            Close();
            InitMainWindow.Engine = new NekoEngine( InitMainWindow.MainOutputConsole, new NekodeTask(CodeTextBox.Text,"main"));
            InitMainWindow.RunEngine();

        }

        private void CalculateProductionsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            List<double>[] r = NekoEngine.CalculateScriptProductions(new NekodeTask(CodeTextBox.Text));
            StaminaCodeLines.Text = "";
            AffinityCodeLines.Text = "";
            if (r[0].Count > 0)
            {
                StaminaProduction = r[0].Last();
                AffinityProduction = r[1].Last();
                int i = 0;
                foreach (double s in r[0])
                {
                    i++;
                    string v = s > 0 ? "+" : "";
                    StaminaCodeLines.Text += $"{v}{Math.Round(s, 1)}\n";
                    if (i == r[1].Count-1) StaminaCodeLines.Text += "===\n";
                }
                i = 0;
                foreach (double a in r[1])
                {
                    i++;
                    string v = a > 0 ? "+" : "";
                    AffinityCodeLines.Text += $"{v}{Math.Round(a, 1)}\n";
                    if (i == r[1].Count-1) AffinityCodeLines.Text += "===\n";
                }
            }
        }

        private void EnableHighlights_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                IsCodeHighlightEnabled = true;
                UpdateCodeHighlights();
                CodeTextBox.Foreground = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            }

        }

        private void EnableHighlights_Unchecked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                IsCodeHighlightEnabled = false;
                ClearCodeHighlights();
                CodeTextBox.Foreground = CurrentTheme["FontColor"] as SolidColorBrush;
            }

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
