using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace AzureSearchCrawler
{
    /// <summary>
    /// Extracts text content from a web page. The default implementation is very simple: it removes all script, style,
    /// svg, and path tags, and then returns the InnerText of the page body.
    /// <para/>You can implement your own custom text extraction by overriding the ExtractText method. The protected
    /// helper methods in this class might be useful.
    /// </summary>
    public class TextExtractor
    {
        public virtual string ExtractText(HtmlDocument doc)
        {
            if (doc == null || doc.DocumentNode == null)
            {
                return null;
            }

            RemoveNodesOfType(doc, "script", "style", "svg", "path");
            return ExtractTextFromFirstMatchingElement(doc, "//body");
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
    }
}
