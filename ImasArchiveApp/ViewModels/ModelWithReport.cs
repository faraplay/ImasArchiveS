using ImasArchiveLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImasArchiveApp
{
    abstract class ModelWithReport
    {
        public Action ClearStatus;

        public Action<ProgressData> ReportProgress;
        public Action<string> ReportMessage;

        public Action<Exception> ReportException;
    }
}
