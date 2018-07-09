
namespace StatReporter.Reporting
{
    public interface IReportManager
    {
        Report[] Results { get; }

        void Start();
    }
}