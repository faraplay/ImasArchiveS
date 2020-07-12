using ImasArchiveLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ImasArchiveApp
{
    class ParModel : ContainerFileModel
    {
        #region Fields
        private readonly ParFile _parFile;

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
        public ParModel(IReport parent, Stream stream, string fileName) : base(parent, fileName)
        {
            try
            {
                _parFile = new ParFile(stream);
                List<string> browserEntries = new List<string>();
                foreach (ParEntry entry in _parFile.Entries)
                {
                    browserEntries.Add(entry.Name);
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
                _parFile?.Dispose();
            }
            disposed = true;
            base.Dispose(disposing);
        }
        #endregion
        public override Task LoadChildFileModel(string fileName)
        {
            ClearStatus();
            if (fileName != null)
            {
                try
                {
                    ReportMessage("Loading " + fileName);
                    FileModel = FileModelFactory.CreateFileModel(_parFile.GetEntry(fileName).Open(), fileName);
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
            return Task.CompletedTask;
        }
    }
}
