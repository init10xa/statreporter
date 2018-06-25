using StatReporter.Types;
using System.Threading.Tasks;

namespace StatReporter.Reporting
{
    public interface IReportGenerator
    {
        string ReportName { get; }

        MessageMetaData[] Messages { get; }

        Task<Report> GenerateAsync();
    }
}