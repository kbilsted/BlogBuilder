using System;
using System.Text.RegularExpressions;

namespace Kbg.BlogBuilder.Business
{
    public class DocumentParser
    {
        static readonly Regex HeaderEx = new Regex("^# (?<title>.*)$", RegexOptions.Compiled | RegexOptions.Multiline);

        public static string GetParsePageTitle(string pageContent)
        {
            var headerMatch = HeaderEx.Match(pageContent);
            var title = headerMatch.Groups["title"].Value.Trim();

            return title;
        }

        public static bool IsDraftFile(string fileContent)
        {
            return fileContent.StartsWith("draft", StringComparison.OrdinalIgnoreCase);
        }
    }
}