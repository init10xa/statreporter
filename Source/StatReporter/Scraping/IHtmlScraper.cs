using StatReporter.Types;

namespace StatReporter.Scraping
{
    public interface IHtmlScraper
    {
        MessageMetaData[] Scrape();
    }
}