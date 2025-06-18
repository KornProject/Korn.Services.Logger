using System.Diagnostics.CodeAnalysis;

class FileHolder
{
    public FileHolder(string filePath, int id)
        => (ID, FilePath, FileName, ShortFilePath) = (id, filePath, Path.GetFileName(filePath), $"{Path.GetFileName(Path.GetDirectoryName(filePath))}\\{Path.GetFileNameWithoutExtension(filePath)}");

    public readonly int ID;
    public readonly string FilePath, FileName, ShortFilePath;
    [AllowNull] public FileStream Stream { get; private set; }

    public bool IsHold => Stream != null && !Stream.SafeFileHandle.IsInvalid && !Stream.SafeFileHandle.IsClosed;

    public FileHolder EnsureHold()
    {
        if (!IsHold)
            Hold();

        return this;
    }

    public FileHolder Hold()
    {
        var path = FilePath;
        var directory = Path.GetDirectoryName(path);
        if (directory != null && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);
        Stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

        return this;
    }
}