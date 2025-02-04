using NekoCode.Nekode.Dialogues;
using NekoCode.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NekoCode.Nekode
{
    public class NekodeBrain
    {
        public NekodeMemory Memory;
        MainWindow Source = Application.Current.MainWindow as MainWindow;
        public Stack<NekodeBrainTask> MyTaskList = new Stack<NekodeBrainTask>();
        DateTime LastTaskProcessTime = DateTime.MinValue;
        public double DoTasksDelay = 3;
        bool canShowStatistic = true;
        bool canTalkAboutUsserProgramErrors = true;
        public List<NekodeBrainTask> AvailableRandomTasks = new List<NekodeBrainTask>() {
            NekodeBrainTask.AnalyseMemory,
            NekodeBrainTask.ClearMemoryTrash,
            
            //NekodeBrainTask,
        };
        public NekodeBrain(NekodeMemory Memory) 
        {
            this.Memory = Memory;
            this.Memory.Load();
            init();
        }
        public NekodeBrain(Uri MemoryFileName)
        {
            Memory = new NekodeMemory(MemoryFileName);
            Memory.Load();
            init();
        }
        public NekodeBrain()
        {
            Memory = new NekodeMemory(new Uri(Properties.Settings.Default.NekodeMemoryFileName, UriKind.RelativeOrAbsolute));
            Memory.Load();
            init();
        }
        public void AddTask(NekodeBrainTask t)
        {
            MyTaskList.Push(t);
        }
        public NekodeMemoryStatistic DoMemoryAnalyse()
        {
            NekodeMemoryStatistic stat = new NekodeMemoryStatistic();
            foreach (NekodeMemoryEvent e in Memory.Events)
            {
                if (e.EventType != NekodeMemoryEventTypes.UserProgramError)
                {
                    stat.Add(e.EventType);
                }
                if (e.EventType == NekodeMemoryEventTypes.UserProgramError)
                {
                    if (Memory.Events.IndexOf(e) >= Memory.Events.Count-50)
                    {
                        stat.Add(e.EventType);
                    }
                }
                if (e.EventType == NekodeMemoryEventTypes.Info && e.EventData == "Crushes registered")
                {
                    stat.Remove(NekodeMemoryEventTypes.UserProgramError);
                }
                if (e.EventType == NekodeMemoryEventTypes.Info && e.EventData == "Corrupted events registered")
                {
                    stat.Remove(NekodeMemoryEventTypes.MemoryEventLoadError);
                }
            }
            if (stat.Get(NekodeMemoryEventTypes.UserProgramError) > 0)
            {
                Memory.Add(new NekodeMemoryEvent(NekodeMemoryEventTypes.Info, "Crushes registered"));
            }
            return stat;
        }
        public NekodeMemoryStatistic GetStatistic()
        {
            NekodeMemoryStatistic stat = new NekodeMemoryStatistic();
            foreach (NekodeMemoryEvent e in Memory.Events) stat.Add(e.EventType); 
            return stat;
        }
        public void Update()
        {
            if (Memory.NewEventsCount >= 25)
            {
                AddTask(NekodeBrainTask.AnalyseMemory);
                Memory.NewEventsCount = 0;
            }
            if (MyTaskList.Count > 0 && DateTime.Now.Subtract(LastTaskProcessTime).TotalSeconds >= DoTasksDelay)
            {
                LastTaskProcessTime = DateTime.Now;
                switch (MyTaskList.Pop())
                {
                    case NekodeBrainTask.ClearMemoryTrash:
                        int deletedTrash = Memory.ClearMemoryTrash();
                        if (deletedTrash == 0)
                        {
                            Source.AddDialogue(NekodeDialogue.FromTextScenrio(Translator.GetLanguage().DeleteMemoryTrashDialogue_ResultIsUseless));
                        }
                        else
                        {
                            Source.AddDialogue(NekodeDialogue.FromTextScenrio(Translator.GetLanguage().DeleteMemoryTrashDialogue.Replace("{0}", $"{deletedTrash}")));
                        }
                        break;
                    case NekodeBrainTask.AskAboutMemorySize:
                        Source.AddDialogue(NekodeDialogue.FromTextScenrio(Translator.GetLanguage().AskAboutMemorySize_Dialogue.Replace("{0}",$"{Memory.LoadedEventsCount-Properties.Settings.Default.RegisteredNekodeMemorySize}")));
                        break;
                    case NekodeBrainTask.AnalyseMemory:
                        NekodeMemoryStatistic stat = DoMemoryAnalyse();
                        dynamic l = Translator.GetLanguage();

                        NekodeDialogue dialogue = NekodeDialogue.FromTextScenrio(l.MemoryAnalyse_NothingInteresting);
                        dialogue.ShowTime = new Duration(TimeSpan.FromSeconds(1));
                        NekodeDialogue defaultDialogue = dialogue;
                        int d_id = 0;

                        if (stat.Get(NekodeMemoryEventTypes.MemoryEventLoadError) > 0)
                        {
                            dialogue = NekodeDialogue.FromTextScenrio(l.MemoryAnalyse_CorruptedEventsRegistered.Replace("{0}",$"{stat.Get(NekodeMemoryEventTypes.MemoryEventLoadError)}"));
                            d_id = 1;
                        }

                        if (d_id == 0)
                        {
                            if (Memory.Events.Count >= 200 && stat.Get(NekodeMemoryEventTypes.TellAboutMemorySize) == 0)
                            {
                                dialogue = NekodeDialogue.FromTextScenrio(l.Over200MemoryEvents_Dialogue);
                                Memory.Add(new NekodeMemoryEvent(NekodeMemoryEventTypes.TellAboutMemorySize, "Over 200 events there!"));
                            }
                            else if (Memory.Events.Count >= 350 && stat.Get(NekodeMemoryEventTypes.TellAboutMemorySize) == 1)
                            {
                                dialogue = NekodeDialogue.FromTextScenrio(l.Over350MemoryEvents_Dialogue);
                                Memory.Add(new NekodeMemoryEvent(NekodeMemoryEventTypes.TellAboutMemorySize, "Over 350 events there!"));
                            }
                            else if (Memory.Events.Count >= 500 && stat.Get(NekodeMemoryEventTypes.TellAboutMemorySize) == 2)
                            {
                                dialogue = NekodeDialogue.FromTextScenrio(l.Over500MemoryEvents_Dialogue);
                                Memory.Add(new NekodeMemoryEvent(NekodeMemoryEventTypes.TellAboutMemorySize, "Over 500 events there!"));
                                Console.WriteLine(stat.Get(NekodeMemoryEventTypes.TellAboutMemorySize));
                                AddTask(NekodeBrainTask.ClearMemoryTrash);
                            }
                            else if (Memory.Events.Count >= 700 && stat.Get(NekodeMemoryEventTypes.TellAboutMemorySize) == 3)
                            {
                                dialogue = NekodeDialogue.FromTextScenrio(l.Over700MemoryEvents_Dialogue);
                                Memory.Add(new NekodeMemoryEvent(NekodeMemoryEventTypes.TellAboutMemorySize, "Over 700 events there!"));
                                AddTask(NekodeBrainTask.ClearMemoryTrash);
                            }
                            else if (Memory.Events.Count >= 1000 && stat.Get(NekodeMemoryEventTypes.TellAboutMemorySize) == 4)
                            {
                                dialogue = NekodeDialogue.FromTextScenrio(l.Over1000MemoryEvents_Dialogue);
                                Memory.Add(new NekodeMemoryEvent(NekodeMemoryEventTypes.TellAboutMemorySize, "Over 1000 events there!"));
                                AddTask(NekodeBrainTask.ClearMemoryTrash);
                            }
                            else if (Memory.Events.Count >= 2000 && stat.Get(NekodeMemoryEventTypes.TellAboutMemorySize) == 5)
                            {
                                dialogue = NekodeDialogue.FromTextScenrio(l.Over2000MemoryEvents_Dialogue);
                                Memory.Add(new NekodeMemoryEvent(NekodeMemoryEventTypes.TellAboutMemorySize, "Over 2000 events there!"));
                                AddTask(NekodeBrainTask.ClearMemoryTrash);
                            }

                            else if (canTalkAboutUsserProgramErrors && stat.Get(NekodeMemoryEventTypes.UserProgramError) >= 15)
                            {
                                dialogue = NekodeDialogue.FromTextScenrio(l.MemoryAnalyse_TooMushErrors_Dialogue);
                                canTalkAboutUsserProgramErrors = false;
                            }
                            else if (new Random().NextDouble() <= 0.2 && canShowStatistic)
                            {
                                stat = GetStatistic();
                                canShowStatistic = false;
                                dialogue = NekodeDialogue.FromTextScenrio(l.MemoryAnalyse_Statistic.Replace("{0}", $"{Memory.Events.Count}")
                                    .Replace("{1}", $"{stat.GetBadSystemEvents()}")
                                    .Replace("{2}", $"{stat.Get(NekodeMemoryEventTypes.MemoryEventLoadError)}")
                                    .Replace("{3}", $"{0}")
                                    .Replace("{4}", $"{0}")
                                    .Replace("{5}", $"{stat.Get(NekodeMemoryEventTypes.UserProgramExecution)}")
                                    .Replace("{6}", $"{stat.Get(NekodeMemoryEventTypes.UserProgramError)}"));
                            }
                            
                        }
                        if (d_id == 1)
                        {
                            Memory.Add(new NekodeMemoryEvent(NekodeMemoryEventTypes.Info, "Corrupted events registered"));
                        }
                        if (defaultDialogue == dialogue )
                        {
                           Source.AddDialogue(dialogue);
                            
                        }
                        else Source.AddDialogue(dialogue);
                        break;
                }
            }
        }
        void init()
        {
            if (!Memory.ContainsEventType(NekodeMemoryEventTypes.FirstDialogue) && !Properties.Settings.Default.IsFirstRun)
            {

                Nekode.Dialogues.NekodeDialogue d = Nekode.Dialogues.Dialogues.FirstRun.Copy();
                d.Completed += delegate (object sender, EventArgs e)
                {
                    Memory.Add(new NekodeMemoryEvent(NekodeMemoryEventTypes.FirstDialogue, "Our first dialogue, nice to meet you!"));
                    Memory.Save();
                };
                Source.AddDialogue(d);
            }
            else
            {
                AddTask(NekodeBrainTask.AnalyseMemory);
            }
        }
    }
}
