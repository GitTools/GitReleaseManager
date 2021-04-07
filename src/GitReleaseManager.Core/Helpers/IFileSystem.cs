using System.Collections.Generic;
using System.IO;

namespace GitReleaseManager.Core.Helpers
{
    public interface IFileSystem
    {
        void Copy(string source, string destination, bool overwrite);

        void Move(string source, string destination);

        bool Exists(string file);

        void Delete(string path);

        string ReadAllText(string path);

        string ResolvePath(string path);

        void WriteAllText(string file, string fileContents);

        IEnumerable<string> DirectoryGetFiles(string directory, string searchPattern, SearchOption searchOption);

        Stream OpenWrite(string path);
    }
}