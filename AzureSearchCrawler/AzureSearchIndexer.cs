using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Abot.Poco;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;

namespace AzureSearchCrawler
{
    /// <summary>
    /// A CrawlHandler that indexes crawled pages into Azure Search. Pages are represented by the nested WebPage class.
    /// <para/>To customize what text is extracted and indexed from each page, you implement a custom TextExtractor
    /// and pass it in.
    /// </summary>
    class AzureSearchIndexer : CrawlHandler
    {
        private const int IndexingBatchSize = 25;

        private TextExtractor _textExtractor;
        private ISearchIndexClient _indexClient;

        private BlockingCollection<WebPage> _queue = new BlockingCollection<WebPage>();
        private SemaphoreSlim indexingLock = new SemaphoreSlim(1, 1);

        public AzureSearchIndexer(string serviceName, string indexName, string adminApiKey, TextExtractor textExtractor)
        {
            _textExtractor = textExtractor;

            SearchServiceClient serviceClient = new SearchServiceClient(serviceName, new SearchCredentials(adminApiKey));
            _indexClient = serviceClient.Indexes.GetClient(indexName);
        }

        public async Task PageCrawledAsync(CrawledPage crawledPage)
        {
            string text = _textExtractor.ExtractText(crawledPage.HtmlDocument);
            if (text == null)
            {
                Console.WriteLine("No content for page {0}", crawledPage?.Uri.AbsoluteUri);
                return;
            }

            _queue.Add(new WebPage(crawledPage.Uri.AbsoluteUri, text));

            if (_queue.Count > IndexingBatchSize)
            {
                await IndexBatchIfNecessary();
            }
        }

        public async Task CrawlFinishedAsync()
        {
            await IndexBatchIfNecessary();

            // sanity check
            if (_queue.Count > 0)
            {
                Console.WriteLine("Error: indexing queue is still not empty at the end.");
            }
        }

        private async Task<DocumentIndexResult> IndexBatchIfNecessary()
        {
            await indexingLock.WaitAsync();

            if (_queue.Count == 0)
            {
                return null;
            }

            int batchSize = Math.Min(_queue.Count, IndexingBatchSize);
            Console.WriteLine("Indexing batch of {0}", batchSize);

            try
            {
                var pages = new List<WebPage>(batchSize);
                for (int i = 0; i < batchSize; i++)
                {
                    pages.Add(_queue.Take());
                }
                var batch = IndexBatch.MergeOrUpload(pages);
                return await _indexClient.Documents.IndexAsync(batch);
            }
            finally
            {
                indexingLock.Release();
            }
        }

        [SerializePropertyNamesAsCamelCase]
        public class WebPage
        {
            public WebPage(string url, string content)
            {
                Url = url;
                Content = content;
                Id = url.GetHashCode().ToString();
            }

            public string Id { get; }

            public string Url { get; }

            public string Content { get; }
        }
    }
}
