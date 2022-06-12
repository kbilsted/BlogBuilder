namespace Kbg.BlogBuilder.Domain
{
    public class Configuration
    {
        public readonly string ReadBasePath, WritePath;
        public string ArticlesPath => Path.Combine(ReadBasePath, "Articles");

        public Configuration(string readBasePath, string writePath)
        {
            ReadBasePath = readBasePath;
            WritePath = writePath;
        }
    }

    public record TocEntry(string Title, string Level);

    public record DocumentInfo(string Path, string Content, string Title);

}