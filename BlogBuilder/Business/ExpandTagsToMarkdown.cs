using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Kbg.BlogBuilder.Domain;
using Kbg.BlogBuilder.Io;

namespace Kbg.BlogBuilder.Business
{
    public class ExpandTagsToMarkdown
    {
        static readonly RegexOptions Options = RegexOptions.Compiled | RegexOptions.Singleline;
        static readonly Regex TopXLatestArticledEx = new Regex("<Top4LatestArticles/>", Options);
        static readonly Regex ArticleHeaderUrlsEx = new Regex("<ArticleHeaderUrls/>", Options);
        static readonly Regex BaseUrlTagEx = new Regex("<BaseUrl/>", Options);
        static readonly Regex GithubPageUrlEx = new Regex("<GithubPageUrl/>", Options);
        static readonly Regex AllTagsEx = new Regex(@"<AllTags\s* />", Options);
        static readonly Regex CommentTextEx = new Regex("<CommentText>[^<]+</CommentText>", Options);
        static readonly Regex SocialButtonShareEx = new Regex("<SocialShareButtons>[^<]+</SocialShareButtons>", Options);
        static readonly Regex ArticleCountEx = new Regex(@"<ArticleCount/>", Options);

        private readonly IFilesystemRepository filesystemRepository;

        public ExpandTagsToMarkdown(IFilesystemRepository filesystemRepository)
        {
            this.filesystemRepository = filesystemRepository;
        }

        static List<Tuple<FileInfo, string, string>> GetTop4NewestFiles(Configuration rootFilePath, List<FileInfo> files)
        {
            var startIndex = rootFilePath.ReadBasePath.Length;

            return files
                .Where(x => x.FullName.StartsWith(Path.Combine(rootFilePath.ReadBasePath, "Articles")))
                .OrderByDescending(x => x.LastWriteTime)
                .Select(x => new {File = x, Content = File.ReadAllText(x.FullName)})
                .Where(x => !DocumentParser.IsDraftFile(x.Content))
                .Take(4)
                .Select(x =>
                    Tuple.Create(
                        x.File,
                        x.File.FullName.Substring(startIndex, x.File.FullName.Length - startIndex - 2) + "html",
                        x.Content))
                .ToList();
        }

        static string MutateTopXArticles(string fileContent, List<Tuple<FileInfo, string, string>> files, string baseUrl)
        {
            var toplist = files
                .Select(x => new
                {
                    Content = x.Item3,
                    Path = x.Item2.Replace('\\', '/'),
                    NewSign = IsLessThan30DaysOld(x.Item1.CreationTime) ? @"<img src=""img/new.gif"">" : ""
                })
                .Select(x => $"* [{DocumentParser.GetParsePageTitle(x.Content)}]({x.Path}) {x.NewSign}");

            var content = TopXLatestArticledEx.Replace(fileContent, x => string.Join("\n", toplist));
            return content;
        }

        static bool IsLessThan30DaysOld(DateTime t) => t > DateTime.Now.AddDays(-30);

        static string MutateArticleHeaderUrlsTag(string fileContent)
        {
            var content = ArticleHeaderUrlsEx.Replace(fileContent,
                x =>
                    @"<br>" +
                    @"[[Introduction]](<BaseUrl/>) " + 
                    @"[[All categories]](<BaseUrl/>AllTags.html) " +
                    @"[[All articles]](<BaseUrl/>AllArticles.html) " +
                    @"[[Edit article <img src=""http://firstclassthoughts.co.uk/img/edit.png""> ]](<GithubPageUrl/>)" +
                    @"<br>");
            return content;
        }

        static string MutateBaseUrlTag(string fileContent, string baseUrl)
        {
            var content = BaseUrlTagEx.Replace(fileContent, x => baseUrl);
            return content;
        }

        static string MutateGithubPageUrlTag(string fileContent, string editUrl)
        {
            var content = GithubPageUrlEx.Replace(fileContent, x => editUrl);
            return content;
        }


        static string MutateAllTagsLine(string fileContent, TagCollection tags, string baseUrl, ContentGenerator contentGenerator)
        {
            var content = AllTagsEx.Replace(
                fileContent,
                z => string.Join(" ", tags
                    .Select(x => x.Key)
                    .OrderBy(x => x.Value)
                    .Select(x => contentGenerator.GenerateCategoryLink(x, baseUrl))));
            return content;
        }

        static string MutateCategoryTags(string fileContent, string baseUrl, ContentGenerator contentGenerator)
        {
            var content = TagsExtractor.CategoryEx.Replace(
                fileContent,
                x =>
                {
                    var parsedTags = TagsExtractor.ExtractTags(x.Groups["tags"].Value);
                    var links = parsedTags.Select(tag => contentGenerator.GenerateCategoryLink(tag, baseUrl));
                    return string.Join("\r\n", links) + "\r\n";
                });

            return content;
        }

        static string MutateCommentText(string fileContent, string editUrl)
        {
            var textBody =
                $@"**Congratulations! You've come all the way to the bottom of the article! Please help me make this site better for everyone by commenting below. Or how about making editorial changes? Feel free to fix spelling mistakes, weird sentences, or correct what is plain wrong. All the material is on GitHub so don't be shy.** <a href=""{
                        editUrl
                    }"">Just go to Github, press the edit button and fire away.</a>
<br>";

            var disqusStuff = @"<div id=""disqus_thread""></div>
<script type=""text/javascript"">
    /* * * CONFIGURATION VARIABLES * * */
    var disqus_shortname = 'qualityandreadability';
    
    /* * * DON'T EDIT BELOW THIS LINE * * */
    (function() {
        var dsq = document.createElement('script'); dsq.type = 'text/javascript'; dsq.async = true;
        dsq.src = '//' + disqus_shortname + '.disqus.com/embed.js';
        (document.getElementsByTagName('head')[0] || document.getElementsByTagName('body')[0]).appendChild(dsq);
    })();
</script>
<noscript>Please enable JavaScript to view the <a href=""https://disqus.com/?ref_noscript"" rel=""nofollow"">comments powered by Disqus.</a></noscript>";

            var content = CommentTextEx.Replace(fileContent, x => $"{textBody}<br><br>{ContentGenerator.Newsletter}{disqusStuff}");

            return content;
        }

        static string MutateSocialLinks(string fileContent, string baseUrl, string relativePath)
        {
            var url = string.Format("{0}{1}.html", baseUrl, relativePath.Substring(0, relativePath.Length - 3));

            var title = new string(fileContent.TakeWhile(x => x != '\n').ToArray()).Substring(1).Trim();
            title = title.Replace(" ", "%20");

            var content = SocialButtonShareEx.Replace(
                fileContent,
                x =>
                    string.Format(@"
[![Reddit this]({0}reddit.png)](https://www.reddit.com/submit?url={1}&title={2})
[![Tweet this]({0}twitter.png)](https://twitter.com/intent/tweet?url={1}&text={2}&via=kbilsted)
[![Googleplus this]({0}gplus.png)](https://plus.google.com/share?url={1})
[![Facebook this]({0}facebook.png)](https://facebook.com/sharer.php?u={1}&t={2})
[![LinkedIn this]({0}linkedin.png)](http://www.linkedin.com/shareArticle?mini=true&url={1})
[![Feedly this]({0}feedly.png)](http://cloud.feedly.com/#subscription%2Ffeed%2F{1})
[![Ycombinator this]({0}ycombinator.png)](http://news.ycombinator.com/submitlink?u={1}&t={2})
", baseUrl + "img/", url, title));

            return content;
        }

        static string MutateTagArticleCount(string fileContent, int numberOfPages)
        {
            return ArticleCountEx.Replace(fileContent, numberOfPages.ToString());
        }

        public IEnumerable<DocumentInfo> Execute(
            Configuration rootFilePath, 
            TagCollection tags, 
            string baseUrl,
            string editBaseUrl, 
            List<FileInfo> files, 
            ContentGenerator contentGenerator)
        {
            var top4Files = GetTop4NewestFiles(rootFilePath, files);
            var articleCount = tags.SelectMany(x => x.Value).Distinct().Count();

            foreach (var path in files)
            {
                var fileContent = filesystemRepository.ReadAllText(path.FullName);

                if (DocumentParser.IsDraftFile(fileContent))
                    continue;

                var relativePath = path.FullName.Substring(rootFilePath.ReadBasePath.Length).Replace('\\', '/');
                var editUrl = editBaseUrl + relativePath;

                fileContent = MutateTopXArticles(fileContent, top4Files, baseUrl);
                fileContent = MutateArticleHeaderUrlsTag(fileContent);
                fileContent = MutateSocialLinks(fileContent, baseUrl, relativePath);
                fileContent = MutateCommentText(fileContent, editUrl);
                fileContent = MutateCategoryTags(fileContent, baseUrl, contentGenerator);
                fileContent = MutateAllTagsLine(fileContent, tags, baseUrl, contentGenerator);
                fileContent = MutateBaseUrlTag(fileContent, baseUrl);
                fileContent = MutateGithubPageUrlTag(fileContent, editUrl);
                fileContent = MutateTagArticleCount(fileContent, articleCount);

                var title = DocumentParser.GetParsePageTitle(fileContent);

                yield return
                    new DocumentInfo
                    {
                        Content = fileContent,
                        Path = Path.Combine(rootFilePath.WritePath, relativePath),
                        Title = title
                    };
            }
        }
    }
}