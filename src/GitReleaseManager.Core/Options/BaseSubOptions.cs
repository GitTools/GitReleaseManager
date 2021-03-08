namespace GitReleaseManager.Core.Options
{
    using CommandLine;

    public abstract class BaseSubOptions
    {
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
    }
}
