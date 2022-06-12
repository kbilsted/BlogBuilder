namespace Kbg.BlogBuilder.Io;

public class FilesystemRepository : IFilesystemRepository
{
    public void EnsureEmptyTagDirectory(string tagDir)
    {
        if (Directory.Exists(tagDir))
            Directory.Delete(tagDir, true);

        Directory.CreateDirectory(tagDir);
    }

    public void WriteFile(string filepath, string content)
    {
        var path = Path.GetDirectoryName(filepath);
        if (path == null) throw new Exception("Path invalid");
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        var destination = Path.Combine(path, Path.GetFileNameWithoutExtension(filepath)) + ".html";
        File.WriteAllText(destination, content, new UTF8Encoding(true));
    }

    public string ReadAllText(string filepath)
    {
        return File.ReadAllText(filepath, new UTF8Encoding());
    }

    public void Copy(string sourcepath, string destinationPath)
    {
        var path = Path.GetDirectoryName(destinationPath);
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        File.Copy(sourcepath, destinationPath, true);
    }

    public IEnumerable<FileInfo> EnumerateFiles(string rootPath, string filter, SearchOption options = SearchOption.AllDirectories)
    {
        var di = new DirectoryInfo(rootPath);
        return di.EnumerateFiles(filter, options);
    }

    public bool Exists(string path) => File.Exists(path);
}