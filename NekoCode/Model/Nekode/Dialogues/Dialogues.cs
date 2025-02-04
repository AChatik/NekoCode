using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoCode.Nekode.Dialogues
{
    public static class Dialogues
    {
        static dynamic l = Translator.GetLanguage();
        public static NekodeDialogue FirstRun = NekodeDialogue.FromTextScenrio(l.FirstDialogue);

        public static List<NekodeDialogue> RandomDialogues = new List<NekodeDialogue>()
        {
            NekodeDialogue.FromTextScenrio(l.RandomDialogue1),
            NekodeDialogue.FromTextScenrio(l.RandomDialogue2),
            NekodeDialogue.FromTextScenrio(l.RandomDialogue3),
            NekodeDialogue.FromTextScenrio(l.RandomDialogue4),
            NekodeDialogue.FromTextScenrio(l.RandomDialogue5),
            NekodeDialogue.FromTextScenrio(l.RandomDialogue6),
            NekodeDialogue.FromTextScenrio(l.RandomDialogue7),
            NekodeDialogue.FromTextScenrio(l.RandomDialogue8),
            NekodeDialogue.FromTextScenrio(l.RandomDialogue9),
            NekodeDialogue.FromTextScenrio(l.RandomDialogue10),
            NekodeDialogue.FromTextScenrio(l.RandomDialogue11)
        };
    }
}
