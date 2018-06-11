using System;

namespace StatReporter.Types
{
    public class MessageMetaData
    {
        public DateTime Timestamp { get; set; }

        public User Sender { get; set; }
    }
}