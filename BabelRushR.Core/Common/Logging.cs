using Microsoft.Extensions.Logging;

namespace BabelRushR.Core.Common;

public static class Logging
{
    private static ILoggerFactory Factory => field ??= LoggerFactory.Create(builder =>
    {
        builder.AddConsole();
    });

    public static ILogger<T> Logger<T>() => Factory.CreateLogger<T>();
    public static ILogger Logger(Type type) => Factory.CreateLogger(type); // Sadly, we don't have a CallerIdentityAttribute yet.

    public static void AddLogChannel(ILoggerProvider loggerProvider)
    {
        Factory.AddProvider(loggerProvider);
    }
}