using System;
using System.Collections.Generic;

namespace StatReporter.Types
{
    public class User
    {
        private List<DateTime> joinedDates;
        private List<DateTime> leftDates;

        public string Name { get; set; }

        public DateTime[] JoinedDates
        {
            get { return joinedDates.ToArray(); }
        }

        public DateTime[] LeftDates
        {
            get { return leftDates.ToArray(); }
        }

        public void AddAJoinDate(DateTime joinedOn)
        {
            if (joinedDates == null)
                joinedDates = new List<DateTime>();

            joinedDates.Add(joinedOn);
        }

        public void AddLeftDate(DateTime leftOn)
        {
            if (leftDates == null)
                leftDates = new List<DateTime>();

            leftDates.Add(leftOn);
        }
    }
}