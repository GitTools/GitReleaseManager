public class BuildData
{
    public string GitHubUsername { get; }
    public string GitHubPassword { get; }
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

        GitHubUsername = context.EnvironmentVariable("GITTOOLS_GITHUB_USERNAME");
        GitHubPassword = context.EnvironmentVariable("GITTOOLS_GITHUB_PASSWORD");
        GitHubToken = context.EnvironmentVariable("GITTOOLS_GITHUB_TOKEN");
        GitHubOwner = "gep13";
        GitHubRepository = "FakeRepository";
        GrmMilestone = "v0.1.2";
    }
}