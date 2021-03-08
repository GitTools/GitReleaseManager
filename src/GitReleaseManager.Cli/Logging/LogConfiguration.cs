namespace GitReleaseManager.Cli.Logging
{
    using System.Diagnostics;
    using System.Text;
    using Destructurama;
    using GitReleaseManager.Core.Options;
    using Octokit;
    using Serilog;
    using Serilog.Events;
    using Serilog.Sinks.SystemConsole.Themes;

    public static class LogConfiguration
    {
        private const string CONSOLE_FULL_TEMPLATE = "[{Level:u3}] " + CONSOLE_INFO_TEMPLATE;
        private const string CONSOLE_INFO_TEMPLATE = "{Message:l}{NewLine}{Exception}";
        private const string FILE_TEMPLATE = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
        private static readonly ConsoleTheme _consoleTheme = AnsiConsoleTheme.Code;

        public static void ConfigureLogging(BaseSubOptions options)
        {
            var config = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Destructure.UsingAttributes()
                .Destructure.ByTransforming<ReleaseAssetUpload>(asset => new { asset.FileName, asset.ContentType });

            CreateDebugLogger(config);
            CreateConsoleInformationLogger(config, CONSOLE_INFO_TEMPLATE, _consoleTheme);
            CreateConsoleFullLogger(config, CONSOLE_FULL_TEMPLATE, _consoleTheme, options);

            if (!string.IsNullOrEmpty(options.LogFilePath))
            {
                CreateFileLogger(config, options.LogFilePath, FILE_TEMPLATE);
            }

            Log.Logger = config.CreateLogger();
        }

        private static void CreateConsoleFullLogger(LoggerConfiguration config, string consoleTemplate, ConsoleTheme consoleTheme, BaseSubOptions options)
        {
            config.WriteTo.Logger((config) => config
                .Filter.ByExcluding((logEvent) => logEvent.Level == LogEventLevel.Information)
                .Filter.ByExcluding((logEvent) => !options.Debug && logEvent.Level == LogEventLevel.Debug)
                .Filter.ByExcluding((logEvent) => !options.Verbose && logEvent.Level == LogEventLevel.Verbose)
                .WriteTo.Console(
                    outputTemplate: consoleTemplate,
                    standardErrorFromLevel: LogEventLevel.Warning,
                    theme: consoleTheme));
        }

        private static void CreateConsoleInformationLogger(LoggerConfiguration config, string consoleTemplate, ConsoleTheme consoleTheme)
        {
            config.WriteTo.Logger((config) => config
                .Filter.ByIncludingOnly((logEvent) => logEvent.Level == LogEventLevel.Information)
                .WriteTo.Console(
                    outputTemplate: consoleTemplate,
                    theme: consoleTheme));
        }

        private static void CreateFileLogger(LoggerConfiguration config, string logFilePath, string logTemplate)
        {
            config.WriteTo.File(logFilePath, outputTemplate: logTemplate, encoding: new UTF8Encoding(false));
        }

        [Conditional("DEBUG")]
        private static void CreateDebugLogger(LoggerConfiguration config)
        {
            config.WriteTo.Debug();
        }
    }
}