using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation.Peers;

namespace NekoCode
{
    public class ListDefaultSkills
    {
        public static Variable roll(Variable sender, List<Variable> args)
        {
            if (!NekoAddons.AddonBase.CheckArgs(args, 1))
            {
                sender.Source.DoError($"{Translator.GetLanguage().SyntaxError} list.roll(<handler(item)>)");
                return NekodeTask.NewUnknownVariable();
            }
            int c = 0;
            NekodeTask s = args[0].Source;
            foreach (Variable item in sender.Parent.Value)
            {
                Console.WriteLine($"roll >> skill task: {args[0].Source} Calling {args[0].Name} ({args[0].VariableType}) with {item.Value}");
                args[0].Call(new List<Variable> { item });
                args[0].Source = s;
                c++;
            }
            return new Variable(VariableTypes.Number, "__ROLL_COUNT_TEMP_NUMBER__", (double)c);
        }
        public static Variable map(Variable sender, List<Variable> args)
        {
            if (!NekoAddons.AddonBase.CheckArgs(args, 1))
            {
                sender.Source.DoError($"{Translator.GetLanguage().SyntaxError} list.map(<handler(item)>)");
                return NekodeTask.NewUnknownVariable();
            }
            
            List<Variable> res = new List<Variable>();
            NekodeTask s = args[0].Source;
            foreach (Variable item in sender.Parent.Value)
            {
                Console.WriteLine($"roll >> skill task: {args[0].Source} Calling {args[0].Name} ({args[0].VariableType}) with {item.Value}");
                res.Add(args[0].Call(new List<Variable> { item }));
                args[0].Source = s;
            }
            return new Variable(VariableTypes.List, "__MAP_LIST_TEMP__", res);
        }
        public static Variable clear(Variable sender, List<Variable> args)
        {
            if (!NekoAddons.me.me.CheckArgs(args, 0)) {
                sender.Source.DoError($"{Translator.GetLanguage().SyntaxError} list.clear()");
                return sender.Parent;
            }
            sender.Parent.Value = new List<Variable>();
            return sender.Parent;
        }
        public static Variable add(Variable sender, List<Variable> args)
        {
            if (NekoAddons.me.me.CheckMinArgs(args,1))
            {
                sender.Source.DoError($"{Translator.GetLanguage().SyntaxError} list.add(<item 1>, <item 2>, ..., <item 3>)");
                return sender.Parent;
            }
            foreach (Variable item in args)
            {
                sender.Parent.Value.Add(item);
            }
            return sender.Parent;
        }
        public static Variable get(Variable sender, List<Variable> args)
        {
            if (NekoAddons.me.me.CheckArgs(args, VariableTypes.Number))
            {
                sender.Source.DoError($"{Translator.GetLanguage().SyntaxError} list.get(<index>)");
                return NekodeTask.NewUnknownVariable();
            }
            int index = args[0].Value;
            if (sender.Parent.Value.Count-1 >= index) return sender.Parent.Value[index];
            sender.Source.DoError($"{Translator.GetLanguage().ListIndexError}");
            return NekodeTask.NewUnknownVariable();
        }
        public static Variable find(Variable sender, List<Variable> args)
        {
            if (NekoAddons.me.me.CheckArgs(args, 1))
            {
                sender.Source.DoError($"{Translator.GetLanguage().SyntaxError} list.get(<item>)");
                return new Variable(VariableTypes.Number,"__INDEX__", -1);
            }
            for (int i = 0; i < sender.Parent.Value.Count; i++)
            {
                if (sender.Parent.Value[i] == args[0])
                {
                    return new Variable(VariableTypes.Number, "__INDEX__", i);
                }
            }
            return new Variable(VariableTypes.Number, "__INDEX__", -1);
        }
    }
}
