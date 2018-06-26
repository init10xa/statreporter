using StatReporter.Types;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace StatReporter.Reporting
{
    public abstract class ContributionsByMonthBase : ContributionsBase
    {
        protected static readonly char Delimiter = '/';

        public ContributionsByMonthBase(MessageMetaData[] messages) : base(messages)
        {
            contrinutions = new Dictionary<string, int>();
        }

        protected override string CreateKey(DateTime date)
        {
            return $"{date.Year}{Delimiter}{date.Month}";
        }

        protected override ReportSection CreateMainSection()
        {
            reportContent = new List<string[]>();
            string date;

            foreach (var record in contrinutions)
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

        protected string GetFormattedDate(string key)
        {
            var dateComponents = key.Split(Delimiter);
            var year = Convert.ToInt32(dateComponents[0]);
            var month = Convert.ToInt32(dateComponents[1]);
            var dummyDate = new DateTime(year, month, 1);
            var monthName = dummyDate.ToString("MMM.", CultureInfo.InvariantCulture);
            var formattedDate = $"{monthName} {year}";

            return formattedDate;
        }
    }
}