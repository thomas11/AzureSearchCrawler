# About

[Azure Search](https://azure.microsoft.com/en-us/services/search/) is a cloud search service for web and mobile app development. This project helps you get content from a website into an Azure Search index. It uses [Abot](https://github.com/sjdirect/abot) to crawl websites. For each page it extracts the content in a customizable way and indexes it into Azure Search.

This project is intended as a demo or a starting point for a real crawler. At a minimum, you'll want to replace the console messages with proper logging, and customize the text extraction to improve results for your use case.


# Howto: quick start

- Create an Azure Search search service. If you're new to Azure Search, follow [this guide](https://docs.microsoft.com/en-us/azure/search/search-create-service-portal).
- Create an index in your search service with three string fields: "id", "url", and "content". Make them searchable.
- At the top of CrawlerMain.cs, fill in your search service information and the root URL of the site you'd like to crawl.
- Run CrawlerMain, either from Visual Studio after opening the .sln file, or from the command line after compiling using msbuild.


# Howto: customize it for your project

## Text extraction

To adjust what content is extracted and indexed from each page, implement your own TextExtractor subclass. See the class documentation for more information.

## CrawlerConfig

The Abot crawler is configured by the method Crawler.CreateCrawlConfiguration, which you can adjust to your liking.


# Code overview

- CrawlerMain contains the setup information such as the Azure Search service information, and the main method that runs the crawler.
- The Crawler class uses Abot to crawl the given website, based off of the Abot sample. It uses a passed-in CrawlHandler to process each page it finds.
- CrawlHandler is a simple interface decoupling crawling from processing each page.
- AzureSearchIndexer is a CrawlHandler that indexes page content into AzureSearch.
- Pages are modeled by the inner class AzureSearchIndexer.WebPage. The schema of your Azure Search index must match this class.
