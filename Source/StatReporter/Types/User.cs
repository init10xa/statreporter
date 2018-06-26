using System;
using System.Collections.Generic;

namespace StatReporter.Types
{
    public class User
    {
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
    }
}