using System;
using NLog;

namespace StatReporter.Core
{
    public class LogFactory : ILogFactory
    {
        public Logger CreateLogger(Type type)
        {
            return LogManager.GetLogger(type.FullName);
        }
    }
}