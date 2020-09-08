using Imas;
using System;

namespace ImasArchiveApp
{
    public interface IReport
    {
        public Action ClearStatus { get; }
        public Action<ProgressData> ReportProgress { get; }
        public Action<string> ReportMessage { get; }
        public Action<Exception> ReportException { get; }
    }
}