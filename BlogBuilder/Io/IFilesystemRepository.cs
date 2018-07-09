using System.Collections.Generic;
using System.IO;

namespace Kbg.BlogBuilder.Io
{
    public interface IFilesystemRepository
    {
        void EnsureEmptyTagDirectory(string tagDir);

        void WriteFile(string filepath, string content);

        string ReadAllText(string filepath);

        void Copy(string sourcepath, string destinationPath);

        IEnumerable<FileInfo> EnumerateFiles(string rootPath, string filter);
    }
}