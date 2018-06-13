
using System.Text;

namespace StatReporter.Reporting.Formatter
{
    public class CsvFormatter : IContentFormatter
    {
        private StringBuilder content;

        public CsvFormatter()
        {
            content = new StringBuilder();
        }

        public ContentType ContentType
        {
            get { return ContentType.CSV; }
        }

        public string ContentFileExtension
        {
            get { return "txt"; }
        }

        public void AddToContent(params object[] items)
        {
            for (int i = 0; i < items.Length - 1; i++)
                content.Append($"{items[i]}, ");

            string lastItem = items[items.Length - 1].ToString();
            content.AppendLine(lastItem);
        }

        public object GetFormattedContent()
        {
            return content.ToString();
        }
    }
}