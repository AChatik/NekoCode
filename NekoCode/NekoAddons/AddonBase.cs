using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoCode.NekoAddons
{
    public class AddonBase
    {
        public static bool CheckArgs(List<Variable> args, int c) => c == args.Count;
        public static bool CheckArgs(List<Variable> args, params VariableType[] types)
        {
            if (types.Length != args.Count) return false;
            for (int i = 0; i < types.Length; i++)
            {
                if (args[i].VariableType != types[i]) return false;
            }
            return true;
        }
        public static bool CheckMinArgs(List<Variable> args, int c) => c <= args.Count;
    }
}
