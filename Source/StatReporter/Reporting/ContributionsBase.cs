using StatReporter.Types;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace StatReporter.Reporting
{
    public abstract class ContributionsBase : ReportGeneratorBase
    {
        protected Dictionary<DateTime, int> contrinutions;
        protected List<string[]> reportContent;

        public ContributionsBase(MessageMetaData[] messages) : base(messages)
        {
            contrinutions = new Dictionary<DateTime, int>();
            reportContent = new List<string[]>();
        }

        protected abstract void CountMessages();

        protected abstract DateTime CreateKey(DateTime date);

        protected abstract ReportSection CreateMainSection();
    }
}