public class BuildData
{
    public string GitHubToken { get; }
    public string GitHubOwner { get; }
    public string GitHubRepository { get; }
    public string GrmMilestone { get; }

    public BuildData(ICakeContext context)
    {
        if (context == null)
        {
            throw new ArgumentException(nameof(context));
        }

        GitHubToken = context.EnvironmentVariable("GITTOOLS_GITHUB_TOKEN");
        GitHubOwner = "gep13";
        GitHubRepository = "FakeRepository";
        GrmMilestone = "v0.1.2";
    }
}