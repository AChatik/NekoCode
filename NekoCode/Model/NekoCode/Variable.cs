using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Ink;
using System.Windows.Markup;

namespace NekoCode
{
    public class Variable
    {
        public NekodeTask Source;
        public Variable Parent = null;
        public bool IsSystem = false;
        public string Name = "unknown";
        public VariableType VariableType = VariableTypes.Unknown;
        public dynamic Value = null;
        public List<Variable> Attributes = new List<Variable>();
        public Variable(VariableType t, string name, dynamic value, bool isSystem=false) 
        {
            this.Name = name;
            this.VariableType = t;
            this.Value = value;
            this.IsSystem = isSystem;
            if (t == VariableTypes.List)
            {
                Attributes.Add(new Variable(VariableTypes.SystemSkill, "roll", "__LIST_SKILL__", true) { Parent = this });
                Attributes.Add(new Variable(VariableTypes.SystemSkill, "map", "__LIST_SKILL__", true) { Parent = this });
                Attributes.Add(new Variable(VariableTypes.SystemSkill, "clear", "__LIST_SKILL__", true) { Parent = this });
                Attributes.Add(new Variable(VariableTypes.SystemSkill, "add", "__LIST_SKILL__", true) { Parent = this });
                Attributes.Add(new Variable(VariableTypes.SystemSkill, "get", "__LIST_SKILL__", true) { Parent = this });
                Attributes.Add(new Variable(VariableTypes.SystemSkill, "find", "__LIST_SKILL__", true) { Parent = this });
            }
        }
        public int MaxRecursiveCalls = 50;
        int RecursiveCalls = 0;
        public void UpdateAttrs()
        {
            for (int i = 0; i < Attributes.Count; i++)
            {
                Attributes[i].Source = Source;
                Attributes[i].Parent = this;
                Attributes[i].UpdateAttrs();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>Список атрибутов, список переменных, если это Task</returns>
        public List<Variable> GetAllAttributes()
        {
            if (VariableType == VariableTypes.Task)
            {
                return Value.variables;
            }
            else
            {
                return Attributes;
            }
            
        }
        public static List<Variable> GetDeepAttr(Variable v, Variable sender=null)
        {
            List<Variable> res = new List<Variable>();
            if (sender == v) return res;
            if (v.VariableType == VariableTypes.Task && v.IsSystem)
            {
                return res;
            }
            res.Add(v);
            if (v.VariableType == VariableTypes.Task)
            {
                Console.WriteLine("GetDeepAttr > Task");
                foreach (Variable attr in (v.Value as NekodeTask).variables)
                {
                    if (attr.IsSystem || attr == sender) continue;
                    Console.WriteLine($" {attr.Name}:");
                    foreach (Variable attrsAttr in Variable.GetDeepAttr(attr, v))
                    {
                        res.Add(attrsAttr);
                        Console.WriteLine($"    {attrsAttr.Name}");
                    }
                }
            }
            else
            {
                foreach (Variable attr in v.Attributes)
                {
                    if (attr.IsSystem || attr == sender) continue;
                    res.Add(attr);
                    foreach (Variable attrsAttr in Variable.GetDeepAttr(attr,v))
                    {
                        res.Add(attrsAttr);
                    }
                }
            }

            return res;
        }
        public static int GetCountOfArguments(Variable v)
        {
            int c = 0;
            foreach (Variable attr in v.Attributes) if (attr.VariableType == VariableTypes.SkillArgument) c++;
            return c;
        }
        public static Variable Unknown { get { return new Variable(VariableTypes.Unknown,"unknown",null); } }
        /// <summary>
        /// Вызывает навык __call__ (написанный на NekoCode типо как в питоне) из атрибутов переменной.
        /// Если навыка __call__ нет, то возращает низвестную переменную
        /// Если тип переменной - навык, то вызывает навык
        /// </summary>
        /// <param name="Args">Параметры вызова</param>
        /// <returns></returns>
        public dynamic Call(List<Variable> Args)
        {
            
            Trace.WriteLine($"RecursiveCalls: {RecursiveCalls}");
            RecursiveCalls++;
            if (RecursiveCalls >= MaxRecursiveCalls) {
                Source.DoError(Translator.GetLanguage().RecursionError);
                return Unknown;
            }
            
            dynamic r = Call_(Args);
            RecursiveCalls--;
            return r;
        }
        dynamic Call_(List<Variable> Args) 
        {
            
            Console.WriteLine($"Calling {Name} with {Args.Count} arguments");
            if (VariableType == VariableTypes.SystemSkill)
            {
                return NekodeSystemSkills.DoSkill(this, Args);
            }
            else if (VariableType == VariableTypes.Skill)
            {
                if (Args.Count != GetCountOfArguments(this))
                {
                    Source.DoError(Translator.GetLanguage().SkillArgsError.Replace("{1}", Name).Replace("{2}", $"{GetCountOfArguments(this)}").Replace("{3}", $"{Args.Count}"));
                }
                NekodeTask tempT = new NekodeTask("");
                tempT.MyEngine = Source.MyEngine;
                tempT.Script = Value.Script;
                tempT.Name = $"Skill \"{Name}\" object";
                tempT.MyEngine = Source.MyEngine;

                foreach (Variable v in Source.variables)
                {
                    if (v.Name != "__THIS__")
                    tempT.UpdateVariable(v);
                }
                tempT.UpdateVariable(new Variable(VariableTypes.Task, "__CALL_SENDER_TASK__", Source, true));
                for (int i = 0; i < Args.Count;i++)
                {
                    Variable v = Args[i];
                    tempT.UpdateVariable(new Variable(v.VariableType, Attributes[i].Name, v.Value));
                } 
                
                Console.WriteLine($"SKILL {Name} PROCESSING >>>>>>> ");
                while (!tempT.IsStopped && !tempT.IsFinished)
                {
                    tempT.Next();
                }
                Console.WriteLine($"SKILL {Name} PROCESSING <<<<<<< ");
                string returnV = GetAttribute("__RETURN_VARIABLE_NAME__").Value;
                Console.WriteLine($"SKILL {Name} RETURNING VARIABLE \"{returnV}\"");
                return tempT.GetVariableByName(returnV);
            }
            else if (IsAttributeExists("__call__"))
            {
                GetAttribute("__call__").Call(Args);
            }
            if (Source != null) Source.DoError(Translator.GetLanguage().CallVariableError.Replace("{0}",Name));
            return Unknown; 
        }
        public bool IsAttributeExists(string attrName)
        {
            
            foreach (Variable a in ( VariableType==VariableTypes.Task ? Value.variables : Attributes))
            {
                if (a.Name == attrName) return true;
            }
            return false;
        }
        public Variable GetAttribute(string attrName)
        {
            if (VariableType == VariableTypes.Number && IsSystem)
            {
                if (NekodeTask.IsOnlyDigits(attrName))
                {
                    Value += Convert.ToInt32(attrName) * (0.1 * attrName.Length);
                    return this;
                }
            }
            if (VariableType == VariableTypes.Task)
            {
                //Console.WriteLine(Value.GetVariableByName(attrName).Value);
                return Value.GetVariableByName(attrName);
            }

            foreach (var attr in Attributes)
            {
                if (attr.Name == attrName)
                {
                    return attr;
                }
            }
            if (Source != null) Source.DoError($"Variable {Name} (type: {VariableType}) has not attribute \"{attrName}\"");
            return NekodeTask.NewUnknownVariable();
        }
        public Variable ConvertTo(VariableType to)
        {
            bool succ = false;
            Variable res = new Variable(VariableType, "__CONVERTED_TEMP__", Value);
            if (VariableType == VariableTypes.Task)
            {
                if (to.ConvertMagicMethodName != null)
                {
                    if ((Value as NekodeTask).IsVariableExists(to.ConvertMagicMethodName))
                    {
                        res.Value = (Value as NekodeTask).GetVariableByName(to.ConvertMagicMethodName).Call(new List<Variable>());
                        succ = true;
                    }
                    
                }
            }
            else
            {

            }
            if (!succ)
            {
                Source.DoError(Translator.GetLanguage().ConvertationError.Replace("{0}", VariableType.Name).Replace("{1}", to.Name));
                return Unknown;
            }
            else
            {
                return Unknown;
            }
        }
    }
}
