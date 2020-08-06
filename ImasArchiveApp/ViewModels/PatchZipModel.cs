using Imas.Archive;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ImasArchiveApp
{
    class PatchZipModel : ContainerFileModel
    {
        #region Fields
        private readonly PatchZipFile _patchZipFile;

        private IFileModel _fileModel;
        #endregion
        #region Properties
        public override IFileModel FileModel
        {
            get => _fileModel;
            set
            {
                _fileModel?.Dispose();
                _fileModel = value;
                OnPropertyChanged();
            }
        }

        #endregion
        #region Constructors
        public PatchZipModel(IReport parent, string inPath, IGetFileName getFileName) : base(parent, inPath.Substring(inPath.LastIndexOf('\\') + 1))
        {
            try
            {
                ClearStatus();
                _patchZipFile = new PatchZipFile(inPath, PatchZipMode.Update);
                List<string> browserEntries = new List<string>();
                foreach (PatchZipEntry entry in _patchZipFile.Entries)
                {
                    browserEntries.Add(entry.FileName);
                }
                BrowserModel = new BrowserModel(this, new BrowserTree("", browserEntries));
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
        public override async Task LoadChildFileModel(string fileName)
        {
            ClearStatus();
            if (fileName != null)
            {
                try
                {
                    ReportMessage("Loading " + fileName);
                    FileModel = FileModelFactory.CreateFileModel(await _patchZipFile.GetEntry(fileName).GetData(), fileName);
                    ReportMessage("Loaded.");
                }
                catch (Exception ex)
                {
                    ReportException(ex);
                    FileModel = null;
                }
            }
            else
            {
                FileModel = null;
            }

        }
    }
}
