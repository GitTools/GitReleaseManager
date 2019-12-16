//-----------------------------------------------------------------------
// <copyright file="ClientBuilder.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.IntegrationTests
{
    using Octokit;
    using Octokit.Internal;

    public static class ClientBuilder
    {
        private static HttpClientAdapter _httpClient;

        public static GitHubClient Build()
        {
            var credentialStore = new InMemoryCredentialStore(Helper.Credentials);

#pragma warning disable DF0024 // Marks undisposed objects assinged to a field, originated in an object creation.
            _httpClient = new HttpClientAdapter(HttpMessageHandlerFactory.CreateDefault);
#pragma warning restore DF0024 // Marks undisposed objects assinged to a field, originated in an object creation.

            var connection = new Connection(
                new ProductHeaderValue("GitReleaseManager"),
                GitHubClient.GitHubApiUrl,
                credentialStore,
                _httpClient,
                new SimpleJsonSerializer());

            return new GitHubClient(connection);
        }

        public static void Cleanup()
        {
            _httpClient?.Dispose();
            _httpClient = null;
        }
    }
}