using Imas.Archive;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ImasArchiveApp
{
    public abstract class ContainerFileModel : FileModel, IDisposable
    {
        #region Fields

        private BrowserModel _browserModel;
        protected IFileModel _fileModel;
        protected readonly IGetFileName _getFileName;

        #endregion Fields

        #region Properties

        protected abstract IContainerFile ContainerFile { get; }

        public BrowserModel BrowserModel
        {
            get => _browserModel;
            set
            {
                _browserModel = value;
                OnPropertyChanged();
            }
        }

        public IFileModel FileModel
        {
            get => _fileModel;
            set
            {
                _fileModel?.Dispose();
                _fileModel = value;
                OnPropertyChanged();
            }
        }

        public string CurrentFile
        {
            get => FileModel.FileName;
        }

        #endregion Properties

        #region Constructors

        protected ContainerFileModel(IReport parent, string fileName, IGetFileName getFileName) : base(parent, fileName)
        {
            _getFileName = getFileName;
        }

        #endregion Constructors

        protected void SetBrowserEntries()
        {
            List<string> browserEntries = new List<string>();
            foreach (ContainerEntry entry in ContainerFile.Entries)
            {
                browserEntries.Add(entry.FileName);
            }
            BrowserModel = new BrowserModel(this, new BrowserTree("", browserEntries));
        }

        public async Task LoadChildFileModel(string fileName)
        {
            ClearStatus();
            if (fileName != null)
            {
                ContainerEntry entry = ContainerFile.GetEntry(fileName);
                if (entry != null)
                {
                    try
                    {
                        ReportMessage("Loading " + fileName);
                        FileModel = FileModelFactory.CreateFileModel(await entry.GetData(), fileName);
                        ReportMessage("Loaded.");
                    }
                    catch (Exception ex)
                    {
                        ReportException(ex);
                        FileModel = null;
                    }
                }
            }
            else
            {
                FileModel = null;
            }
        }

        #region IDisposable

        private bool disposed = false;

        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                FileModel?.Dispose();
            }
            disposed = true;
            base.Dispose(disposing);
        }

        #endregion IDisposable

        #region Import/Export

        public async Task Import(string destFileName)
        {
            string srcFileName;
            ContainerEntry entry;
            try
            {
                ClearStatus();
                srcFileName = _getFileName.OpenGetFileName("Import", "All files (*.*)|*.*");
                if (srcFileName != null)
                {
                    if (!File.Exists(srcFileName))
                    {
                        ReportMessage("Selected file not found.");
                        return;
                    }
                    entry = ContainerFile.GetEntry(destFileName);
                    if (entry == null)
                        throw new ArgumentNullException("Could not find current file in archive.");
                }
                else
                    return;
            }
            catch (Exception ex)
            {
                ReportException(ex);
                return;
            }
            try
            {
                using FileStream fileStream = new FileStream(srcFileName, FileMode.Open, FileAccess.Read);
                ReportMessage("Importing...");
                await entry.SetData(fileStream);
                ReportMessage("Done.");
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
            if (CurrentFile == destFileName)
                await LoadChildFileModel(CurrentFile);
        }

        public async Task Export(string srcFileName)
        {
            try
            {
                ClearStatus();
                string destFileName = _getFileName.SaveGetFileName("Export",
                    "",
                    srcFileName.Substring(srcFileName.LastIndexOf('/') + 1),
                    "");
                if (destFileName != null)
                {
                    ContainerEntry entry = ContainerFile.GetEntry(srcFileName);
                    if (entry == null)
                        throw new ArgumentNullException("Could not find current file in archive.");
                    using Stream stream = await entry.GetData();
                    using FileStream fileStream = new FileStream(destFileName, FileMode.Create, FileAccess.Write);
                    ReportMessage("Exporting...");
                    await stream.CopyToAsync(fileStream);
                    ReportMessage("Done.");
                }
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }

        #endregion Import/Export
    }
}