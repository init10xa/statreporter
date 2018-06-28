using StatReporter.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StatReporter.Reporting
{
    public class NumberOfMessagesByEachUserReport : ReportGeneratorBase
    {
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

            var sortedMessages = usersMessages.OrderBy(record => record.Key);

            foreach (var record in sortedMessages)
                countingResult.Add(new string[] { record.Key, record.Value.Counter.ToString() });
        }

        private ReportSection CreateMainSection()
        {
            var section = new ReportSection()
            {
                Headings = new string[] { "User's Name", "Number of Messages" },
                Content = countingResult
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
            IEnumerable<string[]> info;
            User user;

            foreach (var record in usersMessages)
            {
                user = record.Value.User;
                info = user.GetFormattedUserInfo();

                foreach (var item in info)
                    usersInfo.Add(item);
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