using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StatReporter.Reporting
{
    public class Report
    {
        protected List<ReportSection> sections;

        public Report()
        {
            sections = new List<ReportSection>();
        }

        public IEnumerable<ReportSection> Sections
        {
            get { return sections; }
        }

        public string Title { get; set; }

        public void AddSection(ReportSection section)
        {
            Debug.Assert(section != null, "section != null - section should not be null");
            if (section == null)
                throw new ArgumentNullException("section cannot be null.");

            sections.Add(section);
        }
    }
}