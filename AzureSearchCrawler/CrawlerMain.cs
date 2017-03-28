using System;

namespace AzureSearchCrawler
{
    /// <summary>
    /// The entry point of the crawler. Adjust the constants at the top and run.
    /// </summary>
    class CrawlerMain
    {
        // EDIT HERE - the page you'd like to crawl.
        private const string RootUri = "";
        private const int MaxPagesToIndex = 10;

        // EDIT HERE - get these values for your search service from the Azure portal.
        private const string ServiceName = "";
        private const string IndexName = "";
        private const string AdminApiKey = "";

        static void Main(string[] args)
        {
            var indexer = new AzureSearchIndexer(ServiceName, IndexName, AdminApiKey, new TextExtractor());
            var crawler = new Crawler(indexer);
            crawler.Crawl(RootUri, maxPages: MaxPagesToIndex).Wait();
            Console.Read(); // keep console open until a button is pressed so we see the output
        }
    }
}
