using System;

namespace StatReporter.Core
{
    public interface IProgressable
    {
        event EventHandler Completed;

        event EventHandler<ProgressEventArgs> ProgressChanged;
    }
}