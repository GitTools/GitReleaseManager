//-----------------------------------------------------------------------
// <copyright file="IFileSystem.cs" company="gep13">
//     Copyright (c) 2015 - Present gep13
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Core.Helpers
{
    using System.Collections.Generic;
    using System.IO;

    public interface IFileSystem
    {
        void Copy(string source, string destination, bool overwrite);

        void Move(string source, string destination);

        bool Exists(string file);

        void Delete(string path);

        string ReadAllText(string path);

        void WriteAllText(string file, string fileContents);

        IEnumerable<string> DirectoryGetFiles(string directory, string searchPattern, SearchOption searchOption);

        Stream OpenWrite(string path);
    }
}