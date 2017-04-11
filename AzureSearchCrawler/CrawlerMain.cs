using System;
using Fclp;

namespace AzureSearchCrawler
{
    /// <summary>
    /// The entry point of the crawler. Adjust the constants at the top and run.
    /// </summary>
    class CrawlerMain
    {
        private const int DefaultMaxPagesToIndex = 100;

        private class Arguments
        {
            public string RootUri { get; set; }

            public int MaxPagesToIndex { get; set; }

            public string ServiceName { get; set; }

            public string IndexName { get; set; }

            public string AdminApiKey { get; set; }
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

            p.Setup(arg => arg.ServiceName)
                .As('s', "ServiceName")
                .Required()
                .WithDescription("The name of your Azure Search service");

            p.Setup(arg => arg.IndexName)
                .As('i', "IndexName")
                .Required()
                .WithDescription("The name of the index in your Azure Search service");

            p.Setup(arg => arg.AdminApiKey)
                .As('a', "AdminApiKey")
                .Required();

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

            var indexer = new AzureSearchIndexer(arguments.ServiceName, arguments.IndexName, arguments.AdminApiKey, new TextExtractor());
            var crawler = new Crawler(indexer);
            crawler.Crawl(arguments.RootUri, maxPages: arguments.MaxPagesToIndex).Wait();

            Console.Read(); // keep console open until a button is pressed so we see the output
        }
    }
}
