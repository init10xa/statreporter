using System.Text;

namespace StatReporter.Reporting.Formatter
{
    public class CsvFormatter : IContentFormatter
    {
        private StringBuilder content;
        private string delimiter;
        private DelimiterType delimiterType;

        public CsvFormatter()
        {
            content = new StringBuilder();
            Delimiter = DelimiterType.Comma;
        }

        public ContentType ContentType
        {
            get { return ContentType.CSV; }
        }

        public DelimiterType Delimiter
        {
            get { return delimiterType; }
            set
            {
                delimiterType = value;
                SetDelimiter();
            }
        }

        public void AddRecord(params string[] items)
        {
            for (int i = 0; i < items.Length - 1; i++)
            {
                content.Append(items[i]);
                content.Append(delimiter);
            }

            string lastItem = items[items.Length - 1];
            content.AppendLine(lastItem);
        }

        public string GetFormattedContent()
        {
            return content.ToString();
        }

        private void SetDelimiter()
        {
            switch (this.delimiterType)
            {
                case DelimiterType.Colon:
                    delimiter = $": ";
                    break;

                case DelimiterType.Comma:
                    delimiter = $", ";
                    break;

                case DelimiterType.Pipe:
                    delimiter = $"| ";
                    break;

                case DelimiterType.SemiColon:
                    delimiter = $"; ";
                    break;

                case DelimiterType.Tab:
                    delimiter = $"\t";
                    break;
            }
        }
    }
}