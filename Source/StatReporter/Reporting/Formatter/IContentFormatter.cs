
namespace StatReporter.Reporting.Formatter
{
    public interface IContentFormatter
    {
        ContentType ContentType { get; }

        string ContentFileExtension { get; }

        void AddToContent(params object[] items);

        object GetFormattedContent();
    }
}