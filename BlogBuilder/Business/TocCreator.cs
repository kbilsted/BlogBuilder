using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static System.Environment;

namespace Kbg.BlogBuilder.Business
{
    public class TocCreator
    {
        static readonly Regex TocRex = new Regex("^(?<level>#+)( |\t)+(?<title>.*)", RegexOptions.Multiline | RegexOptions.Compiled);

        public string Execute(string content)
        {
            var parsedToc = Parse(content);

            var tocString = MarkDown(parsedToc);
            return tocString;
        }

        string MarkDown(List<TocEntry> parsedToc)
        {
            var tocString = "Table of Content";
            if (!parsedToc.Any())
                return tocString;

            var markdownedTocLines = parsedToc.Select(x => RemoveSpecialCharactersAndIndent(x));
            tocString += string.Format("{0}{0}{1}{0}", NewLine, string.Join(NewLine, markdownedTocLines));

            return tocString;
        }


        List<TocEntry> Parse(string content)
        {
            var lines = content.Split(new[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

            var tocLines =
                lines.SkipWhile(x => !x.ToLower().Contains("table of content"))
                    .Skip(1)
                    .Select(x => x.Trim())
                    .Where(x => x.StartsWith("#"))
                    .ToArray();

            var parsedToc =
                tocLines.Select(x => TocRex.Match(x))
                    .Select(x => new TocEntry(
                        Title : x.Groups["title"].Value, 
                        Level : x.Groups["level"].Value));
            return parsedToc.ToList();
        }

        static readonly Regex StuffRemover = new Regex("(`[^`]+`)|(^\\d+([.]\\d*)*)", RegexOptions.Multiline | RegexOptions.Compiled);

        string RemoveSpecialCharactersAndIndent(TocEntry entry)
        {
            string indent = entry.Level == "" ? "" : entry.Level.Replace("#", "  ").Substring(1);
            var link = "#" + StuffRemover.Replace(entry.Title, x => "")
                           .Replace(".", "")
                           .Replace("'", "")
                           .Replace(",", "")
                           .Replace(":", "")
                           .Replace("!", "")
                           .Replace("/", "")
                           .Replace("\"", "")
                           .Replace("`", "")
                           .Replace("(", "")
                           .Replace(")", "")
                           .Trim()
                           .Replace(" ", "-")
                           .ToLowerInvariant();

            return $"{indent}* [{entry.Title}]({link})";
        }
    }
}