using GitReleaseManager.Core;
using GitReleaseManager.Core.Configuration;
using GitReleaseManager.Core.Helpers;
using Spectre.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;

namespace GitReleaseManager.Cli
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var app = new CommandApp();

            app.Configure(config => 
            {
                config.AddCommand<CreateCommand>("create");
                config.AddCommand<AddAssetCommand>("addasset");
                config.AddCommand<InitCommand>("init");
            });

            app.Run(args);
        }
    }

    [Description("Creates a draft release notes from a milestone.")]
    public sealed class CreateCommand : Command<CreateSettings>
    {
        public override int Execute(CommandContext context, CreateSettings settings)
        {
            return 0;
        }
    }

    public sealed class CreateSettings : BaseSettings
    {
    }

    [Description("Adds an asset to an existing release.")]
    public sealed class AddAssetCommand : Command<AddAssetSettings>
    {
        public override int Execute(CommandContext context, AddAssetSettings settings)
        {
            return 0;
        }
    }

    public sealed class AddAssetSettings : BaseSettings
    {

    }

    [Description("Creates a sample Yaml Configuration file in root directory")]
    public sealed class InitCommand : BaseCommand<InitSettings>
    {
        protected override int ExecuteCommand(CommandContext context, InitSettings settings)
        {
            ConfigurationProvider.WriteSample(settings.TargetDirectory ?? Environment.CurrentDirectory, FileSystem);
            return 0;
        }
    }

    public sealed class InitSettings : BaseSettings
    {

    }

    public abstract class BaseCommand<T> : Command<T>
        where T : BaseSettings
    {
        private StringBuilder log = new StringBuilder();
        protected FileSystem FileSystem { get; } = new FileSystem();

        public sealed override int Execute(CommandContext context, T settings)
        {
            ConfigureLogging(settings.LogFilePath);

            return ExecuteCommand(context, settings);
        }

        protected abstract int ExecuteCommand(CommandContext context, T settings);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Not required.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This is required here.")]
        private void ConfigureLogging(string logFilePath)
        {
            var writeActions = new List<Action<string>>
            {
                s => log.AppendLine(s)
            };

            if (!string.IsNullOrEmpty(logFilePath))
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));
                    if (File.Exists(logFilePath))
                    {
                        using (File.CreateText(logFilePath))
                        {
                        }
                    }

                    writeActions.Add(x => WriteLogEntry(logFilePath, x));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to configure logging: " + ex.Message);
                }
            }
            else
            {
                // if nothing else is specified, write to console
                writeActions.Add(Console.WriteLine);
            }

            Logger.WriteInfo = s => writeActions.ForEach(a => a(s));
            Logger.WriteWarning = s => writeActions.ForEach(a => a(s));
            Logger.WriteError = s => writeActions.ForEach(a => a(s));
        }

        private static void WriteLogEntry(string logFilePath, string s)
        {
            var contents = string.Format(CultureInfo.InvariantCulture, "{0}\t\t{1}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), s);
            File.AppendAllText(logFilePath, contents);
        }
    }

    public abstract class BaseSettings : CommandSettings
    {
        [CommandOption("-d|--targetDirectory <PATH>")]
        public string TargetDirectory { get; set; }

        [CommandOption("-l|--logFilePath <PATH>")]
        public string LogFilePath { get; set; }

        // TODO: Move to higher level, not on every command
        [CommandOption("--version")]
        public bool Version { get; set; }
    }        
}