using System;
using System.Collections.Generic;

namespace StatReporter.Types
{
    public class User
    {
        private static readonly string SinceCreation = "Since chat started";
        private static readonly string ToDate = "To date";
        private List<DateTime> joinedDates;
        private List<DateTime> leftDates;

        public User()
        {
            joinedDates = new List<DateTime>();
            leftDates = new List<DateTime>();
            AddAJoinDate(DateTime.MinValue);
        }

        public DateTime[] JoinedDates
        {
            get { return joinedDates.ToArray(); }
        }

        public DateTime[] LeftDates
        {
            get { return leftDates.ToArray(); }
        }

        public string Name { get; set; }

        public void AddAJoinDate(DateTime joinedOn)
        {
            if (joinedDates.Count == 1 && joinedDates[0] == DateTime.MinValue)
            {
                joinedDates[0] = joinedOn;
            }
            else
            {
                joinedDates.Add(joinedOn);
                leftDates.Add(DateTime.MaxValue);
            }
        }

        public void AddLeftDate(DateTime leftOn)
        {
            int lastValidIndex = joinedDates.Count - 1;
            leftDates[lastValidIndex] = leftOn;
        }

        public IEnumerable<string[]> GetFormattedUserInfo()
        {
            var userInfo = new List<string[]>();
            string userName;
            string joinedDate;
            string leftDate;

            for (int i = 0; i < joinedDates.Count; i++)
            {
                if (joinedDates[i] == DateTime.MinValue)
                    joinedDate = SinceCreation;
                else
                    joinedDate = joinedDates[0].ToShortDateString();

                if (leftDates[i] == DateTime.MaxValue)
                    leftDate = ToDate;
                else
                    leftDate = leftDates[i].ToShortDateString();

                if (i == 0)
                    userName = Name;
                else
                    userName = string.Empty;

                userInfo.Add(new string[] { userName, joinedDate, leftDate });
            }

            return userInfo;
        }
    }
}