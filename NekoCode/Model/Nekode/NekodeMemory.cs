using System;
using System.Collections.Generic;
using System.IO;

namespace NekoCode.Nekode
{
    public class NekodeMemory
    {
        public Uri MemoryFileName;
        public List<NekodeMemoryEvent> Events = new List<NekodeMemoryEvent>();
        public int ProtectedEventsCount = 20;
        public int NewEventsCount = 0;
        public int LoadedEventsCount = 0;
        public int SavedEventsCount = 0;
        public NekodeMemory(Uri memoryFileName)
        {
            MemoryFileName = memoryFileName;
            if (!File.Exists(memoryFileName.OriginalString))
            {
                StreamWriter sw =  File.AppendText(memoryFileName.OriginalString);
                sw.WriteLine(new NekodeMemoryEvent(NekodeMemoryEventTypes.MemoryCreated, "Hi! Our actions will be saved there ^_-"));
                sw.Close();
            }
        }
        public bool ContainsEventType(NekodeMemoryEventTypes type)
        {
            foreach (NekodeMemoryEvent e in Events)
            {
                if (e.EventType == type) return true;
            }
            return false;
        }
        public void Load(bool JustSkipErrors=false, bool addEvent = true) 
        {
            Events = new List<NekodeMemoryEvent>();
            StreamWriter sw = File.AppendText("corrupted_story.mem");
            if (System.IO.Path.GetFileNameWithoutExtension(MemoryFileName.OriginalString) == "corrupted_story") sw.Close();
            foreach (string line in File.ReadAllLines(MemoryFileName.OriginalString))
            {
                NekodeMemoryEvent e = NekodeMemoryEvent.FromString(line);
                Events.Add(e);
                LoadedEventsCount += 1;
                if (e.Corrupted && !JustSkipErrors)
                {
                    sw.WriteLine(line);
                    
                }
            }
            sw.Close();
            if (addEvent) Add(new NekodeMemoryEvent(NekodeMemoryEventTypes.MemoryLoad, @"(Loading memory) Let's remember what we've been through..."));
        }
        public void Save(bool addEvent=true) 
        {
            SavedEventsCount = 0;
            if (addEvent) Add(new NekodeMemoryEvent(NekodeMemoryEventTypes.MemorySave, "(Saving memory) Saving our wonderful story..."));
            File.Delete(MemoryFileName.OriginalString);
            StreamWriter sw = File.AppendText(MemoryFileName.OriginalString);
            foreach (NekodeMemoryEvent e in Events)
            {
                SavedEventsCount += 1;
                sw.WriteLine(e.ToString());
            }
            sw.Close();
        }
        public void Add(NekodeMemoryEvent e, bool changeInitDate = true)
        {
            if (changeInitDate) e.InitDate = DateTime.Now.ToString();
            Events.Add(e);
            NewEventsCount++;
        }

        public int ClearMemoryTrash()
        {
            int deletedEvents = 0;
            List<NekodeMemoryEvent> events = new List<NekodeMemoryEvent>();
            for (int i = 0; i < Events.Count; i++)
            {
                NekodeMemoryEvent e = Events[i];
                if (i < Events.Count - ProtectedEventsCount)
                {

                    if (e.EventType == NekodeMemoryEventTypes.MemorySave || e.EventType == NekodeMemoryEventTypes.MemoryLoad)
                    {
                        deletedEvents++;
                    }
                    else
                    {
                        events.Add(e);
                    }
                }
                else
                {
                    events.Add(e);
                }

            }
            Events.Clear();
            Events.AddRange(events);
            return deletedEvents;
        }

        public void ReviveEvent(NekodeMemoryEvent memoryEvent)
        {
            int i = 0;
            int replaceIndex = -1;
            foreach (NekodeMemoryEvent e in Events)
            {
                if (e.EventType==NekodeMemoryEventTypes.MemoryEventLoadError)
                {
                    replaceIndex = i;
                }
                i++;
            }
            if (replaceIndex >= 0 && replaceIndex < Events.Count)
            {
                Events[replaceIndex] = memoryEvent;
            }
            else
            {
                Events.Add(memoryEvent);
            }
        }
    }
}
