namespace NekoCode.Nekode
{
    public enum NekodeMemoryEventTypes
    {
        Info = 0,
        Error = 1,
        Warn = 2,
        UserMessage = 3,
        ProgramExit = 4,
        ProgramSave = 5,
        MemorySave = 6,
        MemoryLoad = 7,
        MemoryEndpoint = 8,
        MemoryEventLoadError = 9,
        MemoryCreated = 10,
        MemoryStartPoint = 11,
        UserProgramError = 12,
        UserProgramExecution = 13,
        TellAboutMemorySize = 14,
        FirstDialogue = 15,
    }
}