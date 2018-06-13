using StatReporter.Types;

namespace StatReporter.Reporting
{
    public interface IReportGenerator
    {
        string ReportName { get; }

        MessageMetaData[] Messages { get; set; }

        Report Generate();
    }
}