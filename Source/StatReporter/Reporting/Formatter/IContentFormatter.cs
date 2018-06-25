
namespace StatReporter.Reporting.Formatter
{
    public interface IContentFormatter
    {
        ContentType ContentType { get; }

        void AddRecord(params string[] items);

        string GetFormattedContent();
    }
}