[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("GitReleaseManager.Tests")]

namespace GitReleaseManager.Core.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    internal static class StringExtensions
    {
        public static string ReplaceMilestoneTitle(this string source, string milestoneKey, string milestoneTitle)
        {
            var dict = new Dictionary<string, object>
            {
                { milestoneKey.Trim('{','}'), milestoneTitle },
            };

            return source.ReplaceTemplate(dict);
        }

        public static string ReplaceTemplate(this string source, object values)
        {
            IDictionary<string, object> pairs;

            if (values is IDictionary<string, object> dict)
            {
                pairs = new Dictionary<string, object>(dict, StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                var type = values.GetType();
                var properties = type.GetProperties();
                pairs = properties.ToDictionary(x => x.Name, x => x.GetValue(values), StringComparer.OrdinalIgnoreCase);
            }

            var sb = new StringBuilder();
            int i, prevI = 0;
            while ((i = source.IndexOf('{', prevI)) >= 0)
            {
                sb.Append(source.Substring(prevI, i - prevI));
                prevI = i + 1;
                if ((i = source.IndexOf('}', prevI)) > prevI)
                {
                    var name = source.Substring(prevI, i - prevI);
                    if (pairs.ContainsKey(name))
                    {
                        sb.Append(pairs[name]);
                    }

                    prevI = ++i;
                }
            }

            if (source.Length > prevI)
            {
                sb.Append(source.Substring(prevI, source.Length - prevI));
            }

            return sb.ToString();
        }
    }
}