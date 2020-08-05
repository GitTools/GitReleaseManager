using System;

namespace GitReleaseManager.Core
{
    /// <summary>
    /// Provides static methods that help a constructor or method to verify correct arguments and
    /// state.
    /// </summary>
    public static class Ensure
    {
        /// <summary>
        /// Checks whether a specified string is not <strong>null</strong>, empty, or consists only
        /// of white-space characters.
        /// </summary>
        /// <param name="str">The string to test.</param>
        /// <param name="paramName">The name of the parameter that caused the exception.</param>
        /// <exception cref="ArgumentNullException"><em>str</em> is
        /// <strong>null</strong>.</exception>
        /// <exception cref="ArgumentException"><em>str</em> is
        /// <strong>whitespace</strong>.</exception>
        public static void IsNotNullOrWhiteSpace(string str, string paramName)
        {
            if (str == null)
            {
                throw new ArgumentNullException(paramName);
            }
            else if (string.IsNullOrWhiteSpace(str))
            {
                throw new ArgumentException("Value cannot be empty or white-space.", paramName);
            }
        }
    }
}