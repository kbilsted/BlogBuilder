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
}