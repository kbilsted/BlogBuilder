using System.IO;
using System.Linq;
using Kbg.BlogBuilder.Domain;
using Kbg.BlogBuilder.Io;

namespace Kbg.BlogBuilder.Business
{
    public class ReadMdAndWriteHtml
    {
        readonly ContentGenerator contentGenerator;
        readonly IFilesystemRepository filesystemRepository;
        readonly MarkdownToHtml markdownToHtml;
        readonly ExpandTagsToMarkdown expandTagsToMarkdown;

        public ReadMdAndWriteHtml(IFilesystemRepository filesystemRepository, ContentGenerator contentGenerator, MarkdownToHtml markdownToHtml, ExpandTagsToMarkdown expandTagsToMarkdown)
        {
            this.filesystemRepository = filesystemRepository;
            this.contentGenerator = contentGenerator;
            this.markdownToHtml = markdownToHtml;
            this.expandTagsToMarkdown = expandTagsToMarkdown;
        }

        public void Execute(Configuration rootFilePath, TagCollection tags, string baseUrl, string editBaseUrl)
        {
            var files = filesystemRepository.EnumerateFiles(rootFilePath.ArticlesPath, "*.md")
                .Union(filesystemRepository.EnumerateFiles(rootFilePath.ReadBasePath, "*.md", SearchOption.TopDirectoryOnly))
                .ToList();

            var output = expandTagsToMarkdown.Execute(rootFilePath, tags, baseUrl, editBaseUrl, files, contentGenerator);

            foreach (var inf in output)
            {
                var html = markdownToHtml.Execute(inf.Content, inf.Title, baseUrl);

                filesystemRepository.WriteFile(inf.Path, html);
            }
        }
    }
}