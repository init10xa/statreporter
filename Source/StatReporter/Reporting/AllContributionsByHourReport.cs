using StatReporter.Types;
using System;

namespace StatReporter.Reporting
{
    public class AllContributionsByHourReport : ContributionsByHourBase
    {
        public AllContributionsByHourReport(MessageMetaData[] messages) : base(messages)
        {
            // do nothing!
        }

        protected override void CountMessages()
        {
            int messagesCount = messages.Length;
            DateTime key;
            int counter;

            for (int i = 0; i < messagesCount; i++)
            {
                key = CreateKey(messages[i].Timestamp);

                if (contrinutions.ContainsKey(key))
                {
                    counter = contrinutions[key];
                    counter++;
                    contrinutions[key] = counter;
                }
                else
                {
                    contrinutions.Add(key, 1);
                }
            }
        }

        protected override Report Generate()
        {
            CountMessages();
            var mainSection = CreateMainSection();

            var report = new Report
            {
                Title = "AllContributionsByMonth"
            };

            report.AddSection(mainSection);

            return report;
        }
    }
}