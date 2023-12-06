using Abot2.Poco;
using System.Threading.Tasks;

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
