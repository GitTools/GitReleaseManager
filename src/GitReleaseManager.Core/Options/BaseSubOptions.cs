using System;
using CommandLine;

namespace GitReleaseManager.Core.Options
{
    public abstract class BaseSubOptions
    {
        protected BaseSubOptions()
        {
            var ciEnvironmentVariable = Environment.GetEnvironmentVariable("CI");

            bool isCiSystem;
            if (!string.IsNullOrEmpty(ciEnvironmentVariable) && bool.TryParse(ciEnvironmentVariable, out isCiSystem))
            {
                IsCISystem = isCiSystem;
            }
        }

        [Option("debug", HelpText = "Enable debugging console output")]
        public bool Debug { get; set; }

        [Option('l', "logFilePath", HelpText = "Path to where log file should be created. Defaults to logging to console.", Required = false)]
        public string LogFilePath { get; set; }

        [Option("no-logo", HelpText = "Disables the generation of the GRM commandline logo.", Required = false)]
        public bool NoLogo { get; set; }

        [Option('d', "targetDirectory", HelpText = "The directory on which GitReleaseManager should be executed. Defaults to current directory.", Required = false)]
        public string TargetDirectory { get; set; }

        [Option("verbose", HelpText = "Enable verbose console output")]
        public bool Verbose { get; set; }

        [Option("ci", HelpText = "Configure GitReleaseManager to be compatible with CI systems")]
        public bool IsCISystem { get; set; }
    }
}