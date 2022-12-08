namespace Kbg.BlogBuilder.Business;

public class SiteGenerator
{
    readonly IFilesystemRepository filesystemRepository;
    readonly ContentGenerator generator;
    readonly ReadMdAndWriteHtml readMdAndWriteHtml;
    readonly TagsExtractor tagsExtractor;
    readonly MarkdownToHtml markdownToHtml;

    public SiteGenerator(ContentGenerator generator,
        IFilesystemRepository filesystemRepository,
        TagsExtractor tagsExtractor,
        ReadMdAndWriteHtml readMdAndWriteHtml,
        MarkdownToHtml markdownToHtml)
    {
        this.generator = generator;
        this.filesystemRepository = filesystemRepository;
        this.tagsExtractor = tagsExtractor;
        this.readMdAndWriteHtml = readMdAndWriteHtml;
        this.markdownToHtml = markdownToHtml;
    }

    public void Execute(Configuration cfg, string baseUrl, string editBaseUrl)
    {
        var tags = tagsExtractor.Execute(cfg);

        CopyImmutableFiles(cfg);
        readMdAndWriteHtml.Execute(cfg, tags, baseUrl, editBaseUrl);
        WriteAllArticlesPage(cfg.WritePath, tags, baseUrl);
        WriteTagPages(cfg.WritePath, tags, baseUrl);
        WriteAllTagsPage(cfg.WritePath, tags, baseUrl);
    }

    void CopyImmutableFiles(Configuration cfg)
    {
        var toplevelFiles = filesystemRepository.EnumerateFiles(cfg.ReadBasePath, "*.*", SearchOption.TopDirectoryOnly);
        var img = filesystemRepository.EnumerateFiles(Path.Combine(cfg.ReadBasePath, "img"), "*.*");
        var articles = filesystemRepository.EnumerateFiles(cfg.ArticlesPath, "*.*");

        var copyableFiles = toplevelFiles.Union(img).Union(articles)
            .Where(x => !x.FullName.StartsWith(cfg.WritePath))
            .Where(x => IsToCopy(x));

        foreach (var path in copyableFiles)
        {
            var relativePath = path.FullName.Substring(cfg.ReadBasePath.Length);
            string destinationPath = Path.Combine(cfg.WritePath, relativePath);
            if (!File.Exists(destinationPath)) {
                filesystemRepository.Copy(path.FullName, destinationPath);
            }
        }
    }

    bool IsToCopy(FileInfo path)
    {
        var ext = path.Extension.ToLower();
        var isImage = ext == ".png" || ext == ".jpeg" || ext == ".gif" || ext == ".jpg" || ext == ".ico";
        var isVideo = ext == ".mp4";
        var html = ext == ".css" || ext == ".js";
        var isRelevant = html || path.FullName.EndsWith("CNAME");

        return isImage || isVideo || isRelevant;
    }

    void WriteAllTagsPage(string writePath, TagCollection tags, string baseUrl)
    {
        var allTagsPage = generator.GenerateAllTagsPage(tags.Select(x => x.Key).ToList(), baseUrl);
        var html = markdownToHtml.Execute(allTagsPage, "All categories on Quality and Readability", baseUrl);

        filesystemRepository.WriteFile(Path.Combine(writePath, "AllTags.md"), html);
    }

    void WriteTagPages(string writePath, TagCollection tags, string baseUrl)
    {
        var tagDir = Path.Combine(writePath, "Tags");
        filesystemRepository.EnsureEmptyTagDirectory(tagDir);

        foreach (var tag in tags)
        {
            var tagPage = generator.GenerateTagPage(tag.Key, tag.Value, baseUrl);
            var html = markdownToHtml.Execute(tagPage, $"Pages related to {tag.Key.Value}", baseUrl);
            filesystemRepository.WriteFile(Path.Combine(tagDir, tag.Key + ".html"), html);
        }
    }

    void WriteAllArticlesPage(string writePath, TagCollection tags, string baseUrl)
    {
        var allArticles = generator.GenerateAllArticlesPage(tags.SelectMany(x => x.Value).ToList(), baseUrl);
        var html = markdownToHtml.Execute(allArticles, "All articles on Quality and Readability", baseUrl);
        filesystemRepository.WriteFile(Path.Combine(writePath, "AllArticles.md"), html);
    }
}