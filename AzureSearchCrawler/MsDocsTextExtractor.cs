using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace AzureSearchCrawler
{
    class MsDocsTextExtractor : TextExtractor
    {
        private readonly Regex whitespace = new Regex(@"\s+");

        public override string ExtractText(HtmlDocument doc)
        {
            if (doc == null || doc.DocumentNode == null)
            {
                return null;
            }

            RemoveNodesOfType(doc, "script", "style", "svg", "path");

            string content = ExtractTextFromFirstMatchingElement(doc, "//div[@class = 'content']");

            if (content != null)
            {
                content = whitespace.Replace(content, " ");
            }

            return content;
        }
    }
}
