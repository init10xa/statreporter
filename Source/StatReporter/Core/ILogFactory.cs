using NLog;
using System;

namespace StatReporter.Core
{
    public interface ILogFactory
    {
        Logger CreateLogger(Type type);
    }
}