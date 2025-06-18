using Korn.Logger.Core;

static class LoggerHandleExtensions
{
    public static bool IsValid(this LoggerHandle loggerHandle) => loggerHandle.IsValid && FileHolderCollection.Instance.Holders.Count > loggerHandle.Handle;
}