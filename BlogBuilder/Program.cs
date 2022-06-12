using System.Diagnostics;
using System.Text;
using Kbg.BlogBuilder.Business;
using Kbg.BlogBuilder.Domain;
using Kbg.BlogBuilder.Io;

namespace Kbg.BlogBuilder
{
    internal class Program
    {
        const string ReadPath = @"C:\src\CodeQualityAndReadability\";
        const string WritePath = ReadPath + "docs";

        readonly string baseUrl = @"http://firstclassthoughts.co.uk/";

        readonly string editBaseUrl = "https://github.com/kbilsted/CodeQualityAndReadability/blob/master/";

        static void Main(string[] args)
        {
            Stopwatch watch = Stopwatch.StartNew();
            var program = new Program();

            var configuration = new Configuration(ReadPath, WritePath);

            var filesystemRepository = new FilesystemRepository();
            program.MutateTocInMdFiles(configuration, filesystemRepository);
            program.GenerateSite(configuration, filesystemRepository);

            Console.WriteLine($"Done in {watch.ElapsedMilliseconds} millis.");
        }

        public void GenerateSite(Configuration configuration, FilesystemRepository filesystemRepository)
        {
            var contentGenerator = new ContentGenerator();
            var extractor = new TagsExtractor(filesystemRepository);
            var htmlTransformer = new MarkdownToHtml();
            var expandTagsToMarkdown = new ExpandTagsToMarkdown(filesystemRepository);
            var readMdAndWriteHtml = new ReadMdAndWriteHtml(filesystemRepository, contentGenerator, htmlTransformer, expandTagsToMarkdown);

            var siteGenerator = new SiteGenerator(
                contentGenerator,
                filesystemRepository,
                extractor,
                readMdAndWriteHtml,
                htmlTransformer);

            siteGenerator.Execute(configuration, baseUrl, editBaseUrl);
        }

        public void MutateTocInMdFiles(Configuration folder, IFilesystemRepository filesystemRepository)
        {
            var files = filesystemRepository.EnumerateFiles(folder.ArticlesPath, "*.md")
                .Union(new DirectoryInfo(folder.ReadBasePath).EnumerateFiles("*.md", SearchOption.TopDirectoryOnly));

            foreach (var path in files)
            {
                if (path.FullName.EndsWith("Readme.md"))
                    continue;
                Console.Write(path.FullName);
                MutateTocSection(path.FullName, filesystemRepository);
            }

            void MutateTocSection(string path, IFilesystemRepository repository)
            {
                var content = repository.ReadAllText(path);

                var toc = new TocCreator().Execute(content);
                var newContent = new TocContentReplacer().TryReplaceToc(content, toc);

                // only write file if content has changed since the modified date of the file is used for determining new files
                if (newContent != null && content != newContent)
                {
                    Console.Write("    ... Updating");
                    File.WriteAllText(path, newContent, new UTF8Encoding(true));
                }

                Console.WriteLine("");
            }
        }
    }
}
