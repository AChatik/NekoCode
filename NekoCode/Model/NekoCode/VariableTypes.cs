using System;
namespace NekoCode
{
    public static class VariableTypes
    {
        public static VariableType Unknown = new VariableType("Unknown");
        public static VariableType String = new VariableType("String", "__to_string__");
        public static VariableType Number = new VariableType("Number", "__to_number__");
        public static VariableType Bool = new VariableType("Bool", "__to_bool__");
        public static VariableType List = new VariableType("List", "__to_list__");
        public static VariableType SystemOperator = new VariableType("SystemOperator");
        public static VariableType Operator = new VariableType("Operator");
        public static VariableType Skill = new VariableType("Skill"); //function or class
        public static VariableType SkillArgument = new VariableType("SkillArgument"); //function or class argument
        public static VariableType SkillReturnVariableName = new VariableType("SkillReturnVariableName"); //function or class argument
        public static VariableType SystemSkill = new VariableType("SystemSkill"); //C# function
        public static VariableType Task = new VariableType("Task"); //lib
        public static VariableType __INPUT__ = new VariableType("__INPUT__"); //lib
    }
}