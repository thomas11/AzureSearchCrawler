using HtmlAgilityPack;

namespace AzureSearchCrawler
{
    class MsDocsTextExtractor : TextExtractor
    {
        public override string ExtractText(HtmlDocument doc)
        {
            return GetCleanedUpTextForXpath(doc, "//div[@class = 'content']");
        }
    }
}
