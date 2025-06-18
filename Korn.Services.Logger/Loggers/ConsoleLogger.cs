static class ConsoleLogger
{
    static bool hasConsole = Console.Out != TextWriter.Null;

    public static void Write(string text)
    {
        if (hasConsole)
            Console.Write(text);
    }
}