using NekoCode.Model.Nekode.Dialogues;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Windows;


namespace NekoCode.Nekode.Dialogues
{
    public class NekodeDialogue
    {
        public List<NekodeDialogueAction> Script = new List<NekodeDialogueAction>();
        int i = 0;
        public bool IsEnd = false;
        public string SkipToPoint = null;
        public string CurrentPoint = null;
        public EventHandler Completed = null;
        public NekodeDialogueAction CurrentAction = null;
        public Duration? ShowTime = null;
        public void Next()
        {
            if (IsEnd) return;
            if (i >= Script.Count) {
                IsEnd=true;
                Completed?.Invoke(this, EventArgs.Empty);
                return; 
            }
            NekodeDialogueAction action = Script[i];
            CurrentAction = action;
            //Console.WriteLine($"Action:\n\tcommand: {action.ActionCommand}\n\tIsSpecial {action.IsSpecial}\n\tCurrentPoint = {CurrentPoint}\n\tSkipToPoint = {SkipToPoint}");

            i++;
            if (i == Script.Count&&action.AutoNext) IsEnd = true;
            if (action.IsSpecial && action.SpecialType == NekodeDialogueSpecialActionTypes.Mida)
            {
                (Application.Current.MainWindow as MainWindow).WriteLineToDialogueCodeExample(action.ActionCommand);
            }
            if (action.IsSpecial && action.SpecialType == NekodeDialogueSpecialActionTypes.Choice)
            {
                string[] choices = action.ActionCommand.Split('|');
                foreach (string ButtonText in choices)
                {

                    string t = ButtonText.Trim();
                    Console.WriteLine($"Init choice \"{t}\"");
                    DialogueChoice ch = new DialogueChoice(t, delegate (object sender, EventArgs e)
                    {
                        this.SkipToPoint = t;
                        Console.WriteLine($"Selected {t}");
                    });
                    (Application.Current.MainWindow as MainWindow).AddChoiceToDialogue(ch);
                }
            }
            if (SkipToPoint != null)
            {
                if (action.IsSpecial && action.SpecialType == NekodeDialogueSpecialActionTypes.Point)
                {
                    CurrentPoint = action.ActionCommand;
                }
                else if (action.IsSpecial && action.SpecialType == NekodeDialogueSpecialActionTypes.ChoiceEnd)
                {
                    SkipToPoint = null;
                    CurrentPoint = null;
                }
                else
                {
                }
            }
            
            if (SkipToPoint != CurrentPoint && CurrentPoint != null) 
            {
                Next();
                return;
            }
            if (!action.IsSpecial) DoAction(action);
            if (action.AutoNext) Next();
            
            

        }
        public static void DoAction(NekodeDialogueAction action)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.DialogueEngine.MainTask.Do(action.ActionCommand);
        }
        public void Add(NekodeDialogueAction action)
        {
            Script.Add(action);
        }
        public NekodeDialogue Copy()
        {
            return new NekodeDialogue() { Script = Script };
        }
        public static NekodeDialogue FromTextScenrio(string scenario, string putIn = "me.say(\"{line}\")")
        {
            NekodeDialogue d = new NekodeDialogue();
            foreach (string line in scenario.Split('\n'))
            {
                bool skip = false;
                bool IsSpecial = false;
                string l = line;
                NekodeDialogueSpecialActionTypes SpecialType = NekodeDialogueSpecialActionTypes.Choice;
                if (line.StartsWith("</choice/>"))
                {
                    l = l.Remove(0, 10);
                    skip = false;
                    IsSpecial = true;
                    SpecialType = NekodeDialogueSpecialActionTypes.Choice;
                }
                else if (line.StartsWith("</point/>"))
                {
                    l = l.Remove(0, 9);
                    skip = true;
                    IsSpecial = true;
                    SpecialType = NekodeDialogueSpecialActionTypes.Point;
                }
                else if (line.StartsWith("</choice_end/>"))
                {
                    l = "</choice_end/>";
                    skip = true;
                    IsSpecial = true;
                    SpecialType = NekodeDialogueSpecialActionTypes.ChoiceEnd;
                }
                else if (line.StartsWith("</Mida/>"))
                {
                    l = l.Remove(0, "</Mida/>".Length);
                    IsSpecial = true;
                    SpecialType = NekodeDialogueSpecialActionTypes.Mida;
                }
                if (l.EndsWith("</skip/>"))
                {
                    l = l.Remove(l.Length - 8);
                    skip = true;
                }


                NekodeDialogueAction a = new NekodeDialogueAction(l.StartsWith("\\") ? l.Remove(0, 1) : putIn.Replace("{line}", l));
                if (IsSpecial)
                {
                    a = new NekodeDialogueAction(l,skip,IsSpecial, SpecialType );
                }
                a.AutoNext = skip;
                d.Add(a);
                
            }
            return d;
        }
    }
}
