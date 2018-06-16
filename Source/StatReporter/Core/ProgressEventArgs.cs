
using System;

namespace StatReporter.Core
{
    public class ProgressEventArgs : EventArgs
    {
        public ProgressEventArgs(float progress)
        {
            Progress = progress;
        }

        public ProgressEventArgs(float progress, string message)
        {
            Progress = progress;
            Message = message;
        }

        public string Message { get; protected set; }

        public float Progress { get; protected set; }
    }
}