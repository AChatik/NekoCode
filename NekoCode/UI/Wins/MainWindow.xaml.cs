using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Policy;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using NekoCode.UI.Controls;
using NekoCode.Nekode;
using NekoCode.Nekode.Dialogues;
using System.Windows.Media.Animation;
using NekoCode.Model.NekoCode;
using NekoCode.Model.Nekode.Dialogues;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Linq;
using System.Collections.Specialized;

namespace NekoCode
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public NekoEngine Engine;
        public NekoEngine DialogueEngine;
        public NekoConsole MainOutputConsole;
        public string[] StartupArgs = new string[0];
        public bool IsManualLayoutUpdate = false;
        public static RoutedCommand OpenNewShortcut = new RoutedCommand();
        public NekodeBrain Brain;
        public Queue<NekodeDialogue> Dialogues = new Queue<NekodeDialogue>();
        public NekodeDialogue CurrentDialogue = null;
        public double UserIdleTime = 0;
        public List<NekoConsoleInputTrigger> InputTriggers = new List<NekoConsoleInputTrigger>();
        public double ConsoleMssageAppearingAnimationDuration = 0.5;
        public int SelectedRecentCommandID = 0;

        DateTime HintPanelLastShowTime = DateTime.MinValue;
        bool IsHintVisible = false;
        Duration HintPanelCurrentDuration = Duration.Forever;
        DateTime LastActiveTime = DateTime.Now;
        DateTime RandomDialogueLastTime = DateTime.Now;
        double RandomDialogueDelay = 10;
        double LayoutUpdateDelay = 0.1;
        DateTime LastLayoutUpdateTime = DateTime.MinValue;
        Action EmptyDelegate = delegate () { };
        bool IsLocked = false;
        DateTime LoadedTime = DateTime.MinValue;
        

        public MainWindow()
        {
            InitializeComponent();
            NekodeArea.Children.Remove(DialoguePanel);
            DialoguePanel.Opacity = 0;
            OpenNewShortcut.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control));
            //InputTextBox.IsEnabled = false;
            ApplyTheme(Properties.Settings.Default.Theme);
            UpdateLastProjects();
            Brain = new NekodeBrain();
            
            DialogueEngine = new NekoEngine(new NekoConsole(DialogueConsole, this));
            DialogueEngine.MainTask.Do("@meet me");
            
            UpdateRandomDialogueDelay();
            HintMessageeText.Text = "Initializated";
            HideHint();
            HideDialogueCodeExample();
            HideUIElement(RecentCommandsPanel, 0);
            //AddDialogue(NekodeDialogue.FromTextScenrio(Translator.GetLanguage().Help_AboutIfs_Dialogue));
            if (Properties.Settings.Default.IsFirstRun)
            {
                InitDialogue(NekodeDialogue.FromTextScenrio("Please, select language\n"));
                foreach (string l in new string[] { "RU", "EN" })
                {
                    AddChoiceToDialogue(
                            new DialogueChoice(l, delegate (object sender, EventArgs e)
                            {
                                Properties.Settings.Default.IsFirstRun = false;
                                Properties.Settings.Default.Language = l;
                                Reboot();
                            })
                        );
                }
            }
            else
            {
                if (Brain.Memory.ContainsEventType(NekodeMemoryEventTypes.FirstDialogue))
                {
                    AddRandomStartDialogue();
                }

            }
            if (Brain.Memory.LoadedEventsCount != Properties.Settings.Default.RegisteredNekodeMemorySize && Brain.Memory.ContainsEventType(NekodeMemoryEventTypes.FirstDialogue))
            {
                //ShowHint($"{Brain.Memory.Events.Count}, {Brain.Memory.NewEventsCount}, {Properties.Settings.Default.RegisteredNekodeMemorySize}");
                Brain.AddTask(NekodeBrainTask.AskAboutMemorySize);
            }
            UpdateRecentCommands(false);
        }
        /// <summary>
        /// Добавляяет случайный начальный диалог в очередь
        /// </summary>
        public void AddRandomStartDialogue()
        {
            List<string> rm = Translator.GetLanguage().RandomStartMessages;
            Duration? ShowTime = new Duration(TimeSpan.FromSeconds(5));
            if (new Random().NextDouble() <= 0.2)
            {
                rm = Translator.GetLanguage().RandomStartDialogues;
                ShowTime = null;
            }
            NekodeDialogue d = NekodeDialogue.FromTextScenrio(rm[new Random().Next(0, rm.Count)]);
            d.ShowTime = ShowTime;
            AddDialogue(d);
        }
        public void Reboot()
        {
            RebootHeader.Text = Translator.GetLanguage().RebootHeader;
            RebootDescription.Text = Translator.GetLanguage().RebootDescription;
            ShowUIElement(RebootOverlay,1,2, delegate (object sender, EventArgs e)
            {
                Thread.Sleep(1000);
                try
                {
                    Process newNekoCode = Process.Start("NekoCode.exe");
                    Application.Current.Shutdown(0);
                }
                catch
                {
                    RebootHeader.Text = Translator.GetLanguage().RebootErrorHeader;
                    RebootDescription.Text = Translator.GetLanguage().RebootErrorDescription;
                }
            });
            Save();
            
        }
        public void ResetSettings()
        {
            Properties.Settings.Default.Reset();
        }
        private void UpdateRandomDialogueDelay(int min=20, int max=80)
        {
            RandomDialogueDelay = new Random().Next(min, max);
        }
        private void InitEngine()
        {
            string filename = "";
            bool showpath = false;
            foreach (string arg in StartupArgs)
            {
                if (arg.Length > 0 && arg[0] != '/')
                {
                    filename = arg;
                }
                else
                {
                    if (arg == "/showpath") showpath = true;
                }
            }
            if (filename == "")
            {
                Engine = new NekoEngine(MainOutputConsole);
            }
            else
            {
                try
                {
                    Engine = new NekoEngine(MainOutputConsole, filename);
                }
                catch
                {
                    Engine = new NekoEngine(MainOutputConsole);
                }
            }
            if (showpath)
            {
                MainOutputConsole.WriteLine(filename);
            }
        }
        public void AddLastProject(string source)
        {
            bool IsExist = false;
            foreach(string s in Properties.Settings.Default.LastProjects)
            {
                if (s == source) IsExist = true;
            }
            if (IsExist)
            {
                Properties.Settings.Default.LastProjects.Remove(source);
            }
            Properties.Settings.Default.LastProjects.Insert(0,source);
            UpdateLastProjects();
        }
        public void AddLastProject(NekodeTask t)
        {
            if (Engine.MainTask.SourceFile != null)
            {
                AddLastProject(Engine.MainTask.SourceFile.OriginalString);
            }
        }
        public void RunEngine()
        {
            //new System.Threading.Thread(new System.Threading.ThreadStart(Engine.Run)).Start();
            //Engine.Run();
            AddLastProject(Engine.MainTask);
            Brain.Memory.Add(new NekodeMemoryEvent(NekodeMemoryEventTypes.UserProgramExecution, "Executing NekoCode program..."));
            LastActiveTime = DateTime.Now;
        }
        public void UpdateLastProjects()
        {
            LastProjectsStackPanel.Children.Clear();
            foreach (string ProjectSource in Properties.Settings.Default.LastProjects)
            {
                ProjectItem item = new ProjectItem(ProjectSource);
                LastProjectsStackPanel.Children.Add(item);
            }
        }
        public void UpdateStatsInfo()
        {
            AffinityLabel.Content = $"Affinity: {Math.Round(Engine.Affinity,1)}";
            AffinityProgressBar.Value = 100 + Engine.Affinity;
            StaminaLabel.Content = $"Stamina: {Math.Round(Engine.Stamina,1)}";
            StaminaProgressBar.Value = Engine.Stamina;
            SpeedLabel.Content = $"Delay: {Math.Round(Engine.UpdateDelay,3)}s.";
            SpeedProgressBar.Value = 25-Engine.UpdateDelay*100;
        }
        /// <summary>
        /// Конвертирует объект для вывода в консоль
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public dynamic ConvertDataForOutput(dynamic i)
        {
            if (i == null) i = new TextBlock() { Text = $"{i}", Style = CurrentTheme["TextBlockStyle"] as Style };
            else if (i.GetType() == typeof(string)) i = new TextBlock() { Text = i, Style = CurrentTheme["TextBlockStyle"] as Style };
            else if (i.GetType() == typeof(int)) i = new TextBlock() { Text = $"{i}", Style = CurrentTheme["TextBlockStyle"] as Style };
            else if (i.GetType() == typeof(double)) i = new TextBlock() { Text = $"{i}", Style = CurrentTheme["TextBlockStyle"] as Style };
            else if (i.GetType() == typeof(float)) i = new TextBlock() { Text = $"{i}", Style = CurrentTheme["TextBlockStyle"] as Style };
            else if (i.GetType() == typeof(bool))
            {
                i = new CheckBox() { IsChecked = i, IsEnabled = false};
                i.Style = CurrentTheme["CheckBoxStyle"] as Style;
            }
            else if (i.GetType() == typeof(NekodeTask))
            {

                NekodeTask t = i;
                StackPanel s = new StackPanel();
                //s.Margin = new Thickness(5, 5, 5, 5);
                s.Style = CurrentTheme["StackPanelStyle"] as Style;
                s.Children.Add(new Label { Content = $"{Translator.GetLanguage().Task} \"{i.Name}\"", Margin = new Thickness(5, 5, 5, 0), Style = CurrentTheme["LabelStyle"] as Style });

                Button open = new Button { Content = Translator.GetLanguage().OpenInNekoEditor, Margin = new Thickness(5, 5, 5, 5) };
                open.Click += (object sender, RoutedEventArgs e) => OpenSourceCodeInNekoEditor(t);
                open.Style = CurrentTheme["ButtonStyle"] as Style;
                s.Children.Add(open);
                
                i = s;
            }
            else if (i.GetType() == typeof(List<Variable>))
            {
                var l = i;
                StackPanel s = new StackPanel();
                //s.Style = OutputStyle;
                //s.Children.Add(new Label { Content = $"{Translator.GetLanguage().List}", Margin = new Thickness(5, 5, 5, 0), Style = OutputStyle });
                ListBox lb = new ListBox { Style=CurrentTheme["ListBoxStyle"] as Style, Margin = new Thickness(5, 5, 5, 5), MaxHeight = 300, Width=100 };
                foreach (var item in l)
                {
                    lb.Items.Add($"{item.Value}");
                }
                s.Children.Add(lb);
                s.Style = CurrentTheme["StackPanelStyle"] as Style;
                i = s;
            }
            if (i.GetType() == typeof(TextBlock)) (i as TextBlock).TextWrapping = TextWrapping.Wrap;
            i.Margin = new Thickness(0, 0, 0, 10);
            //else i = new Label() { Content = $"{i}" };
            //i.Style = OutputStyle;
            return i;
        }
        /// <summary>
        /// Выводит объект в консоль
        /// </summary>
        /// <param name="c">Объект StackPanel в который добавится объект</param>
        /// <param name="i">Объект, который нужно вывести</param>
        public void AddToConsole(StackPanel c, dynamic i)
        {
            i = ConvertDataForOutput(i);
            c.Children.Add(i);
            ThicknessAnimation ta = new ThicknessAnimation(new Thickness(0, 0, 0, -30), new Thickness(0, 0, 0, 10), new Duration(TimeSpan.FromSeconds(ConsoleMssageAppearingAnimationDuration)));
            ta.EasingFunction = new QuinticEase();
            DoubleAnimation a = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0.2)));
            a.EasingFunction = new QuinticEase();
            i.BeginAnimation(OpacityProperty, a);
            i.BeginAnimation(MarginProperty,ta);
            try
            {
                (c.Parent as ScrollViewer).ScrollToEnd();
                
            }
            catch { }
        }
        /// <summary>
        /// Выводит текст в главную консоль (NekoConsole)
        /// </summary>
        /// <param name="c"></param>
        /// <param name="text"></param>
        public void WriteToMainConsole(StackPanel c, string text)
        {
            AddToConsole(c, text);
        }   
        /// <summary>
        /// Отображает окно ввода данных
        /// </summary>
        /// <param name="message"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public string Input(string message="", string text="")
        {
            InputWindow inp = new InputWindow(message, text);
            inp.ShowDialog();
            return inp.result;
        }
        /// <summary>
        /// Выполняет код NekoCode в главной задаче текущего движка
        /// </summary>
        /// <param name="command">Код NekoCode</param>
        public void DoCommand(string command)
        {
            string specialCommand = command.ToLower();
            if (command == "привет" && InputTriggers.Count == 0) 
            {
                if (Engine.Affinity > -5)
                {
                    Engine.MainTask.Do("me.say(\"Привет, привет!\")");
                }
            }
            else 
            { 
                Engine.MainTask.Do(command); 
                for (int i = 0; i < InputTriggers.Count; i++)
                {
                    NekoConsoleInputTrigger t = InputTriggers[i];
                    if (t.MaxCallCount == 0)
                    {
                        InputTriggers.Remove(t);
                    }
                    else if (command == t.Command)
                    {
                        Console.WriteLine("\\\\\n   EVENT CALL\n\\\\");
                        t.CommandExecuted(EventArgs.Empty);
                        t.MaxCallCount -= 1;
                    }
                }
            }
        }
        public void RunNekoEditor(Uri source=null)
        {
            if (IsLocked)
            {
                WriteToMainConsole(NekoConsole, Translator.GetLanguage().ActionLocked);
                return;
            }
            CodeEditor ce;
            if (source != null)
            {
                ce = new CodeEditor(source);
            }
            else
            {
                ce = new CodeEditor();
            }
            ce.InitMainWindow = this;
            LastActiveTime = DateTime.Now;
            ce.Show();
        }
        public void OpenSourceCodeInNekoEditor(NekodeTask i)
        {
            AddLastProject(i);
            if (IsLocked)
            {
                WriteToMainConsole(NekoConsole, Translator.GetLanguage().ActionLocked);
                return;
            }

            CodeEditor ce = new CodeEditor();
            if (i.SourceFile != null)
            {
                RunNekoEditor(i.SourceFile);
                return;
            }
            else
            {
                ce = new CodeEditor(i.GetScriptAsString());
            }
            ce.AffinityProduction = i.AffinityProducted;
            ce.StaminaProduction = i.StaminaProducted;
            ce.InitMainWindow = this;
            LastActiveTime = DateTime.Now;
            ce.ShowDialog();
        }
        public void OpenFileInNekoEditor(Uri source)
        {
            AddLastProject(source.OriginalString);
            RunNekoEditor(source);
        }
        public void RunFile(string f)
        {
            if (IsLocked)
            {
                WriteToMainConsole(NekoConsole, Translator.GetLanguage().ActionLocked);
                return;
            }
            Engine = new NekoEngine(MainOutputConsole, f);
            RunEngine();
        }
        /// <summary>
        /// Сохраняет настройки и память
        /// </summary>
        public void Save()
        {
            Brain.Memory.Save();
            Properties.Settings.Default.RegisteredNekodeMemorySize = Brain.Memory.SavedEventsCount;
            Properties.Settings.Default.Save();
        }
        /// <summary>
        /// Применяет тему ко всей программе
        /// </summary>
        /// <param name="ThemeName">Название темы</param>
        public void ApplyTheme(string ThemeName)
        {
            if (IsLocked)
            {
                WriteToMainConsole(NekoConsole, Translator.GetLanguage().ActionLocked);
                return;
            }
            Uri uri = new Uri($"/UI/Wins/Themes/{ThemeName}Theme/theme.xaml", UriKind.Relative);
            ResourceDictionary rd = new ResourceDictionary();
            rd.Source = uri;
            CurrentTheme.Source = rd.Source;
            Properties.Settings.Default.Theme = ThemeName;
        }
        public void Update()
        {
            UserIdleTime = DateTime.Now.Subtract(LastActiveTime).TotalSeconds;
            if (IsLoaded)
            {
                if (!IsLocked)
                {
                    Engine.UpdateTasks();
                    UpdateStatsInfo();
                    if (UserIdleTime >= 10)
                    {
                        Brain.Update();
                    }
                    
                }
                if (DateTime.Now.Subtract(LastLayoutUpdateTime).TotalSeconds >= LayoutUpdateDelay)
                {
                    if ( HintPanelCurrentDuration != Duration.Forever && IsHintVisible)
                    {
                        if (DateTime.Now.Subtract(HintPanelLastShowTime).TotalSeconds >= HintPanelCurrentDuration.TimeSpan.TotalSeconds)
                        {
                            HideHint();
                        }
                    }
                    if (CurrentDialogue != null) { RandomDialogueLastTime = DateTime.Now; }
                    if (DateTime.Now.Subtract(RandomDialogueLastTime).TotalSeconds >= RandomDialogueDelay)
                    {
                        NekodeDialogue randD = Nekode.Dialogues.Dialogues.RandomDialogues[new Random().Next(0, Nekode.Dialogues.Dialogues.RandomDialogues.Count)].Copy();
                        if (randD.Script.Count == 1)
                        {
                            randD.ShowTime = TimeSpan.FromSeconds(1);
                        }
                        InitDialogue(randD);
                        
                        UpdateRandomDialogueDelay();
                    }
                    if (IsManualLayoutUpdate) return;
                    if (IsLoaded)
                    {
                        int a = Engine.GetUnfinishedTasks();
                        int s = Engine.GetStoppedTasks();
                        TaskCountInfoTextBlock.Text = $"{Translator.GetLanguage().MyTasks}: {Engine.MyTasks.Count} ({a} {Translator.GetLanguage().Unfinished})";
                        if (a == 0)
                        {
                            TaskCountInfoTextBlock.Foreground = CurrentTheme["SpecialColor"] as SolidColorBrush;
                        }
                        else
                        {
                            TaskCountInfoTextBlock.Foreground = CurrentTheme["InfoFontColor"] as SolidColorBrush;
                        }
                        if (s >= 0) TaskCountInfoTextBlock.Text += $"\n{Translator.GetLanguage().Stopped}: {s}";

                    }
                    LastLayoutUpdateTime = DateTime.Now;
                }
                if (DateTime.Now.Subtract(LoadedTime).TotalSeconds > 1 && !Engine.IsRunning && Dialogues.Count > 0 && CurrentDialogue == null)
                {
                    this.InitDialogue(Dialogues.Dequeue());
                }
            }
        }
        /// <summary>
        /// Запрещает использовать функции, которые могут помешать диалогу (такие как открытие NekoEditor)
        /// </summary>
        public void Lock()
        {
            IsLocked = true;
        }
        /// <summary>
        /// Разрешает использовать функции, которые могут помешать диалогу (такие как открытие NekoEditor)
        /// </summary>
        public void Unlock()
        {
            IsLocked = false;
        }
        private void CollapseUIElement(dynamic UIElement, bool remove=false)
        {
            UIElement.Visibility = Visibility.Collapsed;
            if (remove)
            {
                UIElement.Parent.Children.Remove(UIElement);
            }
        }
        /// <summary>
        /// Скрывает элемент с анимацией
        /// </summary>
        /// <param name="UIElement">Элемент, который нужно скрыть</param>
        /// <param name="time">Продолжительность анимации</param>
        /// <param name="remove">Убрать элемент из Children родителя?</param>
        public void HideUIElement(dynamic UIElement, double time = 0.4, bool remove = false)
        {
            DoubleAnimation a = new DoubleAnimation(UIElement.Opacity, 0, new Duration(TimeSpan.FromSeconds(time)));
            a.EasingFunction = new QuinticEase();
            a.Completed += delegate (object sender, EventArgs e)
            {
                CollapseUIElement(UIElement, remove);
            };
            UIElement.IsHitTestVisible = false;
            UIElement.BeginAnimation(OpacityProperty, a);
            

        }
        /// <summary>
        /// Отображает элемент с анимацией
        /// </summary>
        /// <param name="UIElement">Элемент, который нужно отобразить</param>
        /// <param name="opacity">Целевая прозрачность (по умолчанию 1)</param>
        /// <param name="time">Продолжительность анимации</param>
        /// <param name="OnAnimationEnd">Вызывается при завершении анимации</param>
        public void ShowUIElement(dynamic UIElement, double opacity=1, double time = 0.4, EventHandler OnAnimationEnd=null)
        {
            
            DoubleAnimation a = new DoubleAnimation(UIElement.Opacity, opacity, new Duration(TimeSpan.FromSeconds(time)));
            a.EasingFunction = new QuinticEase();
            if (OnAnimationEnd != null)
            {
                a.Completed += OnAnimationEnd;
            }
            UIElement.BeginAnimation(OpacityProperty, a);
            UIElement.IsHitTestVisible = true;
            UIElement.Visibility = Visibility.Visible;
        }
        /// <summary>
        /// Скрывает панель диалога
        /// </summary>
        /// <param name="fadeOutTime">Продолжительность анимации пропадания</param>
        public void HideDialoguePanel(double fadeOutTime=0.5)
        {
            HideDialogueCodeExample();
            DoubleAnimation a = new DoubleAnimation(DialoguePanel.Opacity, 0, new Duration(TimeSpan.FromSeconds(fadeOutTime)));
            a.EasingFunction = new QuarticEase();
            DialoguePanel.BeginAnimation(OpacityProperty,a);
            a.Completed += CollapseDialoguePanel;
            DialoguePanel.IsHitTestVisible = false;
            
        }
        
        private void CollapseDialoguePanel(object sender, EventArgs e)
        {
            DialoguePanel.Visibility = Visibility.Collapsed;
            
        }
        void ShowDialoguePanelEnd(object sender, EventArgs e)
        {
            if (CurrentDialogue == null) return;
            if (CurrentDialogue.IsEnd)
            {
                HideDialoguePanel(3);
                CurrentDialogue = null;
            }
        }
        /// <summary>
        /// Отображает панель диалога
        /// </summary>
        public void ShowDialoguePanel()
        {
            DialoguePanel.IsHitTestVisible = true;
            DialoguePanel.IsEnabled = true;
            DoubleAnimation a = new DoubleAnimation(DialoguePanel.Opacity, 0.95, new Duration(TimeSpan.FromSeconds(0.5)));
            a.EasingFunction = new QuarticEase();
            a.Completed += ShowDialoguePanelEnd;
            DialoguePanel.BeginAnimation(OpacityProperty, a);

        }
        /// <summary>
        /// Добавляет диалог в очередь
        /// </summary>
        /// <param name="d">Диалог, который нужно добавить</param>
        public void AddDialogue(NekodeDialogue d)
        {
            Dialogues.Enqueue(d);
        }
        /// <summary>
        /// Убирает кнопки выбора из текущего диалога
        /// </summary>
        public void ClearDialogueChoices()
        {
            int i = 0;
            foreach (dynamic b in DialoguePanelButtonsArea.Children)
            {
                HideUIElement(b, 0.4,true);
                i++;
            }
            ShowUIElement(DialoguePanelNextButton);
        }
        /// <summary>
        /// Добавляет кнопку выбора в текущий диалог
        /// </summary>
        /// <param name="choice"></param>
        public void AddChoiceToDialogue(DialogueChoice choice)
        {

            if (CurrentDialogue == null)
            {
                Console.WriteLine("Current dialogue is null! Can't add choice");
                return;
            }
            if (DialoguePanelButtonsArea.Children.Count >= 6)
            {
                return;
            }
            
            Button b = new Button();
            b.Style = CurrentTheme["ButtonStyle"] as Style;
            b.Content = choice.ButtonText;
            b.Opacity = 0;
            b.Click += delegate (object sender, RoutedEventArgs e)
            {
                choice.Select();
                DialogueNext(this, new RoutedEventArgs());
            };
             b.HorizontalAlignment = HorizontalAlignment.Left;
            //b.HorizontalAlignment = HorizontalAlignment.Stretch; 
            b.Height = 40;
            b.MinWidth = 100;
            b.MaxWidth = 180;
            b.Margin = new Thickness(2.5);
            DialoguePanelButtonsArea.Children.Add(b);
            ShowUIElement(b);
            HideUIElement(DialoguePanelNextButton);
            Console.WriteLine($"{choice.ButtonText}");
        }
        /// <summary>
        /// Отображает подсказку над полем ввода консоли
        /// </summary>
        /// <param name="message">Текст подсказки</param>
        /// <param name="duration">Время отображения подсказки</param>
        public void ShowHint(string message, Duration? duration = null)
        {
            if (duration == null) duration = Duration.Forever;
            HintPanelCurrentDuration = (Duration)duration;
            HintMessageeText.Text = message;
            DoubleAnimation a = new DoubleAnimation(0, 0.8, new Duration(TimeSpan.FromSeconds(0.2)));
            a.EasingFunction = new QuinticEase();
            ThicknessAnimation ta = new ThicknessAnimation(new Thickness(0, 0, 10, -50), new Thickness(0, -50, 10, 0), new Duration(TimeSpan.FromSeconds(0.5)));
            ta.EasingFunction = new QuinticEase();
            HintPanel.BeginAnimation(OpacityProperty, a);
            HintPanel.BeginAnimation(MarginProperty, ta);
            IsHintVisible = true;
            HintPanelLastShowTime = DateTime.Now;
        }
        /// <summary>
        /// Скрывает подсказку
        /// </summary>
        public void HideHint()
        {
            DoubleAnimation a = new DoubleAnimation(0.8, 0, new Duration(TimeSpan.FromSeconds(0.2)));
            a.EasingFunction = new QuinticEase();
            ThicknessAnimation ta = new ThicknessAnimation( new Thickness(0, -50, 10, 0), new Thickness(0, 0, 10, -50), new Duration(TimeSpan.FromSeconds(0.5)));
            ta.EasingFunction = new QuinticEase();
            HintPanel.BeginAnimation(OpacityProperty, a);
            HintPanel.BeginAnimation(MarginProperty, ta);
            IsHintVisible = false;
        }
        /// <summary>
        /// Инициализирует диалог, не добавляя его в очередь. Отображает панель диалога
        /// </summary>
        /// <param name="Dialogue"></param>
        void InitDialogue(NekodeDialogue Dialogue)
        {
            RandomDialogueLastTime = DateTime.Now;
            DialogueConsole.Children.Clear();
            Brain.Memory.Add(new NekodeMemoryEvent(NekodeMemoryEventTypes.Info, "Initializating new dialogue..."));
            CurrentDialogue = Dialogue;
            if (CurrentDialogue.ShowTime != null && CurrentDialogue.Script.Count == 1 && CurrentDialogue.Script[0].AutoNext)
            {
                CurrentDialogue.Script[0].AutoNext = false;
            }
            CurrentDialogue.Next();
            if (CurrentDialogue.ShowTime != null)
            {
                DialoguePanelShowTimeProgressBar.Value = 0;
                DialoguePanelShowTimeProgressBar.Maximum = 1;
                DoubleAnimation a = new DoubleAnimation(1, 0, (Duration)CurrentDialogue.ShowTime);
                a.Completed += (sender, args) => {
                    ShowUIElement(DialoguePanelNextButton,1, 0);
                    HideUIElement(DialoguePanelShowTimeProgressBar, 0);
                    HideDialoguePanel();
                    CurrentDialogue = null;
                };
                DialoguePanelShowTimeProgressBar.BeginAnimation(ProgressBar.ValueProperty, a);
                
                ShowDialoguePanel();
                ShowUIElement(DialoguePanelShowTimeProgressBar,0.7,0);
                if (CurrentDialogue.IsEnd)
                {
                    HideUIElement(DialoguePanelNextButton, 0);
                }
                
            }
            else
            {
                if (CurrentDialogue.IsEnd)
                {
                    ShowDialoguePanel();
                }
                else
                {
                    Lock();
                    ShowDialoguePanel();
                }
            }
        }
        public void ShowDialogueCodeExample()
        {
            DialogueCodeExampleRoot.Visibility = Visibility.Visible;
            DialogueCodeExampleStackPanel.Children.Clear();
            ShowUIElement(DialogueCodeExampleRoot);
            DoubleAnimation a = new DoubleAnimation(DialogueCodeExampleRoot.MinWidth, 200,new Duration(TimeSpan.FromSeconds(0.2)));
            a.EasingFunction = new QuinticEase();
            DialogueCodeExampleRoot.BeginAnimation(MinWidthProperty, a);
        }
        public void HideDialogueCodeExample()
        {
            HideUIElement(DialogueCodeExampleRoot);
            DoubleAnimation a = new DoubleAnimation(DialogueCodeExampleRoot.MinWidth, 0, new Duration(TimeSpan.FromSeconds(0.2)));
            a.EasingFunction = new QuinticEase();
            a.Completed += (sender, e) => {
                DialogueCodeExampleRoot.Visibility = Visibility.Collapsed;
            };
            DialogueCodeExampleRoot.BeginAnimation(MinWidthProperty, a);
        }
        public void WriteLineToDialogueCodeExample(string message)
        {
            if (DialogueCodeExampleRoot.Visibility == Visibility.Collapsed)
            {
                ShowDialogueCodeExample();
            }
            if (!message.EndsWith('\n')) { message.Remove(message.Length-1); }
            TextBlock tb = new TextBlock();
            foreach (Run r in CodeEditor.GetHighlightsRuns(message, CurrentTheme))
            {
                tb.Inlines.Add(r);
            }
            tb.FontSize = 13.5;
            tb.Opacity = 0;
            ShowUIElement(tb,1, ConsoleMssageAppearingAnimationDuration, (sender, e) => { DialogueCodeExampleScrollViewer.ScrollToEnd(); });
            ThicknessAnimation a = new ThicknessAnimation(new Thickness(0, 0, 0, -30), new Thickness(0, 0, 0, 10), new Duration(TimeSpan.FromSeconds(ConsoleMssageAppearingAnimationDuration)));
            a.EasingFunction= new QuinticEase();
            tb.BeginAnimation(MarginProperty, a);
            
            DialogueCodeExampleStackPanel.Children.Add(tb);
        }
        public void NextRecentCommand()
        {
            ShowUIElement(RecentCommandsPanel, 1, 0.2);
            SelectedRecentCommandID++;
            if (SelectedRecentCommandID >= Properties.Settings.Default.LastCommands.Count) SelectedRecentCommandID = Properties.Settings.Default.LastCommands.Count - 1;
            UpdateRecentCommands();
        }
        public void PreviewRecentCommand()
        {
            ShowUIElement(RecentCommandsPanel, 1, 0.2);
            SelectedRecentCommandID--;
            if (SelectedRecentCommandID < 0) SelectedRecentCommandID = 0;
            UpdateRecentCommands();
        }
        public void UpdateRecentCommands(bool setText=true)
        {
            RecentCommandsStackPanel.Children.Clear();
            int i = 0;
            double offset = 0;
            
            foreach (string command in Properties.Settings.Default.LastCommands)
            {
                TextBlock tb = new TextBlock { Text = command, Height=20 };
                
                tb.Opacity = 0.4;
                if (SelectedRecentCommandID == i)
                {
                    tb.Opacity = 1;
                    tb.Background = CurrentTheme["BodySpecialColor"] as SolidColorBrush;
                    if (setText) InputTextBox.Text = command;
                }

                tb.MouseLeftButtonDown += (sender, e) => {
                    SelectedRecentCommandID = i;
                    UpdateRecentCommands();
                };
                RecentCommandsStackPanel.Children.Add(tb);
                if (i < SelectedRecentCommandID)
                {
                    offset += tb.Height;
                }
                i++;
            }
            if (RecentCommandsStackPanel.Children.Count > 0)
            {
                ScrollViewer sv = RecentCommandsScrollViewer;
                TextBlock target = RecentCommandsStackPanel.Children[SelectedRecentCommandID] as TextBlock;
                sv.ScrollToVerticalOffset(offset - (sv.ActualHeight / 2) + target.Height / 2);
            }


            
        }
        //
        //  Event handlers
        //
        void OpenExamle()
        {
            string s = "";
            foreach (var line in DialogueCodeExampleStackPanel.Children)
            {
                
                for (int i = 0; i < (line as TextBlock).Inlines.Count; i++)
                {
                    Inline inline = (line as TextBlock).Inlines.ElementAt(i);
                    s += new TextRange(inline.ContentStart, inline.ContentEnd).Text;
                }
                s += "\n";
            }
            OpenSourceCodeInNekoEditor(new NekodeTask(s));

        }
        private void CheckDialogueState()
        {
            EventHandler yes = delegate (object sender, EventArgs e) { Unlock(); OpenExamle(); HideDialoguePanel(); CurrentDialogue = null; };
            EventHandler no = delegate (object sender, EventArgs e) { HideDialoguePanel(); CurrentDialogue = null; };
            if (CurrentDialogue.IsEnd)
            {
                Unlock();

                if (DialogueCodeExampleRoot.Visibility == Visibility.Visible)
                {
                    dynamic l = Translator.GetLanguage();
                    InitDialogue(NekodeDialogue.FromTextScenrio(l.OpenExampleInNekoEditor) );
                    AddChoiceToDialogue(new DialogueChoice(l.Yes, yes));
                    AddChoiceToDialogue(new DialogueChoice(l.No, no));
                }
                else
                {
                    CurrentDialogue = null;
                    HideDialoguePanel();
                }

            }
        }
        public void DialogueNext(object sender, RoutedEventArgs e)
        {
            ClearDialogueChoices();
            RandomDialogueLastTime = DateTime.Now;
            if (CurrentDialogue == null) return;
            LastActiveTime = DateTime.Now;
            CheckDialogueState();
            if (CurrentDialogue != null)
            {
                CurrentDialogue.Next(); 
                CheckDialogueState();
            }
        }
        private void MainWin_Loaded(object sender, RoutedEventArgs e)
        {
            MainOutputConsole = new NekoConsole(this.NekoConsole, this);
            InitEngine();
            RunEngine();
            LoadedTime = DateTime.Now;
        }
        private void MainWin_LayoutUpdated(object sender, EventArgs e) => Update();
        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (InputTextBox.Text == "") return;
                string command = InputTextBox.Text;
                //if (!Properties.Settings.Default.LastCommands.Contains(command))
                //{
                    Properties.Settings.Default.LastCommands.Add(command.Replace("\n",""));
                //}
                HideUIElement(RecentCommandsPanel, 0.2);
                DoCommand(command);
                InputTextBox.Text = "";
            }
            if (e.Key == Key.Up)
            {
                if (RecentCommandsPanel.Visibility == Visibility.Collapsed)
                {
                    SelectedRecentCommandID = Properties.Settings.Default.LastCommands.Count ;
                }
                PreviewRecentCommand();
            }
            if (e.Key == Key.Down) NextRecentCommand();
            if (e.Key == Key.Escape)
            {
                if (RecentCommandsPanel.Visibility == Visibility.Visible)
                    HideUIElement(RecentCommandsPanel, 0.2);
                else
                {
                    InputTextBox.Text = "";
                }
            }
        }
        public void OpenFileInNekoEditor(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.ShowDialog();
            if (dlg.FileName == string.Empty)
            {
                return;
            }
            else
            {
                RunNekoEditor(new Uri(dlg.FileName));
            }
        }
        private void OpenNewFileInNekoEditor(object sender, RoutedEventArgs e)
        {
            RunNekoEditor();
        }
        public void ReviveEvents(string corruptedStoryPath)
        {
            Console.WriteLine("Reviving memory events...");
            NekodeMemory mem = new NekodeMemory(new Uri(corruptedStoryPath, UriKind.RelativeOrAbsolute));
            Console.WriteLine("Loading corrupted memory events...");
            mem.Load(false, false);
            mem.Events.Reverse();
            List<NekodeMemoryEvent> revivedEvents = new List<NekodeMemoryEvent>();

            foreach (NekodeMemoryEvent e in mem.Events)
            {
                if (e.EventType != NekodeMemoryEventTypes.MemoryEventLoadError)
                {
                    Brain.Memory.ReviveEvent(e);
                    revivedEvents.Add(e);

                }
            }
            foreach (NekodeMemoryEvent e in revivedEvents)
            {
                mem.Events.Remove(e);
            }
            Console.WriteLine("Saving corrupted memory events...");
            mem.Save(false);
            if (revivedEvents.Count > 0)
            {
                AddDialogue(NekodeDialogue.FromTextScenrio(Translator.GetLanguage().reviveMemoryEvents_Dialogue.Replace("{0}",$"{revivedEvents.Count}")));
                Brain.AddTask(NekodeBrainTask.AnalyseMemory);
            }
            else
            {
                AddDialogue(NekodeDialogue.FromTextScenrio(Translator.GetLanguage().reviveMemoryEvents_Fail_Dialogue));
            }
        }
        private void Nekode_Drop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string f in files)
            {
                if (System.IO.Path.GetExtension(f) == ".mem")
                {
                    var fileName = System.IO.Path.GetFileNameWithoutExtension(f);
                    if (fileName == "corrupted_story")
                    {
                        ReviveEvents(f);
                    }
                    else if (fileName == "our_story")
                    {
                        AddDialogue(NekodeDialogue.FromTextScenrio(Translator.GetLanguage().DropMyMemoryReaction_Dialogue));
                    }
                    else
                    {
                        AddDialogue(NekodeDialogue.FromTextScenrio(Translator.GetLanguage().DropUnknownMemoryReaction_Dialogue));
                    }
                }
                else
                {
                    MainOutputConsole.WriteLine($"Starting {f}...");
                    RunFile(f);
                }

            }
        }
        private void MainWin_Closed(object sender, EventArgs e)
        {
            Save();
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
        private void SetENLanguage(object sender, EventArgs e)
        {
            Properties.Settings.Default.Language = "EN";
        }
        private void SetRULanguage(object sender, EventArgs e)
        {
            Properties.Settings.Default.Language = "RU";
        }
        private void OpenNewShortcut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenNewFileInNekoEditor(sender, e);
        }
        private void SetNightTheme_Click(object sender, RoutedEventArgs e)
        {
            ApplyTheme("Night");
        }
        private void SetCuteTheme_Click(object sender, RoutedEventArgs e)
        {
            ApplyTheme("Cute");
        }
        private void SetCoffeeTheme_Click(object sender, RoutedEventArgs e)
        {
            ApplyTheme("Coffee");
        }
        private void Set1СTheme_Click(object sender, RoutedEventArgs e)
        {
            ApplyTheme("1C");
        }
        private void MenuItemRunDialogue_FirstRun_Click(object sender, RoutedEventArgs e)
        {
            AddDialogue(Nekode.Dialogues.Dialogues.FirstRun.Copy());
        }
        private void MenuItemRunDialogue_RandomDialogue1_Click(object sender, RoutedEventArgs e)
        {
            AddDialogue(Nekode.Dialogues.Dialogues.RandomDialogues[0].Copy());
        }
        private void MenuItemBrainTask_ClearMemoryTrash_Click(object sender, RoutedEventArgs e)
        {
            Brain.AddTask(NekodeBrainTask.ClearMemoryTrash);
        }
        private void MenuItemBrainTask_AnalyseMemory_Click(object sender, RoutedEventArgs e)
        {
            Brain.AddTask(NekodeBrainTask.AnalyseMemory);
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }
        private void ResetSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            InitDialogue(NekodeDialogue.FromTextScenrio("Are you sure? Your metadata (such as the list of \"My projects\" list and the list of recent commands) will be permanently deleted!"));
            AddChoiceToDialogue(new DialogueChoice("Yes", delegate (object sender, EventArgs e) {
                ResetSettings();
                ShowHint("Settings reseted", new Duration(TimeSpan.FromSeconds(5)));
            }));
            AddChoiceToDialogue(new DialogueChoice("No", delegate (object sender, EventArgs e) {
                ShowHint("Canceled", new Duration(TimeSpan.FromSeconds(5)));
            }));
        }
        private void RebootMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Reboot();
        }
        private void AddRandomStartDialogueMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AddRandomStartDialogue();
        }
    }
}

