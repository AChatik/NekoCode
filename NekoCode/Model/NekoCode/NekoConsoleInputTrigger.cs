using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoCode.Model.NekoCode
{
    public class NekoConsoleInputTrigger
    {
        public string Command;
        public event EventHandler<EventArgs> OnCommandExetuced;
        public int MaxCallCount = -1;
        public NekoConsoleInputTrigger(string command, int maxCallCount = -1)
        {
            MaxCallCount = maxCallCount;
            Command = command;
        }
        
        public void CommandExecuted(EventArgs e)
        {
            OnCommandExetuced?.Invoke(this, e);
        }
    }
}
