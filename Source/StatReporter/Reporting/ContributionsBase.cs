using StatReporter.Types;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace StatReporter.Reporting
{
    public abstract class ContributionsBase : ReportGeneratorBase
    {
        protected Dictionary<string, int> contrinutions;
        protected List<string[]> reportContent;

        public ContributionsBase(MessageMetaData[] messages) : base(messages)
        {
            contrinutions = new Dictionary<string, int>();
            reportContent = new List<string[]>();
        }

        protected abstract void CountMessages();

        protected abstract string CreateKey(DateTime date);

        protected abstract ReportSection CreateMainSection();
    }
}