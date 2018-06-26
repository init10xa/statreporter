using System.Collections.Generic;

namespace StatReporter.Reporting
{
    public class ReportSection
    {
        public IEnumerable<string[]> Content { get; set; }

        public string[] Headings { get; set; }
    }
}