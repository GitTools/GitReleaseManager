namespace ReleaseNotesCompiler.Tests
{
    using System;
    using System.Net;
    using Octokit;

    public static class Helper
    {
        // From https://github.com/octokit/octokit.net/blob/master/Octokit.Tests.Integration/Helper.cs

        static readonly Lazy<Credentials> _credentialsThunk = new Lazy<Credentials>(() =>
        {
            var githubUsername = Environment.GetEnvironmentVariable("OCTOKIT_GITHUBUSERNAME");

            var githubToken = Environment.GetEnvironmentVariable("OCTOKIT_OAUTHTOKEN");

            if (githubToken != null)
                return new Credentials(githubToken);

            var githubPassword = Environment.GetEnvironmentVariable("OCTOKIT_GITHUBPASSWORD");

            if (githubUsername == null || githubPassword == null)
                return Credentials.Anonymous;

            return new Credentials(githubUsername, githubPassword);
        });

        public static Credentials Credentials
        {
            get { return _credentialsThunk.Value; }
        }

        public static IWebProxy Proxy
        {
            get
            {
                return null;
/*
                return new WebProxy(
                    new System.Uri("http://myproxy:42"),
                    true,
                    new string[] {},
                    new NetworkCredential(@"domain\login", "password"));
*/
            }
        }
    }
}
