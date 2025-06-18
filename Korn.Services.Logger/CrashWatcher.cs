using System.Diagnostics.Eventing.Reader;
using System.Diagnostics;

class CrashWatcher
{
    const int ApplicationError = 1000;
    const int RuntimeError = 1026;

    public CrashWatcher()
    {
        var query = new EventLogQuery("Application", PathType.LogName, $"*[System[(EventID={ApplicationError} or EventID={RuntimeError})]]");
        watcher = new EventLogWatcher(query);
        watcher.EventRecordWritten += OnEventRecord;
        watcher.Enabled = true;
    }

    ProcessCollection processes = new();
    EventLogWatcher watcher;

    void OnEventRecord(object? sender, EventRecordWrittenEventArgs args)
    {
        var eventRecord = args.EventRecord;
        if (eventRecord is null)
            return;

        OnApplicationCrash(eventRecord);
    }

    void OnApplicationCrash(EventRecord eventRecord)
    {
        var processId = GetProcessId();
        if (!processes.Contains(processId))
            return;

        var file = processes.GetAssociatedFile(processId);
        if (file is null)
            return;

        var message = GetMessage();
        FileLogger.WriteLine(file, message);

        int GetProcessId() => eventRecord.Id switch
        {
            ApplicationError => (int)eventRecord.Properties[8].Value, // creates from a different process, therefore ProcessId is an incorrect process ID source
            RuntimeError => eventRecord.ProcessId ?? 0, // stores in «Execution» sector as ProcessID and ThreadID
            _ => 0
        };

        string GetMessage() => string.Join("\n\t", ($"Application({processId}) crash:\n" + eventRecord.FormatDescription()).Split('\n'));
    }

    public void StartWatchProcess(int processId, Process process, FileHolder file) => processes.Add(new(processId, process, file));

    class ProcessCollection
    {
        Dictionary<int, ProcessEntry> Entries = [];

        public void Add(ProcessEntry processEntry)
        {
            if (processEntry is null)
                return;

            var entries = Entries;
            var processId = processEntry.ProcessID;
            if (entries.ContainsKey(processId))
                entries[processId] = processEntry;
            else Entries.Add(processId, processEntry);

            if (entries.Count > 255)
                TotalRelevanceCheck();
        }

        public bool Contains(int processId) => Entries.ContainsKey(processId);

        public FileHolder? GetAssociatedFile(int processId)
        {
            Entries.TryGetValue(processId, out ProcessEntry? processEntry);
            if (processEntry is null)
                return null;

            return processEntry.File;
        }

        public bool CheckRelevance(int processId)
        {
            if (!Contains(processId))
                return false;

            return CheckRelevanceNoKeyValidation(processId);
        }

        bool CheckRelevanceNoKeyValidation(int processId)
        {
            var processInfo = Entries[processId];
            var isRelevance = !processInfo.Process.HasExited;
            if (!isRelevance)
                Entries.Remove(processId);

            return isRelevance;
        }

        void TotalRelevanceCheck()
        {
            var processes = Entries.Values.ToList();
            for (var index = 0; index < processes.Count; index++)
                CheckRelevanceNoKeyValidation(processes[index].ProcessID);
        }
    }

    record ProcessEntry(int ProcessID, Process Process, FileHolder File);
}