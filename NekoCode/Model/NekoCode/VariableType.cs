using System;
using System.Windows.Media.Animation;
namespace NekoCode
{
    public class VariableType
    {
        public string Name = "Unknown";
        public string ConvertMagicMethodName = null;
        public VariableType(string name, string convertMagicMethodName=null)
        {
            Name = name;
            ConvertMagicMethodName = convertMagicMethodName;
        }
        public override string ToString() {return Name;}
    }
}