using MarkdownDeep;

namespace Kbg.BlogBuilder.Business;

public class MarkdownToHtml
{
    private readonly Markdown md = new Markdown
    {
        ExtraMode = true,
        SafeMode = false,
        AutoHeadingIDs = true,
        FormatCodeBlock = (m, code) => $"<pre class=\"prettyprint\"><code>{code}</code></pre>\n"
    };

    public string Execute(string markdownContent, string pageTitle, string baseUrl)
    {
        var footer = $@"<br>
<br>
Read the [Introduction]({baseUrl}) or browse the rest [of the site]({baseUrl}AllArticles.html)
<br>
<br>
";
        var html = md.Transform(markdownContent + footer);
        var googleAnalytics = @"
<script>
  (function(i,s,o,g,r,a,m){{i['GoogleAnalyticsObject']=r;i[r]=i[r]||function(){{
  (i[r].q=i[r].q||[]).push(arguments)}},i[r].l=1*new Date();a=s.createElement(o),
  m=s.getElementsByTagName(o)[0];a.async=1;a.src=g;m.parentNode.insertBefore(a,m)
  }})(window,document,'script','//www.google-analytics.com/analytics.js','ga');

  ga('create', 'UA-66546851-1', 'auto');
  ga('send', 'pageview');
</script>
";
        var htmlWithCss = $@"<html>
<head>
<title>{pageTitle}</title>

<script src=""https://cdn.rawgit.com/google/code-prettify/master/loader/run_prettify.js""></script>
<link href=""http://firstclassthoughts.co.uk/atelier-forest-light.css"" type =""text/css"" rel=""stylesheet"" />

<link href=""{baseUrl}github-markdown.css"" type =""text/css"" rel=""stylesheet"">
<link rel='shortcut icon' type='image/x-icon' href='{baseUrl}favicon.ico'/>
<style>
      .markdown-body {{
                min-width: 200px;
                max-width: 790px;
                margin: 0 auto;
                padding: 30px;
            }}
</style>

{googleAnalytics}

</head>
<body onload=""prettyPrint()"">
<article class=""markdown-body"">

{html}


</article>
</body>
</html>";

        return htmlWithCss;
    }
}