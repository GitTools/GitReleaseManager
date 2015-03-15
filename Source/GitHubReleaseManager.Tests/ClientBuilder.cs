//-----------------------------------------------------------------------
// <copyright file="ClientBuilder.cs" company="gep13">
//     Copyright (c) 2015 - Present gep13
// </copyright>
//-----------------------------------------------------------------------

namespace GitHubReleaseManager.Tests
{
    using Octokit;
    using Octokit.Internal;

    public static class ClientBuilder
    {
        public static GitHubClient Build()
        {
            var credentialStore = new InMemoryCredentialStore(Helper.Credentials);

            var httpClient = new HttpClientAdapter(Helper.Proxy);

            var connection = new Connection(
                new ProductHeaderValue("GitHubReleaseManager"),
                GitHubClient.GitHubApiUrl,
                credentialStore,
                httpClient,
                new SimpleJsonSerializer());

            return new GitHubClient(connection);
        }
    }
}