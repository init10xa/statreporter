using StatReporter.Types;
using System.Collections.Generic;

namespace StatReporter.Reporting
{
    public class NumberOfMessagesByEachUserReport : ReportGeneratorBase
    {
        private Dictionary<string, int> userMessages = new Dictionary<string, int>();

        public NumberOfMessagesByEachUserReport(MessageMetaData[] messages) : base(messages)
        {
        }

        protected override Report Generate()
        {
            int counter;
            string name;

            for (int i = 0; i < messages.Length; i++)
            {
                name = messages[i].Sender.Name;

                if (userMessages.TryGetValue(name, out counter))
                {
                    counter++;
                    userMessages[name] = counter;
                }
                else
                {
                    userMessages.Add(name, 1);
                }
            }

            List<string[]> result = new List<string[]>();

            foreach (var record in userMessages)
                result.Add(new string[] { record.Key, record.Value.ToString() });

            var report = new Report();
            report.Content = result.ToArray();
            report.ContentType = ContentType.CSV;
            report.Title = "NumberOfMessagesByEachUser";

            return report;
        }
    }
}