using Imas.Archive;
using System.IO;

namespace ImasArchiveApp
{
    class ParModel : ContainerFileModel
    {
        #region Fields
        private readonly ParFile _parFile;
        #endregion
        #region Properties
        protected override IContainerFile ContainerFile => _parFile;
        #endregion
        #region Constructors
        public ParModel(IReport parent, Stream stream, string fileName, IGetFileName getFileName)
            : base(parent, fileName, getFileName)
        {
            try
            {
                _parFile = new ParFile(stream);
                SetBrowserEntries();
            }
            catch
            {
                Dispose();
                throw;
            }
        }
        internal static FileModelFactory.FileModelBuilder Builder { get; set; } = 
            (report, filename, getFilename, stream) => new ParModel(report, stream, filename, getFilename);
        #endregion
        #region IDisposable
        bool disposed = false;
        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                _parFile?.Dispose();
            }
            disposed = true;
            base.Dispose(disposing);
        }
        #endregion
    }
}
