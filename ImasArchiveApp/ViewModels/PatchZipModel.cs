using Imas.Archive;

namespace ImasArchiveApp
{
    class PatchZipModel : ContainerFileModel
    {
        #region Fields
        private readonly PatchZipFile _patchZipFile;
        #endregion
        #region Properties
        protected override IContainerFile ContainerFile => _patchZipFile;
        #endregion
        #region Constructors
        public PatchZipModel(IReport parent, string inPath, IGetFileName getFileName)
            : base(parent, inPath.Substring(inPath.LastIndexOf('\\') + 1), getFileName)
        {
            try
            {
                _patchZipFile = new PatchZipFile(inPath, PatchZipMode.Update);
                SetBrowserEntries();
            }
            catch
            {
                Dispose();
                throw;
            }
        }
        #endregion
        #region IDisposable
        bool disposed = false;
        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                _patchZipFile?.Dispose();
            }
            disposed = true;
            base.Dispose(disposing);
        }
        #endregion
    }
}
