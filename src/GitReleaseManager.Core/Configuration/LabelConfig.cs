using YamlDotNet.Serialization;

namespace GitReleaseManager.Core.Configuration
{
    public class LabelConfig
    {
        [YamlMember(Alias = "name")] public string Name { get; set; }

        [YamlMember(Alias = "description")] public string Description { get; set; }

        [YamlMember(Alias = "color")] public string Color { get; set; }

        [YamlMember(Alias = "renameFrom")] public string RenameFrom { get; set; }
    }
}