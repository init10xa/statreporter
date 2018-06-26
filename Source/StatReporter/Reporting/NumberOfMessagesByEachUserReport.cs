using StatReporter.Types;
using System;
using System.Collections.Generic;

namespace StatReporter.Reporting
{
    public class NumberOfMessagesByEachUserReport : ReportGeneratorBase
    {
        private static readonly string SinceCreation = "Since chat started";

        private static readonly string ToDate = "To date";

        private List<string[]> countingResult;

        private List<string[]> usersInfo;

        private Dictionary<string, ReportItem> usersMessages;

        public NumberOfMessagesByEachUserReport(MessageMetaData[] messages) : base(messages)
        {
            countingResult = new List<string[]>();
            usersInfo = new List<string[]>();
            usersMessages = new Dictionary<string, ReportItem>();
        }

        protected override Report Generate()
        {
            CountMessages();
            var mainSection = CreateMainSection();
            PrepareUsersInfo();
            var usersSection = CreateUserSection();

            var report = new Report
            {
                Title = "NumberOfMessagesByEachUser"
            };

            report.AddSection(mainSection);
            report.AddSection(usersSection);

            return report;
        }

        private void CountMessages()
        {
            int counter;
            User user;
            string userName;
            ReportItem item;

            for (int i = 0; i < messages.Length; i++)
            {
                user = messages[i].Sender;
                userName = user.Name;

                if (usersMessages.TryGetValue(userName, out item))
                    item.Counter++;
                else
                    usersMessages.Add(userName, new ReportItem(user, 1));
            }

            foreach (var record in usersMessages)
                countingResult.Add(new string[] { record.Key, record.Value.Counter.ToString() });
        }

        private ReportSection CreateMainSection()
        {
            var section = new ReportSection()
            {
                Headings = new string[] { "User's Name", "Number of Messages" },
                Content = countingResult.ToArray()
            };

            return section;
        }

        private ReportSection CreateUserSection()
        {
            return new ReportSection()
            {
                Headings = new string[] { "User's Name", "Joined", "Left/Removed" },
                Content = usersInfo
            };
        }

        private void PrepareUsersInfo()
        {
            usersInfo = new List<string[]>();
            User user;
            string userName;
            string joinedDate;
            string leftDate;

            foreach (var record in usersMessages)
            {
                user = record.Value.User;

                for (int i = 0; i < user.JoinedDates.Length; i++)
                {
                    if (user.JoinedDates[i] == DateTime.MinValue)
                        joinedDate = SinceCreation;
                    else
                        joinedDate = user.JoinedDates[0].ToShortDateString();

                    if (user.LeftDates[i] == DateTime.MaxValue)
                        leftDate = ToDate;
                    else
                        leftDate = user.LeftDates[i].ToShortDateString();

                    if (i == 0)
                        userName = user.Name;
                    else
                        userName = string.Empty;

                    usersInfo.Add(new string[] { userName, joinedDate, leftDate });
                }
            }
        }

        private class ReportItem
        {
            public ReportItem(User user, int counter)
            {
                User = user;
                Counter = counter;
            }

            public int Counter { get; set; }

            public User User { get; set; }
        }
    }
}