
using StatReporter.Types;
using System.Threading.Tasks;

namespace StatReporter.Reporting
{
    public abstract class ReportGeneratorBase : IReportGenerator
    {
        protected MessageMetaData[] messages;

        protected ReportGeneratorBase(MessageMetaData[] messages)
        {
            this.messages = messages;
        }

        public virtual MessageMetaData[] Messages
        {
            get { return messages; }
        }

        public virtual string ReportName { get; protected set; }

        public virtual async Task<Report> GenerateAsync()
        {
            var report = await Task.Factory.StartNew(Generate);
            return report;
        }

        protected abstract Report Generate();
    }
}