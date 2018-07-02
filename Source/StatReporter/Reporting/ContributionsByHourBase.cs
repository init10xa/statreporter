using StatReporter.Types;
using System;
using System.Globalization;
using System.Linq;

namespace StatReporter.Reporting
{
    public abstract class ContributionsByHourBase : ContributionsBase
    {
        public ContributionsByHourBase(MessageMetaData[] messages) : base(messages)
        {
            // do nothing!
        }

        protected override DateTime CreateKey(DateTime date)
        {
            return new DateTime(1, 1, 1, date.Hour, 0, 0);
        }

        protected override ReportSection CreateMainSection()
        {
            var sortedContributions = contrinutions.OrderBy(item => item.Key);

            foreach (var record in sortedContributions)
                reportContent.Add(new string[] { record.Key.ToString("HH"), record.Value.ToString() });

            var mainSection = new ReportSection()
            {
                Headings = new string[] { "Hour", "Number of Messages" },
                Content = reportContent
            };

            return mainSection;
        }
    }
}