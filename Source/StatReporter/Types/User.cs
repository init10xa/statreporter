using System;

namespace StatReporter.Types
{
    public class User
    {
        public User()
        {
            JoinedOn = DateTime.MinValue;
            LeftOn = DateTime.MaxValue;
        }

        public string Name { get; set; }

        public DateTime JoinedOn { get; set; }

        public DateTime LeftOn { get; set; }
    }
}