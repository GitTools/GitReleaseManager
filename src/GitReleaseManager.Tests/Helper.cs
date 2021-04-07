using System;
using System.Net;
using Octokit;

namespace GitReleaseManager.Tests
{
    public static class Helper
    {
        // From https://github.com/octokit/octokit.net/blob/master/Octokit.Tests.Integration/Helper.cs
        private static readonly Lazy<Credentials> CredentialsThunk = new Lazy<Credentials>(() =>
        {
            var githubToken = Environment.GetEnvironmentVariable("OCTOKIT_OAUTHTOKEN");

            if (!string.IsNullOrWhiteSpace(githubToken))
            {
                return new Credentials(githubToken);
            }

            return Credentials.Anonymous;
        });

        public static Credentials Credentials
        {
            get { return CredentialsThunk.Value; }
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