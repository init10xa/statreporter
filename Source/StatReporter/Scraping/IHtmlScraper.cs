using StatReporter.Core;
using StatReporter.Types;

namespace StatReporter.Scraping
{
    public interface IHtmlScraper : IProgressable
    {
        MessageMetaData[] Scrape();
    }
}