using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoCode.Nekode.Dialogues
{
    public class NekodeDialogueAction
    {
        public string ActionCommand;
        public bool AutoNext = false;
        public bool IsSpecial = false;
        public NekodeDialogueSpecialActionTypes SpecialType;
        public NekodeDialogueAction(string command, bool autoNext=false, bool isSpecial = false, NekodeDialogueSpecialActionTypes specialType = NekodeDialogueSpecialActionTypes.Choice) 
        {

            ActionCommand = command;
            AutoNext = autoNext;
            IsSpecial = isSpecial;
            SpecialType = specialType;
        }
    }
}
