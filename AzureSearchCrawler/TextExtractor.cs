using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AzureSearchCrawler
{
    /// <summary>
    /// Extracts text content from a web page. The default implementation is very simple: it removes all script, style,
    /// svg, and path tags, and then returns the InnerText of the page body, with cleaned up whitespace.
    /// <para/>You can implement your own custom text extraction by overriding the ExtractText method. The protected
    /// helper methods in this class might be useful. GetCleanedUpTextForXpath is the easiest way to get started.
    /// </summary>
    public partial class TextExtractor
    {
        private readonly Regex newlines = MyRegex();
        private readonly Regex spaces = MyRegex1();

        public virtual string ExtractText(string content)
        {
            HtmlDocument doc = new();
            doc.LoadHtml(content);
            return GetCleanedUpTextForXpath(doc, "//body");
        }

        public string GetCleanedUpTextForXpath(HtmlDocument doc, string xpath)
        {
            if (doc == null || doc.DocumentNode == null)
            {
                return null;
            }

            RemoveNodesOfType(doc, "script", "style", "svg", "path");

            string content = ExtractTextFromFirstMatchingElement(doc, xpath);
            return NormalizeWhitespace(content);
        }

        protected string NormalizeWhitespace(string content)
        {
            if (content == null)
            {
                return null;
            }

            content = newlines.Replace(content, "\n");
            return spaces.Replace(content, " ");
        }

        protected void RemoveNodesOfType(HtmlDocument doc, params string[] types)
        {
            string xpath = String.Join(" | ", types.Select(t => "//" + t));
            RemoveNodes(doc, xpath);
        }

        protected void RemoveNodes(HtmlDocument doc, string xpath)
        {
            var nodes = SafeSelectNodes(doc, xpath).ToList();
            // Console.WriteLine("Removing {0} nodes matching {1}.", nodes.Count, xpath);
            foreach (var node in nodes)
            {
                node.Remove();
            }
        }

        /// <summary>
        /// Returns InnerText of the first element matching the xpath expression, or null if no elements match.
        /// </summary>
        protected string ExtractTextFromFirstMatchingElement(HtmlDocument doc, string xpath)
        {
            return SafeSelectNodes(doc, xpath).FirstOrDefault()?.InnerText;
        }

        /// <summary>
        /// Null-safe DocumentNode.SelectNodes
        /// </summary>
        protected IEnumerable<HtmlNode> SafeSelectNodes(HtmlDocument doc, string xpath)
        {
            return doc.DocumentNode.SelectNodes(xpath) ?? Enumerable.Empty<HtmlNode>();
        }

        [GeneratedRegex(@"(\r\n|\n)+")]
        private static partial Regex MyRegex();
        [GeneratedRegex(@"[ \t]+")]
        private static partial Regex MyRegex1();
    }
}
