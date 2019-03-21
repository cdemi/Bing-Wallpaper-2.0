using Serilog;

namespace Bing_Wallpaper
{
    public static class AppLogger
    {
        public static ILogger Logger { get; } = new LoggerConfiguration().WriteTo.File("Logs\\log.txt",
            rollingInterval: RollingInterval.Day,
            fileSizeLimitBytes: 1000000
            ).CreateLogger();
    }
}
