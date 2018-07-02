using StatReporter.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StatReporter.Reporting
{
    public class UserContributionByHourReport : ContributionsByHourBase
    {
        private User user;
        private string userName;

        public UserContributionByHourReport(MessageMetaData[] messages, string userName) : base(messages)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(userName),
                "!string.IsNullOrWhiteSpace(userName) - user name expected to be non-empty.");
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("userName argument cannot be null.");

            this.userName = userName;
        }

        protected override void CountMessages()
        {
            int messagesCount = messages.Length;
            DateTime key;
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
    }
}