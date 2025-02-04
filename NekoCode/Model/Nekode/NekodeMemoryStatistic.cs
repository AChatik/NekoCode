using System.Collections.Generic;
using System.IO;

namespace NekoCode.Nekode
{
    public class NekodeMemoryStatistic
    {
        public Dictionary<NekodeMemoryEventTypes, int> TotalMemoryEventTypes = new Dictionary<NekodeMemoryEventTypes, int>();
        public void Add(NekodeMemoryEventTypes t, int c = 1)
        {
            if (TotalMemoryEventTypes.ContainsKey(t))
            {
                TotalMemoryEventTypes[t] += c;
            }
            else
            {
                TotalMemoryEventTypes.Add(t, c);
            }
            
        }
        public void Remove(NekodeMemoryEventTypes t)
        {
            TotalMemoryEventTypes.Remove(t);
        }
        public int Get(NekodeMemoryEventTypes t)
        {
            int r = 0;
            if (TotalMemoryEventTypes.ContainsKey(t)) r = TotalMemoryEventTypes[t];
            return r;
        }
        public int GetBadSystemEvents()
        {
            int errors = 0;
            int ele = 0;
            TotalMemoryEventTypes.TryGetValue(NekodeMemoryEventTypes.Error, out errors);
            TotalMemoryEventTypes.TryGetValue(NekodeMemoryEventTypes.MemoryEventLoadError, out ele);
            return errors + ele;
        }
        public int GetTrashEvents()
        {
            int l = 0;
            int s = 0;
            TotalMemoryEventTypes.TryGetValue(NekodeMemoryEventTypes.MemoryLoad, out l);
            TotalMemoryEventTypes.TryGetValue(NekodeMemoryEventTypes.MemorySave, out s);
            return l + s;
        }
    }
}