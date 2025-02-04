using NekoCode.Model.NekoCode;
using NekoCode.Nekode.Dialogues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NekoCode.NekoAddons.system
{
    public class systemAddon : AddonBase
    {
        public static string Name = "system";
        public static void Inject(NekodeTask injectTo)
        {
            NekodeTask t = new NekodeTask("", Name);
            injectTo.MyEngine.AddSystemVariables(t);
            t.MyEngine = injectTo.MyEngine;
            t.variables.Add(new Variable(VariableTypes.SystemSkill, "set_sprite", Name, true));
            t.variables.Add(new Variable(VariableTypes.SystemSkill, "run_dialogue", Name, true));
            t.variables.Add(new Variable(VariableTypes.SystemSkill, "wait_for_console_input", Name, true));
            t.variables.Add(new Variable(VariableTypes.SystemSkill, "show_hint", Name, true));
            t.variables.Add(new Variable(VariableTypes.SystemSkill, "hide_hint", Name, true));
            t.UpdateVariables();
            injectTo.variables.Add(new Variable(VariableTypes.Task, Name, t));
        }
        public static Variable set_sprite(Variable sender, List<Variable> args)
        {
            if (!CheckArgs(args, VariableTypes.String))
            {
                sender.Source.DoError($"{Translator.GetLanguage().SyntaxError} system.set_sprite(<sprite_name: string>)");
                return NekodeTask.NewUnknownVariable();
            }
            string SpriteName = args[0].Value;

            return NekodeTask.NewUnknownVariable();
        }
        public static Variable run_dialogue(Variable sender, List<Variable> args)
        {
            if (!CheckArgs(args, VariableTypes.String))
            {
                sender.Source.DoError($"{Translator.GetLanguage().SyntaxError} system.set_sprite(<sprite_name: string>)");
                return NekodeTask.NewUnknownVariable();
            }
            string DialogueScenario = args[0].Value;
            sender.Source.MyEngine.OutputConsole.MainWindow.AddDialogue(NekodeDialogue.FromTextScenrio(DialogueScenario));
            return NekodeTask.NewUnknownVariable();
        }
        public static Variable show_hint(Variable sender, List<Variable> args)
        {
            if (!CheckMinArgs(args, 1))
            {
                sender.Source.DoError($"{Translator.GetLanguage().SyntaxError} system.show_hint(<sprite_name: string>, [duration: number default -1 (forever)])");
                return NekodeTask.NewUnknownVariable();
            }
            string HintMessage = $"{args[0].Value}";
            Duration duration = Duration.Forever;
            if (args.Count > 1 && args[1].VariableType == VariableTypes.Number)
            {
                duration = new Duration(TimeSpan.FromSeconds(args[1].Value));
            }
            sender.Source.MyEngine.OutputConsole.MainWindow.ShowHint(HintMessage, duration);
            return NekodeTask.NewUnknownVariable();
        }
        public static Variable hide_hint(Variable sender, List<Variable> args)
        {
            sender.Source.MyEngine.OutputConsole.MainWindow.HideHint();
            return NekodeTask.NewUnknownVariable();
        }
        public static Variable wait_for_console_input(Variable sender, List<Variable> args)
        {
            if (!CheckMinArgs(args, 1) || args[0].VariableType != VariableTypes.String)
            {
                sender.Source.DoError($"{Translator.GetLanguage().SyntaxError} system.wait_for_console_input(<command: string>, [auto_next: bool]=yes)");
                return NekodeTask.NewUnknownVariable();
            }
            
            if (sender.Source.MyEngine.OutputConsole.MainWindow.CurrentDialogue != null)
            {
                bool autoNext = true;
                if (args.Count > 1 && args[1].VariableType == VariableTypes.Bool) autoNext = args[1].Value;

                sender.Source.MyEngine.OutputConsole.MainWindow.HideDialoguePanel();
                sender.Source.MyEngine.OutputConsole.MainWindow.Lock();
                NekoConsoleInputTrigger t = new NekoConsoleInputTrigger(args[0].Value, 1);
                t.OnCommandExetuced += delegate (object s, EventArgs e) {
                    sender.Source.MyEngine.OutputConsole.MainWindow.ShowDialoguePanel();
                    if (autoNext) sender.Source.MyEngine.OutputConsole.MainWindow.DialogueNext(null,new RoutedEventArgs());
                };
                sender.Source.MyEngine.OutputConsole.MainWindow.InputTriggers.Add(t);
            }

            return NekodeTask.NewUnknownVariable();
        }
    }
}
