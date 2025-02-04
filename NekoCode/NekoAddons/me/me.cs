using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NekoCode.NekoAddons.me
{
    public class me : AddonBase
    {
        public static string Name = "me";
        public static void Inject(NekodeTask injectTo)
        {
            NekodeTask t = new NekodeTask("", Name);
            injectTo.MyEngine.AddSystemVariables(t);
            t.MyEngine = injectTo.MyEngine;
            t.variables.Add(new Variable(VariableTypes.SystemSkill, "hear", Name, true));
            t.variables.Add(new Variable(VariableTypes.SystemSkill, "say", Name, true));
            t.variables.Add(new Variable(VariableTypes.SystemSkill, "pat", Name, true));
            t.variables.Add(new Variable(VariableTypes.SystemSkill, "command", Name, true));
            t.variables.Add(new Variable(VariableTypes.SystemSkill, "display", Name, true));
            t.variables.Add(new Variable(VariableTypes.SystemSkill, "compile", Name, true));
            t.variables.Add(new Variable(VariableTypes.SystemSkill, "eat", Name, true));
            t.variables.Add(new Variable(VariableTypes.SystemSkill, "list", Name, true));
            //t.variables.Add(new Variable(VariableTypes.SystemSkill, "web", Name, true));
            t.UpdateVariables();
            injectTo.variables.Add(new Variable(VariableTypes.Task, Name, t));
        }
        public static Variable hear(Variable sender, List<Variable> args)
        {
            if (!CheckArgs(args, 1))
            {
                
                sender.Source.DoError($"{Translator.GetLanguage().SyntaxError} me.hear(<message: string>)");
                return NekodeTask.NewUnknownVariable();
            }
            string s = sender.Source.MyEngine.OutputConsole.ReadLine($"{args[0].Value}");

            return new Variable(VariableTypes.String, "__INPUT__", s);
        }
        public static Variable display(Variable sender, List<Variable> args)
        {
            Console.WriteLine("me.display >");
            dynamic[] v = new dynamic[args.Count];
            for (int i = 0; i < args.Count; i++)
            {
                Console.WriteLine($"v[{i}]: {args[i].Value}");
                v[i] = args[i].Value;
            }
            sender.Source.MyEngine.OutputConsole.RichWriteLine(v);
            Console.WriteLine("me.display <");
            return NekodeTask.NewUnknownVariable();
        }
        public static Variable command(Variable sender, List<Variable> args)
        {
            Console.WriteLine("me.command >");
            if (!CheckArgs(args, 3))
            {
                sender.Source.DoError($"{Translator.GetLanguage().SyntaxError} me.command(<message>,<button>,<callback_function>)");
                return NekodeTask.NewUnknownVariable();
            }
            Button b = new Button() { Content = args[1].Value };
            //b.HorizontalAlignment = HorizontalAlignment.Right;
            Variable target = args[2];

            b.Click += (object s, RoutedEventArgs e) => target.Call(new List<Variable>()); 
            
            Console.WriteLine("RichWriteLine()");
            sender.Source.MyEngine.OutputConsole.RichWriteLine(new dynamic[] { args[0].Value, b });
            Console.WriteLine("< me.command");
            return NekodeTask.NewUnknownVariable();
            
        }
        public static Variable compile(Variable sender, List<Variable> args)
        {
            if (!CheckArgs(args, VariableTypes.String))
            {
                sender.Source.DoError($"{Translator.GetLanguage().SyntaxError} me.compile(<line>)");
                return NekodeTask.NewUnknownVariable();
            }
            return sender.Source.CompileLine(args[0].Value);
        }
        public static Variable say(Variable sender, List<Variable> args)
        {
            if (!CheckArgs(args, 1))
            {
                sender.Source.DoError($"{Translator.GetLanguage().SyntaxError} me.say(<message>)");
                return NekodeTask.NewUnknownVariable();
            }
            sender.Source.MyEngine.OutputConsole.WriteLine($"{args[0].Value}");
            return NekodeTask.NewUnknownVariable();
        }
        public static Variable pat(Variable sender, List<Variable> args)
        {
            Console.WriteLine("me.pat");
            sender.Source.MyEngine.AddAffinity(sender.Source, 1);
            return NekodeTask.NewUnknownVariable();
        }
        public static Variable web(Variable sender, List<Variable> args)
        {
            if (!CheckArgs(args, VariableTypes.String))
            {
                sender.Source.DoError($"{Translator.GetLanguage().SyntaxError} me.web(<url>)");
                return NekodeTask.NewUnknownVariable();
            }
            try
            {
                sender.Source.MyEngine.OutputConsole.RichWriteLine(new dynamic[] { new WebBrowser() { Source = new Uri(args[0].Value), MinHeight=200, MinWidth=200 } });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return NekodeTask.NewUnknownVariable();
        }
        public static Variable eat(Variable sender, List<Variable> args)
        {
            if (!CheckArgs(args, 1))
            {
                sender.Source.DoError($"{Translator.GetLanguage().SyntaxError} me.eat(<variable>)");
                return NekodeTask.NewUnknownVariable();
            }
            if (args[0].IsSystem)
            {
                sender.Source.DoError(Translator.GetLanguage().YouCantEatSystemVariables);
                return NekodeTask.NewUnknownVariable();
            }
            if (args[0].Source == null || args[0].Source.MyEngine == null)
            {
                sender.Source.DoError(Translator.GetLanguage().YouCantEatConstVariables);
                return NekodeTask.NewUnknownVariable();
            }
            double s = NekoEngine.GetVariableStaminaCost(args[0]);
            Console.WriteLine($"me.eat > add stamina: {s}");
            
            args[0].Source.MyEngine.AddStamina(sender.Source, s);
            args[0].Source.DelVariable(args[0]);
            return NekodeTask.NewUnknownVariable();
        }
        public static Variable list(Variable sender, List<Variable> args)
        {

            List<Variable> list = new List<Variable>();
            foreach(Variable a in args) list.Add(a);
            Variable NewList = new Variable(VariableTypes.List, "__TEMP_LIST__", list, false);
            Console.WriteLine($"me.Llist >> Created new list with {list.Count} items");
            return NewList;
        }

    }
}
