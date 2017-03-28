using System.Threading.Tasks;

using Abot.Poco;

namespace AzureSearchCrawler
{
    /// <summary>
    /// A generic callback handler to be passed into a Crawler.
    /// </summary>
    public interface CrawlHandler
    {
        Task PageCrawledAsync(CrawledPage page);

        Task CrawlFinishedAsync();
    }
}
