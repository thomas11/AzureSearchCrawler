using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AzureSearchCrawler
{
    partial class AzureSearchIndexer
    {
        private static string CreateSHA512(string strData)
        {
            var message = Encoding.UTF8.GetBytes(strData);
            var hashValue = SHA512.HashData(message);
            return hashValue.Aggregate("", (current, x) => current + $"{x:x2}");
        }
        /// <summary>
        /// Web page that is defined in the Azure AI Search Index
        /// </summary>
        /// <param name="url">Url</param>
        /// <param name="title">Title</param>
        /// <param name="content">Content</param>
        public class WebPage(string url, string title, string content)
        {
            public string Id { get; } = CreateSHA512(url);

            public string Url { get; } = url;
            public string Title { get; } = title;

            public string Content { get; } = content;
        }
    }
}
