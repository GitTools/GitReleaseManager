namespace GitReleaseManager.Core.Options
{
    public class MainOptions
    {
        public CreateSubOptions CreateVerb { get; set; }

        public AddAssetSubOptions AddAssetVerb { get; set; }

        public CloseSubOptions CloseVerb { get; set; }

        public PublishSubOptions PublishVerb { get; set; }

        public ExportSubOptions ExportVerb { get; set; }

        public InitSubOptions InitVerb { get; set; }

        public ShowConfigSubOptions ShowConfigVerb { get; set; }

        public LabelSubOptions LabelVerb { get; set; }

        ////[HelpVerbOption]
        ////public string DoHelpForVerb(string verbName)
        ////{
        ////    return HelpText.AutoBuild(this, verbName);
        ////}
    }
}