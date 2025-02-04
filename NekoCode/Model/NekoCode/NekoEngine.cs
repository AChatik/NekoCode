using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace NekoCode
{
    public class NekoEngine
    {
        public List<NekodeTask> MyTasks = new List<NekodeTask>();
        public NekodeTask MainTask;
        public NekodeTask Nekode;
        public NekoConsole OutputConsole;
        public double Affinity = 0;
        public double Stamina = 100;
        public bool IsRunning = false;
        public double UpdateDelay = 0;
        DateTime LastUpdateTime = DateTime.MinValue;
        public void InitNekodeTask()
        {
            Nekode = new NekodeTask("", "Nekode");
            Nekode.MyEngine = this;
            Nekode.variables.Add(new Variable(VariableTypes.Number, "__AFFINITY__", Affinity, true));
            Nekode.variables.Add(new Variable(VariableTypes.Number, "__STAMINA__", Stamina, true));
            Nekode.variables.Add(new Variable(VariableTypes.Number, "__NAME__", "Nekode", true));
        }
        public void UpdateNekodeTask()
        {
            Nekode.GetVariableByName("__AFFINITY__",true).Value = Affinity;
            Nekode.GetVariableByName("__STAMINA__", true).Value = Stamina;
        }
        public NekoEngine(NekoConsole outputConsole)
        {
            InitNekodeTask();
            OutputConsole = outputConsole;
            string defaultScript = Translator.GetLanguage().DefaultScript;
            MainTask = InitNewTask(defaultScript, "main");
            MainTask.GetVariableByName("__MAIN_TASK__").Value = MainTask;
        }
        public NekoEngine(NekoConsole outputConsole, string scriptPath)
        {
            InitNekodeTask();
            OutputConsole = outputConsole;
            MainTask = InitNewTask(new Uri(scriptPath), "main");
            MainTask.GetVariableByName("__MAIN_TASK__").Value = MainTask;
        }
        public NekoEngine(NekoConsole outputConsole, NekodeTask main)
        {
            InitNekodeTask();
            OutputConsole = outputConsole;
            MainTask = main;
            AddSystemVariables(main);
            main.MyEngine = this;
            MyTasks.Add(main);
            MainTask.GetVariableByName("__MAIN_TASK__").Value = MainTask;
        }
        public string GetRandomAffinityErrorText()
        {
            string[] texts = new string[]
            {
                $"Affinity error: \"{Translator.GetLanguage().AffinityErrorNekodeMessage1}\"",
                $"Affinity error: \"{Translator.GetLanguage().AffinityErrorNekodeMessage2}\"",
            };
            return texts[new Random().Next(0, texts.Length - 1)];
        }
        public void UpdateTasks()
        {
            if (DateTime.Now.Subtract(LastUpdateTime).TotalSeconds < UpdateDelay)
            {
                return;
            }
            for (int i = 0; i < MyTasks.Count; i++)
            {
                UpdateNekodeTask();
                NekodeTask t = MyTasks[i];
                if (Affinity <= -100)
                {
                    t.DoError(GetRandomAffinityErrorText());
                }
                if (!(t.WaitingFor == null || t.WaitingFor.IsFinished || t.WaitingFor.IsStopped)) continue;
                if (t.IsStopped) continue;
                if (t.IsFinished) continue;
                AddStamina(t, -0.6);
                t.Next();
            }
            LastUpdateTime = DateTime.Now;
            if (Stamina < 0) Stamina = 0;
            if (Stamina > 100) Stamina = 100;
            UpdateDelay = (Stamina < 70)? (100 - Stamina)/400:0;
        }
        public void Run()
        {
            if (IsRunning) return;
            IsRunning = true;
            while (GetActiveTasks() > 0)
            {
                UpdateTasks();
            }
            IsRunning = false;
        }
        public NekodeTask InitNewTask(Uri taskSource, string name = "task")
        {
            StreamReader sr = new StreamReader(taskSource.OriginalString, Encoding.UTF8);
            string script = sr.ReadToEnd();
            sr.Close();
            NekodeTask t = InitNewTask(script, name);
            t.SourceFile = taskSource;
            return t;
        }
        public NekodeTask InitNewTask(string script, string name = "task")
        {
            NekodeTask t = new NekodeTask(script, name);
            
            t.MyEngine = this;
            AddSystemVariables(t);
            MyTasks.Add(t);
            return t;
        }
        public NekodeTask GetTaskByName(string name)
        {
            foreach (NekodeTask t in MyTasks)
            {
                if (t.Name == name) return t;
            }
            return null;
        }

        public void AddSystemVariables(NekodeTask t)
        {
            t.variables.Add(new Variable(VariableTypes.String, "__NEKOCODE_VERSION__", "1.0", true));
            t.variables.Add(new Variable(VariableTypes.Task, "__MAIN_TASK__", MainTask, true));
            t.variables.Add(new Variable(VariableTypes.Task, "__NEKODE__", Nekode, true));
        }
        public VariableType GetNewVariableTypeByValue(string value)
        {
            return VariableTypes.String;
        }
        public dynamic ConvertValue(string value)
        {
            return value; 
        }
        public void AddStamina(NekodeTask t, double s)
        {

            t.StaminaProducted += s;
            Stamina += s;
            Stamina = Math.Round(Stamina, 1);
            UpdateNekodeTask();
        }
        public void AddAffinity(NekodeTask t, double a)
        {
            t.AffinityProducted += a;
            Affinity += a;
            Affinity = Math.Round(Affinity, 1);
            UpdateNekodeTask();
        }
        public int GetActiveTasks()
        {
            int c = 0;
            foreach (var t in MyTasks)
            {
                if (!t.IsFinished && !t.IsStopped) c++;
            }
            return c;
        }
        public int GetUnfinishedTasks()
        {
            int c = 0;
            foreach (var t in MyTasks)
            {
                if (!t.IsFinished) c++;
            }
            return c;
        }
        public int GetStoppedTasks()
        {
            int c = 0;
            foreach (var t in MyTasks)
            {
                if (t.IsStopped) c++;
            }
            return c;
        }
        /// <summary>
        /// Добавляет аддон в движок, создёт переменную в задаче
        /// </summary>
        /// <param name="path">Расположение файла аддона</param>
        /// <param name="sender">Задача, в которую нужно добавить переменную аддона</param>
        /// <param name="meetAlong">Добавить аддон параллельно?</param>
        public void GetAddon(string path, NekodeTask sender, bool meetAlong=false) //meetAlong - include as parallel
        {
            if (path == "me")
            {
                NekoAddons.me.me.Inject(sender);
            }
            else if (path == "system")
            {
                NekoAddons.system.systemAddon.Inject(sender);
            }
            else
            {
                if (File.Exists(path))
                {
                    NekodeTask t = InitNewTask(new Uri(path), System.IO.Path.GetFileNameWithoutExtension(path));
                    sender.variables.Add(new Variable(VariableTypes.Task, t.Name, t));
                    if (!meetAlong)
                    {
                        sender.WaitingFor = t;
                    }
                }
                else
                {
                    sender.DoError(Translator.GetLanguage().AddonNotFound);
                }
            }
        }
        public static List<double>[] CalculateScriptProductions(NekodeTask t, double timeout=15)
        {
            ProductionsCalculationWin w = new ProductionsCalculationWin(t, timeout);
            w.ShowDialog();
            Console.WriteLine("CalculateScriptProductions > RETURN");
            return w.result;
        }
        public static double GetVariableStaminaCost(Variable variable)
        {
            return Variable.GetDeepAttr(variable).Count * 3.5;
        }
    }
}
