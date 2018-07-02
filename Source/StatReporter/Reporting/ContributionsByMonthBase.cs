using StatReporter.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace StatReporter.Reporting
{
    public abstract class ContributionsByMonthBase : ContributionsBase
    {
        protected static readonly char Delimiter = '/';

        public ContributionsByMonthBase(MessageMetaData[] messages) : base(messages)
        {
            // do nothing!
        }

        protected override DateTime CreateKey(DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        protected override ReportSection CreateMainSection()
        {
            string date;

            var sortedContributions = contrinutions.OrderBy(record => record.Key);

            foreach (var record in sortedContributions)
            {
                date = GetFormattedDate(record.Key);
                reportContent.Add(new string[] { date, record.Value.ToString() });
            }

            var mainSection = new ReportSection()
            {
                Headings = new string[] { "Month", "Number of Messages" },
                Content = reportContent
            };

            return mainSection;
        }

        protected string GetFormattedDate(DateTime date)
        {
            var formattedDate = date.ToString("MMM. yyyy");
            return formattedDate;
        }
    }
}