using NekoCode.Nekode.Dialogues;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Controls;
using System.Windows.Diagnostics;
using System.Windows;
namespace NekoCode
{
    public static class BasicCommands
    {
        public static void mem(NekodeTask t)
        {
            string text = $"Task name: {t.Name}";
            string names = Translator.GetLanguage().memNameTitle;
            string types = Translator.GetLanguage().memTypeTitle;
            string values = Translator.GetLanguage().memValueTitle;
            string s = Translator.GetLanguage().memIsSystemTitle;
            foreach (Variable v in t.variables)
            {
                var val = v.Value;
                if (v.VariableType == VariableTypes.List) { val = $"({val.Count} items)"; }
                //text += $"\n\t{v.Name}: {v.Value} | {v.VariableType.Name} | Is system: {v.IsSystem}";
                names += "\n" + v.Name;
                values += $"\n{val}";
                types += "\n" + v.VariableType.Name;
                s += $"\n{v.IsSystem}";
            }
            t.MyEngine.OutputConsole.RichWriteLine(new dynamic[] { text,names,values,types,s});
            //return text;
        }
        public static void nya(NekodeTask t, string line)
        {
            if (line.Trim() == "@nya") t.MyEngine.OutputConsole.WriteLine("");
            else t.MyEngine.OutputConsole.WriteLine(line);
        }
        public static void info(NekodeTask t, string line)
        {
            string varName;
            try
            {
                varName = line.Split(new char[] { ' ' }, 2)[1];
            }
            catch
            {
                t.DoError(Translator.GetLanguage().SyntaxError + " @info <var name>");
                return;
            }
            Variable v = t.GetVariableByName(varName);
            string Out = $"Variable \"{v.Name}\":\n\tType: {v.VariableType.Name} ({v.VariableType})\n\tValue: {v.Value}\n\tAttributes:";
            foreach (Variable a in v.GetAllAttributes())
            {
                Out += $"\n\t\tName: \"{a.Name}\" Type: {a.VariableType.ToString()} Value: {a.Value.ToString()}";
            }
            t.MyEngine.OutputConsole.WriteLine(Out);
        }
        public static void help(NekodeTask t, string line)
        {
            dynamic l = Translator.GetLanguage();
            //t.CompileLine("me.say()");
            string[] Headers = [l.Help_AboutVariables, l.Help_AboutSkills, l.Help_AboutIfs];
            string[] Scripts = [l.Help_AboutVariables_Dialogue, l.Help_AboutSkills_Dialogue, l.Help_AboutIfs_Dialogue];
            //t.MyEngine.OutputConsole.MainWindow.AddDialogue(NekodeDialogue.FromTextScenrio(Translator.GetLanguage().HelpDialogue));
            
            Button b;
            for (int i = 0; i < Headers.Length && i < Scripts.Length; i++) 
            {
                b = new Button() { Content = Headers[i] };
                string s = Scripts[i];
                b.Click += (sender, e) => {
                    (System.Windows.Application.Current.MainWindow as MainWindow).AddDialogue(NekodeDialogue.FromTextScenrio(s));
                };
                t.MyEngine.OutputConsole.RichWriteLine([b]);
            }
    }
        public static void meet(NekodeTask t, string line)
        {
            line = NekodeTask.ClearLine(line).Replace("\t", "").Replace("\n", "");
            string path = line.Split()[1];
            bool meetAlong = false;
            try
            {
                string param = line.Split()[2];
                if (param == "along")
                {
                    meetAlong = true;
                }
            }
            catch {

            }
            t.MyEngine.GetAddon(path, t, meetAlong);
        }
        public static void doin(NekodeTask t, string line)
        {
            try
            {
                string taskName = line.Trim().Split()[1];
                Variable v = t.GetVariableByName(taskName);
                if (v.VariableType != VariableTypes.Task)
                {
                    t.DoError(Translator.GetLanguage().TaskNotFound.Replace("{}", taskName));
                }
                else
                {
                    t.DoIn = v.Value;
                }
            }
            catch
            {
                t.DoError(Translator.GetLanguage().SyntaxError+" @doin <taskname>");
            }
        }
        public static void stop(NekodeTask t, string line)
        {
            try
            {
                string taskName = line.Trim().Split()[1];
                Variable v = t.GetVariableByName(taskName);
                if (v.VariableType != VariableTypes.Task)
                {
                    t.DoError(Translator.GetLanguage().TaskNotFound.Replace("{}", taskName));
                }
                else
                {
                    v.Value.IsStopped = true;
                }
            }
            catch
            {
                t.IsStopped = true;
            }
        }
    }
}
