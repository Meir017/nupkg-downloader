// See https://aka.ms/new-console-template for more information
using NuGet.Common;

public class ConsoleLogger : LoggerBase
{
    public override void Log(ILogMessage message) => LogMessage(message);

    public override Task LogAsync(ILogMessage message)
    {
        LogMessage(message);
        return Task.CompletedTask;
    }

    private static void LogMessage(ILogMessage message)
    {
        Console.WriteLine($"[{DateTime.UtcNow}] {message.Level}: {message.FormatWithCode()}");
    }
}