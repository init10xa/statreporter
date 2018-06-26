using StatReporter.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace StatReporter.Reporting
{
    public class UserContributionByMonthReport : ReportGeneratorBase
    {
        private static readonly char Delimiter = '/';
        private Dictionary<string, int> contrinutions;
        private List<string[]> reportContent;
        private User user;
        private string userName;

        public UserContributionByMonthReport(MessageMetaData[] messages, string userName) : base(messages)
        {
            contrinutions = new Dictionary<string, int>();

            Debug.Assert(!string.IsNullOrWhiteSpace(userName),
                "!string.IsNullOrWhiteSpace(userName) - user name expected to be non-empty.");
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("userName argument cannot be null.");

            this.userName = userName;
        }

        protected override Report Generate()
        {
            CountMessages();
            var mainSection = CreateMainSection();
            var userInfoSection = CreateUserInfoSection();

            var report = new Report
            {
                Title = "UserContributionByMonth"
            };

            report.AddSection(mainSection);
            report.AddSection(userInfoSection);

            return report;
        }

        private void CountMessages()
        {
            int messagesCount = messages.Length;
            string key = null;
            int counter;

            for (int i = 0; i < messagesCount; i++)
            {
                if (messages[i].Sender.Name == userName)
                {
                    if (user == null)
                        user = messages[i].Sender;

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
        }

        private string CreateKey(DateTime date)
        {
            return $"{date.Year}{Delimiter}{date.Month}";
        }

        private ReportSection CreateMainSection()
        {
            reportContent = new List<string[]>();
            string[] dateComponents;
            int month;
            int year;
            string monthName;
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

        private ReportSection CreateUserInfoSection()
        {
            IEnumerable<string[]> info;

            if (user != null)
                info = user.GetFormattedUserInfo();
            else
                info = new string[1][] { new string[] { userName, "The user was not found!" } };

            return new ReportSection()
            {
                Headings = new string[] { "User's Name", "Number of Messages" },
                Content = info
            };
        }

        private string GetFormattedDate(string key)
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