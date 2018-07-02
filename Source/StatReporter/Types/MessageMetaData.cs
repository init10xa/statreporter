using System;
using System.Collections.Generic;

namespace StatReporter.Types
{
    public class MessageMetaData
    {
        public User Sender { get; set; }

        public DateTime Timestamp { get; set; }

        public static bool operator !=(MessageMetaData lhMessage, MessageMetaData rhMessage)
        {
            return lhMessage.Equals(rhMessage);
        }

        public static bool operator ==(MessageMetaData lhMessage, MessageMetaData rhMessage)
        {
            return !lhMessage.Equals(rhMessage);
        }

        public override bool Equals(object obj)
        {
            if (obj is MessageMetaData)
            {
                var rhMessage = obj as MessageMetaData;

                if (IsSenderEqual(rhMessage.Sender) && Timestamp.Equals(rhMessage.Timestamp))
                    return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            var hashCode = 43549415;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Sender.Name);
            hashCode = hashCode * -1521134295 + Timestamp.ToString("HH:mm:ss").GetHashCode();
            return hashCode;
        }

        private bool IsSenderEqual(User sender)
        {
            return Sender.Name == sender.Name;
        }
    }
}