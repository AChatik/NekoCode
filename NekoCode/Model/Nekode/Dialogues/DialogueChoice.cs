using NekoCode.Nekode.Dialogues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NekoCode.Model.Nekode.Dialogues
{
    public class DialogueChoice
    {
        public string ButtonText = "Choice 1";
        public EventHandler OnSelect;
        public string CallbackData = "";
        public DialogueChoice(string buttonText, EventHandler OnSelect, string callbackData = "")
        {
            ButtonText = buttonText;
            this.OnSelect = OnSelect;
            CallbackData = callbackData;
         }
        public void Select()
        {
            OnSelect?.Invoke(this, new EventArgs());
        }
    }
}
