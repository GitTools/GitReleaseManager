namespace ReleaseNotesCompiler.Tests
{
    using System.Diagnostics;
    using System.Linq;
    using NUnit.Framework;
    using ReleaseNotesCompiler;

    [TestFixture]
    public class ReleaseManagerTests
    {
        [Test]
        [Explicit]
        public async void List_releases_that_needs_updates()
        {
            var gitHubClient = ClientBuilder.Build();

            var releaseNotesBuilder = new ReleaseManager(gitHubClient, "Particular");
            var result = await releaseNotesBuilder.GetReleasesInNeedOfUpdates();

            Debug.WriteLine("{0} releases found that needs updating", result.Count());
            foreach (var releaseName in result)
            {
                Debug.WriteLine(releaseName);
            }

        }
    }
}
