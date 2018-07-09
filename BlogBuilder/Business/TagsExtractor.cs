using System;
using System.Linq;
using System.Text.RegularExpressions;
using Kbg.BlogBuilder.Domain;
using Kbg.BlogBuilder.Io;

namespace Kbg.BlogBuilder.Business
{
    public class TagsExtractor
    {
        static readonly RegexOptions Options = RegexOptions.Compiled | RegexOptions.Singleline;
        public static readonly Regex CategoryEx = new Regex(@"<Categories Tags=""(?<tags>[^""]*)"">[^<]+</Categories>", Options);

        readonly IFilesystemRepository filesystemRepository;

        public TagsExtractor(IFilesystemRepository filesystemRepository)
        {
            this.filesystemRepository = filesystemRepository;
        }

        public TagCollection Execute(Configuration configuration)
        {
            var tags = filesystemRepository.EnumerateFiles(configuration.ArticlesPath, "*.md")
                .Select(path => new
                {
                    relativePath = path.FullName.Substring(configuration.ReadBasePath.Length),
                    fileContent = filesystemRepository.ReadAllText(path.FullName)
                })
                .Where(x => !DocumentParser.IsDraftFile(x.fileContent))
                .Select(x => ParsePage(x.fileContent, x.relativePath));

            return new TagCollection(tags);
        }

        internal static TagCollection ParsePage(string pageContent, string fullName)
        {
            var title = DocumentParser.GetParsePageTitle(pageContent);

            var tags = GetTagsFromContent(pageContent)
                .Select(x => Tuple.Create(x, new[] { new Page(title, fullName) }));

            return new TagCollection(tags);
        }

        static Tag[] GetTagsFromContent(string content)
        {
            return ExtractTags(CategoryEx.Match(content).Groups["tags"].Value);
        }

        public static Tag[] ExtractTags(string tagsArgument)
        {
            var parsedTags = tagsArgument
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(y => new Tag(y.Trim()))
                .ToArray();
            return parsedTags;
        }
    }
}