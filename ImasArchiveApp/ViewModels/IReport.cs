using Imas;
using System;
using System.Collections.Generic;
using System.Text;

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
