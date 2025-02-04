using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoCode.Nekode
{
    public class NekodeMemoryEvent
    {
        public NekodeMemoryEventTypes EventType;
        public string EventData;
        public string InitDate;
        public bool Corrupted = false; 
        public NekodeMemoryEvent(NekodeMemoryEventTypes t, string data)
        {
            EventType = t;
            EventData = data;
            InitDate = DateTime.Now.ToString();
        }
        public override string ToString()
        {
            return $"{InitDate.ToString()} | {(int)EventType} | {EventData}";
        }
        static public NekodeMemoryEvent FromString(string s) 
        {
            try
            {
                string initDate = s.Split('|')[0].Trim();
                int EventTypeID = Convert.ToInt32(s.Trim().Replace(" ","").Split('|')[1]);
                string data = s.Split(new char[] { '|' }, 3)[2].Trim();
                return new NekodeMemoryEvent((NekodeMemoryEventTypes)EventTypeID, data) { InitDate = initDate };
            }
            catch
            {
                return new NekodeMemoryEvent(NekodeMemoryEventTypes.MemoryEventLoadError, "[CORRUPTED]") { Corrupted = true }; 
            }
        }
    }
    
}
