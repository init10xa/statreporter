using System.Collections.Generic;

namespace StatReporter.Reporting
{
    public class Report
    {
        public IEnumerable<string[]> Content { get; set; }

        public ContentType ContentType { get; set; }

        public string Title { get; set; }
    }
}