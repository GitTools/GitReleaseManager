//-----------------------------------------------------------------------
// <copyright file="FileSystem.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Core.Helpers
{
    using System.Collections.Generic;
    using System.IO;

    public class FileSystem : IFileSystem
    {
        public void Copy(string @source, string destination, bool overwrite)
        {
            File.Copy(@source, destination, overwrite);
        }

        public void Move(string @source, string destination)
        {
            File.Move(@source, destination);
        }

        public bool Exists(string file)
        {
            return File.Exists(file);
        }

        public void Delete(string path)
        {
            File.Delete(path);
        }

        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        public void WriteAllText(string file, string fileContents)
        {
            File.WriteAllText(file, fileContents);
        }

        public IEnumerable<string> DirectoryGetFiles(string directory, string searchPattern, SearchOption searchOption)
        {
            return Directory.GetFiles(directory, searchPattern, searchOption);
        }

        public Stream OpenWrite(string path)
        {
            return File.OpenWrite(path);
        }
    }
}