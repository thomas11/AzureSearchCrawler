using Abot2.Poco;
using Azure;
using Azure.Core.Serialization;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;


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

        private readonly TextExtractor _textExtractor;
        private readonly SearchClient _indexClient;

        private readonly BlockingCollection<WebPage> _queue = [];
        private readonly SemaphoreSlim indexingLock = new(1, 1);


        public AzureSearchIndexer(string serviceEndPoint, string indexName, string adminApiKey, TextExtractor textExtractor)
        {
            _textExtractor = textExtractor;

            // Create serializer options to convert to camelCase
            JsonSerializerOptions serializerOptions = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            SearchClientOptions clientOptions = new()
            {
                Serializer = new JsonObjectSerializer(serializerOptions)
            };

            // Create a client
            Uri endpoint = new(serviceEndPoint);
            AzureKeyCredential credential = new(adminApiKey);
            _indexClient = new SearchClient(endpoint, indexName, credential, clientOptions);

        }

        public async Task PageCrawledAsync(CrawledPage crawledPage)
        {
            string text = _textExtractor.ExtractText(crawledPage.Content.Text);
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

        private async Task<IndexDocumentsResult> IndexBatchIfNecessary()
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
                return await _indexClient.MergeOrUploadDocumentsAsync(pages);
            }
            finally
            {
                indexingLock.Release();
            }
        }

        public class WebPage(string url, string content)
        {
            public string Id { get; } = url.GetHashCode().ToString();

            public string Url { get; } = url;

            public string Content { get; } = content;
        }
    }
}
