using Max2Babylon;
using System.Drawing;

namespace Utilities
{
    public interface ILoggingProvider
    {
        LogLevel LoggerLevel { get; set; }
        void ReportProgressChanged(int progress);

        void RaiseCriticalError(string error, int rank = 0);

        void RaiseError(string error, int rank = 0);

        void RaiseWarning(string warning, int rank = 0);

        void RaiseMessage(string message, int rank = 0, bool emphasis = false);

        void RaiseMessage(string message, Color color, int rank = 0, bool emphasis = false);

        void RaiseVerbose(string message, int rank = 0, bool emphasis = false);
        
        void Print(string message, Color color, int rank = 0, bool emphasis = false);

        void CheckCancelled(BabylonExporter exporter);
    }
}
