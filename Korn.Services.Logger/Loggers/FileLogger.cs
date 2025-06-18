using System.Text;

static class FileLogger
{
    public static void Write(FileHolder file, string text)
    {
        var buffer = Encoding.Unicode.GetBytes(text);
        var stream = file.Stream;
        stream.Write(buffer, 0, buffer.Length);
        stream.Flush();

        ConsoleLogger.Write(text);
    }

    public static void WriteLine(FileHolder file, string line) => Write(file, line + '\n');

    public static void Clear(FileHolder file) => file.Stream.SetLength(0);
}