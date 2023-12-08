using Fclp;
using System;

namespace AzureSearchCrawler
{
    /// <summary>
    /// The entry point of the crawler. Adjust the constants at the top and run.
    /// </summary>
    class CrawlerMain
    {
        private const int DefaultMaxPagesToIndex = 100;
        private const int DefaultMaxCrawlDepth = 10;

        private class Arguments
        {
            public string RootUri { get; set; }

            public int MaxPagesToIndex { get; set; }

            public int MaxCrawlDepth { get; set; }

            public string ServiceEndPoint { get; set; }

            public string IndexName { get; set; }

            public string AdminApiKey { get; set; }

            public bool ExtractText { get; set; }
        }

        static void Main(string[] args)
        {
            var p = new FluentCommandLineParser<Arguments>();

            p.Setup(arg => arg.RootUri)
                .As('r', "rootUri")
                .Required()
                .WithDescription("Start crawling from this web page");

            p.Setup(arg => arg.MaxPagesToIndex)
                .As('m', "maxPages")
                .SetDefault(DefaultMaxPagesToIndex)
                .WithDescription("Stop after having indexed this many pages. Default is " + DefaultMaxPagesToIndex + "; 0 means no limit.");

            p.Setup(arg => arg.MaxCrawlDepth)
                  .As('d', "maxDepth")
                  .SetDefault(DefaultMaxCrawlDepth)
                  .WithDescription("Maximum crawl depth. Default is " + DefaultMaxCrawlDepth);

            p.Setup(arg => arg.ServiceEndPoint)
                .As('s', "ServiceEndPoint")
                .Required()
                .WithDescription("The Url (service end point) of your Azure Search service");

            p.Setup(arg => arg.IndexName)
                .As('i', "IndexName")
                .Required()
                .WithDescription("The name of the index in your Azure Search service");

            p.Setup(arg => arg.AdminApiKey)
                .As('a', "AdminApiKey")
                .Required();

            p.Setup(arg => arg.ExtractText)
                .As('e', "ExtractText")
                .SetDefault(true)
                .WithDescription("Extract text from the body or store HTML as is. Default = true");

            p.SetupHelp("?", "h", "help").Callback(text => Console.Error.WriteLine(text));

            ICommandLineParserResult result = p.Parse(args);
            if (result.HasErrors)
            {
                Console.Error.WriteLine(result.ErrorText);
                Console.Error.WriteLine("Usage: ");
                p.HelpOption.ShowHelp(p.Options);
                return;
            }
            if (result.HelpCalled)
            {
                return;
            }

            Arguments arguments = p.Object;

            var indexer = new AzureSearchIndexer(arguments.ServiceEndPoint, arguments.IndexName, arguments.AdminApiKey, arguments.ExtractText, new TextExtractor());
            var crawler = new Crawler(indexer);
            crawler.Crawl(arguments.RootUri, maxPages: arguments.MaxPagesToIndex, maxDepth: arguments.MaxCrawlDepth).Wait();

        }
    }
}
