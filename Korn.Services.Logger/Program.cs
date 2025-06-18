using Korn.Logger.Core.Client;
using Korn.Logger.Core.Server;
using System.Diagnostics;
using Korn.Logger.Core;
using Korn.Service;

var files = FileHolderCollection.Instance;
var logFile = files.GetOrCreateHolder(Korn.Interface.LoggerService.LogFile);
var crasher = new CrashWatcher();

var server = new Server(LoggerServerConfiguration.Instance);
server
.Register<CreateLoggerPacket>((connection, packet) =>
{
    var loggerPath = packet.LoggerPath;
    var fileHolder = files.GetOrCreateHolder(loggerPath);
    var fileId = fileHolder.ID;
    var loggerHandle = LoggerHandle.FromID(fileId);

    FileLogger.WriteLine(logFile, $"{DateTime.Now:yy/MM/dd HH:mm:ss.fff} Requested file handle({loggerHandle.Handle}) for \"{loggerPath}\"");
    
    var callback = new CreateLoggerCallbackPacket(loggerHandle);
    connection.Callback(packet, callback);
})
.Register<WriteMessagePacket>(packet =>
{
    var loggerHandle = packet.LoggerHandle;
    if (!loggerHandle.IsValid())
        return;

    var fileHolder = files[loggerHandle.Handle];
    var message = packet.Message;
    FileLogger.Write(fileHolder, message);
})
.Register<ClearLoggerPacket>(packet =>
{
    var loggerHandle = packet.LoggerHandle;
    if (!loggerHandle.IsValid())
            return;

    var fileHolder = files[loggerHandle.Handle];
    FileLogger.Clear(fileHolder);    
})
.Register<WatchProcessPacket>(packet =>
{
    var loggerHandle = packet.LoggerHandle;
    if (!loggerHandle.IsValid())
        return;

    var fileHolder = files[loggerHandle.Handle];

    Process? process = null;
    try
    {
        process = Process.GetProcessById(packet.ProcessID);
    } catch { return; }

    crasher.StartWatchProcess(packet.ProcessID, process, fileHolder);
    FileLogger.WriteLine(logFile, $"{DateTime.Now:yy/MM/dd HH:mm:ss.fff} Requested crash watching for \"{process.ProcessName}\"({process.Id})");
});

Thread.Sleep(int.MaxValue);