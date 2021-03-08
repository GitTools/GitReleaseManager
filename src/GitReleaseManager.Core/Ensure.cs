namespace GitReleaseManager.Core
{
    using System;
    using System.IO;

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

            if (string.IsNullOrWhiteSpace(str))
            {
                throw new ArgumentException("Value cannot be empty or white-space.", paramName);
            }
        }

        /// <summary>
        /// Checks if a file exists.
        /// </summary>
        /// <param name="path">The path to test.</param>
        /// <param name="message">A message that describes the error.</param>
        /// <exception cref="FileNotFoundException">File does not exist.</exception>
        public static void FileExists(string path, string message)
        {
            if (!File.Exists(path))
            {
                var fileName = Path.GetFileName(path);
                throw new FileNotFoundException(message, fileName);
            }
        }
    }
}