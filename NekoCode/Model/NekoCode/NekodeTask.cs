using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xaml;
using System.Xml.Linq;

namespace NekoCode
{
    public class NekodeTask
    {
        public NekoEngine MyEngine;
        public string Name = "my hard task";
        public List<Variable> variables = new List<Variable>();
        public List<string> Script = new List<string>();
        public int CurrentLineId = 0;
        public bool IsFinished = false;
        public bool IsStopped = false;
        public NekodeTask DoIn;
        public NekodeTask WaitingFor = null;
        public Uri SourceFile = null;
        public double StaminaProducted = 0;
        public double AffinityProducted = 0;
        bool isSkillScript = false;
        NekodeTask tempSkillTask = null;
        Variable tempSkillVariable = null;
        int SkillCodeDepth = 0;
        int BlockDepth = 0;
        int RequiredBlockDepth = 0;
        public const string spec = "-=+/|\'\"\\()[]{}.,?$@%^<>!&*;:";
        public const string digits = "1234567890";
        Stack<int> WaitLinePoints = new Stack<int>();
        
        public override string ToString()
        {
            return $"Nekode's task \"{Name}\"";
        }
        public NekodeTask(string script, string name = "my hard task")
        {
            Script = new List<string>(script.Split('\n'));
            Name = name;
            DoIn = this;
            variables.Add(new Variable(VariableTypes.Task, "__THIS__", this, true));
            variables.Add(new Variable(VariableTypes.String, "__TASK_NAME__", Name, true));
            variables.Add(new Variable(VariableTypes.Bool, "__IS_CONSOLE_LOCKED__", false, true));
        }
        public Variable GetVariableByName(string name, bool IsSystemCall = false, bool isEqOperator=false)
        {
            if (!IsSystemCall) MyEngine.AddStamina(this, -0.2); ;
            foreach (Variable v in variables)
            {
                if (v.Name == name) return v;
            }
            if (!isEqOperator)  DoError(Translator.GetLanguage().VariableNotFound.Replace("{name}", name));
            else
            {
                return new Variable(VariableTypes.Unknown,"__CREATE_NEW_VAR__", null, true);
            }
            return new Variable(VariableTypes.Unknown, "unknown", null);
        }
        public bool IsVariableExists(string name)
        {
            foreach (Variable v in variables)
            {
                if (v.Name == name) return true;
            }
            return false;
        }
        public string GetScriptAsString()
        {
            string res = "";
            foreach (string s in Script) res += s+"\n";
            return res;
        }
        public bool IsVariableNameLegit(string n)
        {
            if (n.Length == 0) return false;
            foreach(char c in n) if (spec.Contains(c)) return false;
            return true;
        }
        /// <summary>
        /// Добавляет переменную в variables
        /// Если переменная уже существует - изменяет ее
        /// </summary>
        /// <param name="v">Добавляемая переменная</param>
        public void UpdateVariable(Variable v)
        {
            if (!IsVariableNameLegit(v.Name))
            {
                DoError(Translator.GetLanguage().VariableNameError);
                return;
            }
            MyEngine.AddStamina(this, -0.5);
            //MyEngine.Stamina -= 0.5;
            if (IsVariableExists(v.Name))
            {
                variables.Remove(GetVariableByName(v.Name));
                UpdateVariable(v);
                //Variable s = GetVariableByName(v.Name);
                //s.Value = v.Value;
                //s.VariableType = v.VariableType;
            }
            else
            {
                variables.Add(v);
            }
        }
        public static bool CheckLine(string line)
        {
            if (line.Length == 0) return false;

            return true;
        }
        public static Variable NewUnknownVariable() => new Variable(VariableTypes.Unknown, "Unknown", null);
        public static string ClearLine(string line)
        {
            return line.Trim('\n', '\t', ' ').Replace("\n", "");
        }
        public static bool IsOnlyDigits(string line, string ignore = "")
        {
            foreach (char c in line) if (!digits.Contains(c) && !ignore.Contains(c)) return false;
            return true;
        }
        public void DelVariable(Variable v)
        {
            
            Trace.WriteLine("DelVariable > " + v.Name);
            List<Variable> temp = new List<Variable>();
            for (int i = 0; i < variables.Count; i++)
            {
                if (!(variables[i].Name == v.Name))
                {
                    temp.Add(variables[i]);
                }
            }
            variables = temp;
            //variables.Remove(v);
        }
        public Variable FindValueInLocalMemory(string dataName)
        {
            foreach (Variable v in variables)
            {
                if (v.Name == dataName) return v;
            }
            return NewUnknownVariable();
        }
        public static string ClearSpacesForCompile(string line)
        {
            string r = "";
            bool isString = false;
            char stringChar = '\0';

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"' || c=='\'')
                {
                    if (!isString)
                    {
                        isString = true;
                        stringChar = c;
                    }
                    else
                    {
                        if (stringChar == c)
                        {
                            if (line[i - 1] != '\\')
                            {
                                isString = false;
                            }
                        }
                    }

                }
                if ((c != ' ' && c != '\t') || isString)
                {
                    r += c;
                }
            }
            return r;
        }
        public void Jump(int lineId)
        {
            CurrentLineId = lineId;
        }
        public Variable CompileLine(string line, int start=0, bool isList=false)
        {   //me.say("hi")
            //__THIS__.me.say(__THIS__.__THIS__.__NAME__)
            line = line.Replace("\n", "");
            line = ClearSpacesForCompile(line);
            if (!isList)Trace.WriteLine($"=== CompileLine >>> Compiling line (c: {BlockDepth}, r: {RequiredBlockDepth}): " + line.Remove(0, start));
            if (!line.EndsWith(";"))
            {
                line += ';';
            }
            
            string dataName = "";
            bool isString = false;
            char stringChar = '\0';
            Variable last = new Variable(VariableTypes.Unknown, "__TEMP__", null, true);
            Variable defaulLast = last;
            Variable list = new Variable(VariableTypes.List, "", new List<Variable>(), true);
            Variable args = new Variable(VariableTypes.List, "", new List<Variable>(), true);
            bool skipParams = false;
            bool isAttrName = false;
            bool isMath = false;
            string MathOperator = "";
            Variable lastMath = new Variable(VariableTypes.Unknown, "__TEMP__", null, true);
            Variable lastF = new Variable(VariableTypes.Unknown, "__TEMP__", null, true);
            string res = "";
            int CallDeepLevel = 0;
            bool isComment = false;

            for (int i = start; i < line.Length; i++)
            {

                char c = line[i];
                if (c == '#' && !isString) isComment = true;
                if (isComment && !(c == ';' && i == line.Length-1)) continue;
                if (c == '\n' || c =='\r') continue;
                if (!isString && new char[] { ' ', '\t'}.Contains(c)) continue;
                if (skipParams && !"()".Contains(c)) continue;
                else if (!spec.Contains(c) || (isString && (c!=stringChar || (c == stringChar && line[i-1]=='\\'))))
                {
                    if (c == '\\' && new char[] { '"', '\'' }.Contains(line[i + 1]))
                    {
                        continue;
                    }
                    //if (isString) Trace.WriteLine($"String: {c}");
                    dataName += c;
                }
                else
                {
                    //Trace.WriteLine($"dataName: {dataName}");
                    
                    if (isAttrName)
                    {

                        last = last.GetAttribute(dataName);
                        //Trace.WriteLine($"attr: {last.Name} {last.Value}");
                        isAttrName = false;
                        dataName = "";
                    }
                    else if (dataName.Length > 0 && !isString)
                    {
                        if (IsOnlyDigits(dataName))
                        {
                            last = new Variable(VariableTypes.Number, dataName, Convert.ToDouble(dataName),true);
                        } 
                        else
                        {
                            Trace.WriteLine($"r: {RequiredBlockDepth} c: {BlockDepth} dataName=\"{dataName}\"");
                            if (dataName == "yes") last = new Variable(VariableTypes.Bool, "__TEMP_BOOL__", true);
                            else if (dataName == "no") last = new Variable(VariableTypes.Bool, "__TEMP_BOOL__", false);
                            else if (dataName == "if")
                            {
                                last = new Variable(VariableTypes.SystemOperator, "if", true);
                                lastF = last;
                                BlockDepth++;
                            }
                            else if (dataName == "elif")
                            {
                                if (BlockDepth <= 0)
                                {
                                    DoError(Translator.GetLanguage().elifOperatorError);
                                    return Variable.Unknown;
                                }

                                last = new Variable(VariableTypes.SystemOperator, "elif", true);
                                lastF = last;
                            }
                            else if (dataName == "else")
                            {
                                last = new Variable(VariableTypes.SystemOperator, "else", true);
                                if (BlockDepth <= 0)
                                {
                                    DoError(Translator.GetLanguage().elseOperatorError);
                                    return Variable.Unknown;
                                }
                                if (RequiredBlockDepth == BlockDepth) RequiredBlockDepth--;
                                
                                else if (RequiredBlockDepth+1 == BlockDepth)
                                {
                                    RequiredBlockDepth++;
                                }
                            }
                            else if (dataName == "while")
                            {
                                last = new Variable(VariableTypes.SystemOperator, "while", true);
                                lastF = last;
                                BlockDepth++;
                            }
                            else if (dataName == "end")
                            {
                                last = new Variable(VariableTypes.SystemOperator, "end", true);
                                if (BlockDepth <= 0)
                                {
                                    DoError(Translator.GetLanguage().endOperatorError);
                                    return Variable.Unknown;
                                }
                                
                                if (RequiredBlockDepth == BlockDepth) RequiredBlockDepth--;
                                BlockDepth--;
                                if (WaitLinePoints.Count > 0)
                                {
                                    Jump(WaitLinePoints.Pop());
                                    return Variable.Unknown;
                                }
                            }
                            else
                            {
                                bool isEqNext = line.Length-1 > i + 1 && line[i] == '=';
                                if (isEqNext) Trace.WriteLine("isEqNext=true");
                                last = GetVariableByName(dataName, false, isEqNext);
                                if (last.IsSystem && last.Name=="__CREATE_NEW_VAR__" && last.Value==null && last.VariableType == VariableTypes.Unknown)
                                {
                                    last = new Variable(VariableTypes.Unknown, dataName, "undefined", false);
                                    UpdateVariable(last);
                                }
                            }
                        }
                        
                        Trace.WriteLine($"base var: {dataName} {last.Name} {last.Value}");
                        dataName = "";
                    }
                    if (c == '.')
                    {
                        isAttrName = true;
                        dataName = "";
                    }
                    if (isMath && !isAttrName && c != '(' && (lastMath != last)  && CallDeepLevel == 0)
                    {
                        if ( (i + 1 < line.Length && line[i + 1] == '(') )
                        {
                            continue;
                        }
                        Trace.WriteLine($"MATH data: lastMath: {lastMath.Name} last: {last.Name} like: \"{lastMath.Value}{MathOperator}{last.Value}\" i: {i}");
                        if (MathOperator == "+")
                        {
                            if (last.Name == "__TEMP__" && last.IsSystem)
                            {
                                last = new Variable(VariableTypes.Number, "__TEMP_NUMBER__", 0);
                            }else 
                            lastMath = new Variable(lastMath.VariableType, "__MATH_TEMP__", lastMath.Value + last.Value);
                            //lastMath.Value += last.Value; 
                        }
                        if (MathOperator == "-")
                        {
                            if (lastMath.Name == "__TEMP__" && lastMath.IsSystem) {
                                lastMath = new Variable(VariableTypes.Number, "__TEMP_NUMBER__", 0);
                            }
                            lastMath = new Variable(lastMath.VariableType, "__MATH_TEMP__", lastMath.Value - last.Value);
                            //lastMath.Value -= last.Value;
                        }
                        if (MathOperator == "*")
                        {
                            if (lastMath.Name == "__TEMP__" && lastMath.IsSystem)
                            {
                                lastMath = new Variable(VariableTypes.Number, "__TEMP_NUMBER__", 0);
                            } else 
                            lastMath = new Variable(lastMath.VariableType, "__MATH_TEMP__", lastMath.Value * last.Value);
                            //lastMath.Value *= last.Value;
                        }
                        if (MathOperator == "/")
                        {
                            if (lastMath.Name == "__TEMP__" && lastMath.IsSystem)
                            {
                                lastMath = new Variable(VariableTypes.Number, "__TEMP_NUMBER__", 0);
                            }
                            else 
                            lastMath = new Variable(lastMath.VariableType, "__MATH_TEMP__", lastMath.Value * last.Value);
                            //lastMath.Value /= last.Value;
                        }
                        if (MathOperator == "==")
                        {
                            if (lastMath.Name == "__TEMP__" && lastMath.IsSystem)
                            {
                                lastMath = new Variable(VariableTypes.Bool, "__TEMP_BOOL__", false);
                            }
                            else
                                lastMath = new Variable(VariableTypes.Bool, "__MATH_TEMP__", lastMath.Value == last.Value);
                        }
                        if (MathOperator == ">")
                        {
                            if (lastMath.Name == "__TEMP__" && lastMath.IsSystem)
                            {
                                lastMath = new Variable(VariableTypes.Bool, "__TEMP_BOOL__", false);
                            }
                            else
                                lastMath = new Variable(VariableTypes.Bool, "__MATH_TEMP__", lastMath.Value > last.Value);
                        }
                        if (MathOperator == "<")
                        {
                            if (lastMath.Name == "__TEMP__" && lastMath.IsSystem)
                            {
                                lastMath = new Variable(VariableTypes.Bool, "__TEMP_BOOL__", false);
                            }
                            else
                                lastMath = new Variable(VariableTypes.Bool, "__MATH_TEMP__", lastMath.Value < last.Value);
                        }
                        if (MathOperator == ">=")
                        {
                            if (lastMath.Name == "__TEMP__" && lastMath.IsSystem)
                            {
                                lastMath = new Variable(VariableTypes.Bool, "__TEMP_BOOL__", false);
                            }
                            else
                                lastMath = new Variable(VariableTypes.Bool, "__MATH_TEMP__", lastMath.Value >= last.Value);
                        }
                        if (MathOperator == "<=")
                        {
                            if (lastMath.Name == "__TEMP__" && lastMath.IsSystem)
                            {
                                lastMath = new Variable(VariableTypes.Bool, "__TEMP_BOOL__", false);
                            }
                            else
                                lastMath = new Variable(VariableTypes.Bool, "__MATH_TEMP__", lastMath.Value <= last.Value);
                        }
                        if (MathOperator == "!=")
                        {
                            if (lastMath.Name == "__TEMP__" && lastMath.IsSystem)
                            {
                                lastMath = new Variable(VariableTypes.Bool, "__TEMP_BOOL__", false);
                            }
                            else
                                lastMath = new Variable(VariableTypes.Bool, "__MATH_TEMP__", lastMath.Value != last.Value);
                        }
                        if (MathOperator == "!")
                        {
                            if (lastMath.VariableType != VariableTypes.Bool || last.VariableType != VariableTypes.Bool) { DoError(Translator.GetLanguage().BoolOperatorError); return Variable.Unknown; }
                            else lastMath = new Variable(VariableTypes.Bool, "__MATH_TEMP__", !last.Value);
                        }
                        if (MathOperator == "&&")
                        {
                            if (lastMath.Name == "__TEMP__" && lastMath.IsSystem)
                            {
                                lastMath = new Variable(VariableTypes.Bool, "__TEMP_BOOL__", false);
                            }
                            else if (lastMath.VariableType != VariableTypes.Bool || last.VariableType != VariableTypes.Bool) { DoError(Translator.GetLanguage().BoolOperatorError); return Variable.Unknown; }
                            else
                                lastMath = new Variable(VariableTypes.Bool, "__MATH_TEMP__", lastMath.Value && last.Value);
                        }
                        if (MathOperator == "||")
                        {
                            if (lastMath.Name == "__TEMP__" && lastMath.IsSystem)
                            {
                                lastMath = new Variable(VariableTypes.Bool, "__TEMP_BOOL__", false);
                            }
                            else if (lastMath.VariableType != VariableTypes.Bool || last.VariableType != VariableTypes.Bool) { DoError(Translator.GetLanguage().BoolOperatorError); return Variable.Unknown; }
                            else
                                lastMath = new Variable(VariableTypes.Bool, "__MATH_TEMP__", lastMath.Value || last.Value);
                        }
                        if (MathOperator == "%")
                        {
                            if (lastMath.Name == "__TEMP__" && lastMath.IsSystem)
                            {
                                lastMath = new Variable(VariableTypes.Number, "__TEMP_NUMBER__", 0);
                            }
                            else
                                lastMath = new Variable(lastMath.VariableType, "__MATH_TEMP__", lastMath.Value % last.Value);
                        }
                        if (MathOperator == "&")
                        {
                            if (lastMath.Name == "__TEMP__" && lastMath.IsSystem)
                            {
                                lastMath = new Variable(VariableTypes.Number, "__TEMP_NUMBER__", 0);
                            }
                            else if (lastMath.VariableType != VariableTypes.Bool || last.VariableType != VariableTypes.Bool) { DoError(Translator.GetLanguage().BoolOperatorError); return Variable.Unknown; }
                            else
                                lastMath = new Variable(lastMath.VariableType, "__MATH_TEMP__", lastMath.Value & last.Value);
                        }
                        if (MathOperator == "|")
                        {
                            if (lastMath.Name == "__TEMP__" && lastMath.IsSystem)
                            {
                                lastMath = new Variable(VariableTypes.Number, "__TEMP_NUMBER__", 0);
                            }
                            else if (lastMath.VariableType != VariableTypes.Bool || last.VariableType != VariableTypes.Bool) { DoError(Translator.GetLanguage().BoolOperatorError); return Variable.Unknown; }
                            else
                                lastMath = new Variable(lastMath.VariableType, "__MATH_TEMP__", lastMath.Value | last.Value);
                        }
                        if (MathOperator == ">>")
                        {
                            Trace.WriteLine($">>: t: {lastMath.VariableType.Name}, n: {last.Value}, v: {lastMath.Value}");
                            if (last.Value != null && last.Value.GetType() != typeof(string))
                            {
                                DoError("Operator \">>\" syntax error! <value> >> <varname: string>");
                                return last;
                            }
                            UpdateVariable(new Variable(lastMath.VariableType, last.Value, lastMath.Value) { Attributes = lastMath.Attributes }) ;
                            
                            //UpdateVariable(v);
                        }
                        isMath = false;
                        last = lastMath;
                        Trace.WriteLine($"math: {last.Value}");
                    }
                    if ("+=-*/><!|&%".Contains(c))
                    {
                        isMath = true;
                        lastMath = last;
                        lastF = new Variable(VariableTypes.Unknown, "__TEMP__", null, true);
                        MathOperator = $"{c}";
                        
                        if (c == '>' && line[i + 1] == '>')
                        {
                            //Trace.WriteLine($"prepeare for >>: {lastMath.Name} {lastMath.Value}");
                            i += 1;
                            MathOperator = ">>";
                        }
                        if (c == '|' && line[i + 1] == '|')
                        {
                            i += 1;
                            MathOperator = "||";
                        }
                        if (c == '&' && line[i + 1] == '&')
                        {
                            i += 1;
                            MathOperator = "&&";
                        }
                        if (c == '=' && line[i+1]=='=')
                        {
                            i += 1;
                            MathOperator = "==";
                        }
                        if (c == '>' && line[i + 1] == '=')
                        {
                            i += 1;
                            MathOperator = ">=";
                        }
                        if (c == '<' && line[i + 1] == '=')
                        {
                            i += 1;
                            MathOperator = "<=";
                        }
                        if (c == '!' && line[i + 1] == '=')
                        {
                            i += 1;
                            MathOperator = "!=";
                        }
                        if (new string[] { "=", "+=", "-=" }.Contains(MathOperator))
                        {
                            if (MathOperator == "=")
                            {
                                if (lastMath.IsSystem)
                                {
                                    DoError(Translator.GetLanguage().YouCantModifySystemVariables);
                                    break;
                                }
                                Trace.WriteLine("Operator = found! Starting new recursion and terminating current...");
                                last = CompileLine(line, i + 1);
                                lastMath.Value = last.Value;
                                lastMath.VariableType = last.VariableType;
                                lastMath.Attributes = last.Attributes;
                                break;
                                //UpdateVariable(lastMath);
                            }
                        }
                        dataName = "";

                        //last = new Variable(VariableTypes.Operator, Name = "__TEMP_OPERATOR__", MathOperator, true);
                    }
                    if ((c == ',' || c == ')') && isList && !isString)
                    {
                        // Trace.WriteLine($"Added {last.Name} {last.Value} to list");
                        if (CallDeepLevel == 0)
                        {
                            if (last != defaulLast)
                            {
                                list.Value.Add(last);
                            }
                        }
                    }
                    if (c == '(' && !isString)
                    {
                        if (skipParams) { CallDeepLevel += 1; }
                        else
                        {
                            Trace.WriteLine($"Init new list recursion: Last: {last.Name}");
                            args = CompileLine(line, i + 1, true);
                            skipParams = true;
                            CallDeepLevel += 1;
                            lastF = last;
                        }

                        Trace.WriteLine($"Current CallDeepLevel: {CallDeepLevel} (up) Last: {last.Name}");
                    }
                    if (c == ')' && !isString)
                    {
                        if (isList && CallDeepLevel == 0)
                        {
                            Trace.WriteLine(":Return list with");
                            foreach (var v in list.Value)
                            {
                                Trace.WriteLine($"\t{v.Name} {v.Value}");
                            }
                            return list;
                        }

                        if (!skipParams && CallDeepLevel == 0) DoError("UnExceptedSymbol \")\"");
                        if (skipParams && CallDeepLevel > 0)
                        {
                            CallDeepLevel -= 1;
                            Trace.WriteLine($"Current CallDeepLevel: {CallDeepLevel} (down) Last: {last.Name}");
                        }
                        if (skipParams && CallDeepLevel == 0)
                        {
                            skipParams = false;
                            Trace.WriteLine($"calling {lastF.Name} with");
                            foreach (var v in args.Value)
                            {
                                Trace.WriteLine($"\t{v.Name} {v.Value}");
                            }
                            if (last.VariableType == VariableTypes.SystemOperator)
                            {
                                Trace.WriteLine("\tIt is a SystemOperator! Calling...");
                                if (last.Name == "if")
                                {
                                    List<Variable> _ = args.Value;

                                    if (_.Count == 1 && _[0].VariableType == VariableTypes.Bool)
                                    {
                                        if (_[0].Value == true)
                                        {
                                            Trace.WriteLine("if passed");
                                            RequiredBlockDepth += 1;
                                        }
                                        else
                                        {
                                            Trace.WriteLine("if not passed");
                                        }
                                    }
                                    else
                                    {
                                        DoError(Translator.GetLanguage().ifOperatorError);
                                    }
                                }
                                if (last.Name == "while")
                                {
                                    List<Variable> _ = args.Value;

                                    if (_.Count == 1 && _[0].VariableType == VariableTypes.Bool)
                                    {
                                        if (_[0].Value == true)
                                        {
                                            WaitLinePoints.Push(CurrentLineId-1);
                                            RequiredBlockDepth += 1;
                                        }
                                        else
                                        {
                                        }
                                    }
                                    else
                                    {
                                        DoError(Translator.GetLanguage().ifOperatorError);
                                    }
                                }
                                if (last.Name == "elif")
                                {
                                    List<Variable> _ = args.Value;
                                    if (RequiredBlockDepth >= BlockDepth)
                                    {
                                        RequiredBlockDepth--;
                                        Trace.WriteLine("elif not passed");
                                    }
                                    else
                                    {
                                        if ( _.Count == 1 && _[0].VariableType == VariableTypes.Bool)
                                        {
                                            if (_[0].Value == true)
                                            {
                                                Trace.WriteLine("if passed");
                                                RequiredBlockDepth += 1;
                                            }
                                            else
                                            {
                                                Trace.WriteLine("elif not passed");
                                            }
                                        }
                                        else
                                        {
                                            DoError(Translator.GetLanguage().ifOperatorError);
                                        }
                                    }
                                    
                                }
                            }
                            else
                            {
                                Trace.WriteLine("\tIt is not a SystemOperator! Calling...");
                                if (lastF.VariableType == VariableTypes.Unknown && lastF.Value==null && lastF.IsSystem && lastF.Name == "__TEMP__")
                                {
                                    List<Variable> _ = args.Value;
                                    if (_.Count == 1)
                                    {
                                        last = _[0];
                                    }
                                    else
                                    {
                                        last = NekoAddons.me.me.list(NekodeTask.NewUnknownVariable(), _);
                                    }
                                }
                                else
                                {
                                    last = lastF.Call(args.Value);
                                    args.Value.Clear();
                                }
                            }
                        }
                    }
                    if (c == '\"' || c == '\'')
                    {
                        if (isString && stringChar == c)
                        {
                            isString = false;
                            stringChar = '\0';
                            last = new Variable(VariableTypes.String, "__TEMP_CONST_STRING__", dataName.Replace("\\n", "\n"), true);
                            dataName = "";
                        }
                        else
                        {
                            isString = true;
                            stringChar = c;
                        }
                    }
                    dataName = "";
                }
                res += c;
            }
            Trace.WriteLine($"LOGIC LINE: {res}");
            if (isList)
            {
                return list;
            }
            if (last.IsSystem && last.VariableType == VariableTypes.Unknown &&  last.Value == null)
            {
                if (dataName.Length > 0)
                {
                    last = GetVariableByName(dataName);
                }
            }
            Trace.WriteLine($"CompileLine() Return.Value = {last.Value}");
            return last;
        }
        public void DoError(string message)
        {
            IsStopped = true;
            if (MyEngine == null) return;

            MyEngine.OutputConsole.RichWriteLine(new dynamic[] {Translator.GetLanguage().Error +": "+ Name+$":{CurrentLineId+1}", message, this });
            MyEngine.OutputConsole.MainWindow.Brain.Memory.Add(new Nekode.NekodeMemoryEvent(Nekode.NekodeMemoryEventTypes.UserProgramError, "Ooops! Your program crushed!"));
        }
        public void UpdateVariables()
        {
            
            for (int i = 0; i < variables.Count; i++)
            {
                var variable = variables[i];
                variable.Source = this;
                variable.UpdateAttrs();
            }
        }
        public static bool IsLineContainsSpec(string line)
        {
            foreach (char c in line)
            {
                if (spec.Contains(c)) return true;
            }
            return false;
        }
        public void Do(string line)
        {
            line = ClearLine(line);

            if (!CheckLine(line))
            {
                MyEngine.AddStamina(this, 1.2);
                
                return;
            }
            //MyEngine.OutputTrace.WriteLine($"{BlockDepth} {RequiredBlockDepth} {line}");
            if (BlockDepth != RequiredBlockDepth && (!line.StartsWith("end") && !line.StartsWith("else") && !line.StartsWith("while") && !line.StartsWith("if") && !line.StartsWith("elif"))) return;
            List<string> data = new List<string>(line.Split());
            if (data.Count == 0) return;

            switch (data[0])
            {
                case "@mem":
                    BasicCommands.mem(this);
                    //MyEngine.OutputTrace.WriteLine(BasicCommands.mem(this));
                    return;
                case "@nya":
                    BasicCommands.nya(this, line);
                    return;
                case "@meet":
                    //MyEngine.Stamina -= 2;
                    MyEngine.AddStamina(this, -2);
                    BasicCommands.meet(this, line);
                    return;
                case "@doin":
                    MyEngine.AddStamina(this, -2);
                    BasicCommands.doin(this, line);
                    return;
                case "@stop":
                    BasicCommands.stop(this, line);
                    return;
                case "@clear":
                    MyEngine.OutputConsole.Clear();
                    return;
                case "@help":
                    BasicCommands.help(this, line);
                    return;
                case "@info":
                    BasicCommands.info(this, line);
                    return;
                case "@skill":
                    string skillName = "";
                    try
                    {
                        skillName = line.Split()[1].Replace(":", "");
                        if (!IsVariableNameLegit(skillName))
                        {
                            DoError(Translator.GetLanguage().SkillNameError);
                            return;
                        }
                    }
                    catch
                    {
                        DoError("");
                        return;
                    }
                    isSkillScript = true;
                    tempSkillTask = new NekodeTask("");
                    tempSkillTask.MyEngine = MyEngine;
                    tempSkillTask.Name = skillName;
                                        
                    Variable SkillVar = new Variable(VariableTypes.Skill, skillName, tempSkillTask);
                    if (line.Contains(':'))
                    {
                        try
                        {
                            Trace.WriteLine($"New skill \"{skillName}\" 's arguments:");
                            foreach (string argName in line.Split(':')[1].Split())
                            {
                                string n = argName.Trim();
                                if (n.Length == 0)
                                {
                                    continue;
                                }
                                if (!IsVariableNameLegit(n))
                                {
                                    DoError(Translator.GetLanguage().VariableNameError);
                                }
                                Trace.WriteLine($"\t{n}");
                                SkillVar.Attributes.Add(new Variable(VariableTypes.SkillArgument, n, n));
                            }
                        }
                        catch 
                        {
                            
                            DoError(Translator.GetLanguage().SkillSyntaxError);
                            return;
                        }
                    }
                    tempSkillVariable = SkillVar;
                    //UpdateVariable(SkillVar);
                    return;
                case "@return":
                    if (!isSkillScript)
                    {
                        DoError(Translator.GetLanguage().ReturnInvokeError);
                    }
                    string returnVariableName = "";
                    try
                    {
                        returnVariableName = line.Split()[1];
                        if (!IsVariableNameLegit(returnVariableName))
                        {
                            DoError(Translator.GetLanguage().VariableNameError);
                            return;
                        }
                    }
                    catch
                    {
                        DoError($"{Translator.GetLanguage().SyntaxError} @return <return variable name>");
                    }
                    isSkillScript = false;
                    tempSkillVariable.Attributes.Add(new Variable(VariableTypes.SkillReturnVariableName, "__RETURN_VARIABLE_NAME__", returnVariableName, true));
                    UpdateVariable(tempSkillVariable);
                    return;
                case "@var":
                    try
                    {
                        string name = ClearLine(data[1].Replace("\n", ""));
                        if (IsLineContainsSpec(name))
                        {
                            DoError(Translator.GetLanguage().VariableNameError);
                        }
                        Trace.WriteLine($"var: {name}");
                        Variable v = new Variable(VariableTypes.String, name, name);
                        UpdateVariable(v);
                    }
                    catch 
                    {
                        DoError($"{Translator.GetLanguage().SyntaxError} var <name>");
                    }
                    return;
                default: break;
            }
            if (data[0].Length > 0 && data[0][0] == '@') 
            {
                DoError(Translator.GetLanguage().CommandNotFound.Replace("{}", data[0])); 
                return; 
            }
            Trace.WriteLine(">>> =---------------= Compiling line: "+line);

            CompileLine(line);
            
            Trace.WriteLine("<<< =---------------= Compiling end");
        }
        public void Next()
        {
            UpdateVariables();
            if (Script.Count == CurrentLineId)
            {
                IsFinished = true;
                return;
            }
            
            string line = Script[CurrentLineId];
            bool skip = false;
            if (isSkillScript)
            {
                bool add = true;
                string f = ClearLine(line).Split().Length > 0 ? ClearLine(line).Split()[0] : "";
                if (f == "@return" && SkillCodeDepth > 0)
                {

                    SkillCodeDepth -= 1;
                    Trace.WriteLine($"@SKILL DEPTH: {SkillCodeDepth} (DOWN)");
                    skip = true;
                    add = true;
                }
                if (f == "@return" && SkillCodeDepth == 0 && !skip)
                {
                    Trace.WriteLine($"@SKILL DEPTH: {SkillCodeDepth} (END)");
                    skip = false;
                    add = false;
                }
                if (add) 
                {
                    if (f == "@skill")
                    {
                        
                        SkillCodeDepth += 1;
                        Trace.WriteLine($"@SKILL DEPTH: {SkillCodeDepth} (UP)");
                    }
                    Trace.WriteLine($"@SKILL FROM {Name} @SKILL add to {tempSkillTask.Name} ({SkillCodeDepth}): {line}");
                    tempSkillTask.Script.Add(line);
                    skip = true;
                }
            }
            if (!skip)
            {
                char f = (ClearLine(line).Length > 0) ? ClearLine(line)[0] : '#';
                if (f == '$')
                {
                    Do(line.Remove(0, 1));
                }
                else if (f == '#') { }

                else
                {
                    DoIn.Do(line);
                }
            }
            CurrentLineId++;
        }
    }
}
