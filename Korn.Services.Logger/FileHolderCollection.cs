class FileHolderCollection
{
    FileHolderCollection() { }

    public static FileHolderCollection Instance = new FileHolderCollection();

    public readonly List<FileHolder> Holders = [];

    public FileHolder this[int index]
    {
        get
        {
            var holders = Holders;
            if (index >= holders.Count)
            {
                var holder = holders.FirstOrDefault();
                if (holder is null)
                    throw new Exception("There is no 0-existed holder");

                FileLogger.Write(holder, $"id pointed to a nonexisted holder. holders count: {holders.Count}, pointed id: {index}\n");
                return holder;
            }
            else return holders[index];
        }
    }

    public FileHolder GetOrCreateHolder(string path)
    {
        var existsHolderIndex = GetHolderIndex(path);
        if (existsHolderIndex == -1)
            return CreateHolder(path).EnsureHold();
        return Holders[existsHolderIndex].EnsureHold();
    }

    FileHolder CreateHolder(string path)
    {
        lock (Holders)
        {
            var id = Holders.Count;
            var holder = new FileHolder(path, id);
            Holders.Add(holder);
            return holder;
        }
    }

    int GetHolderIndex(string path)
    {
        for (var i = 0; i < Holders.Count; i++)
            if (string.Equals(path, Holders[i].FilePath, StringComparison.OrdinalIgnoreCase))
                return i;

        return -1;
    }
}