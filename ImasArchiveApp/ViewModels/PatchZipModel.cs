using Imas.Archive;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ImasArchiveApp
{
    public class PatchZipModel : ContainerFileModel
    {
        #region Fields

        private readonly PatchZipFile _patchZipFile;

        #endregion Fields

        #region Properties

        protected override IContainerFile ContainerFile => _patchZipFile;

        #endregion Properties

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

        #endregion Constructors

        #region Commands

        private AsyncCommand _addFileCommand;

        public IAsyncCommand AddFileCommand
        {
            get
            {
                if (_addFileCommand == null)
                {
                    _addFileCommand = new AsyncCommand(() => AddFile());
                }
                return _addFileCommand;
            }
        }

        private AsyncCommand _addCommusCommand;

        public IAsyncCommand AddCommusCommand
        {
            get
            {
                if (_addCommusCommand == null)
                {
                    _addCommusCommand = new AsyncCommand(() => AddCommus());
                }
                return _addCommusCommand;
            }
        }

        private AsyncCommand _addParameterCommand;

        public IAsyncCommand AddParameterCommand
        {
            get
            {
                if (_addParameterCommand == null)
                {
                    _addParameterCommand = new AsyncCommand(() => AddParameter());
                }
                return _addParameterCommand;
            }
        }

        private AsyncCommand _addImagesCommand;

        public IAsyncCommand AddImagesCommand
        {
            get
            {
                if (_addImagesCommand == null)
                {
                    _addImagesCommand = new AsyncCommand(() => AddImages());
                }
                return _addImagesCommand;
            }
        }

        private AsyncCommand _addLyricsCommand;

        public IAsyncCommand AddLyricsCommand
        {
            get
            {
                if (_addLyricsCommand == null)
                {
                    _addLyricsCommand = new AsyncCommand(() => AddLyrics());
                }
                return _addLyricsCommand;
            }
        }

        #endregion Commands

        #region Command Methods

        public async Task AddFile()
        {
            try
            {
                string srcFileName = _getFileName.OpenGetFileName("Select File", "All Files (*.*)|*.*");
                if (srcFileName != null)
                {
                    if (!File.Exists(srcFileName))
                    {
                        ReportMessage("Selected file not found.");
                        return;
                    }
                    string destFileName = _getFileName.GetString();
                    if (destFileName == null)
                    {
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(destFileName))
                    {
                        ReportMessage("Filename not valid.");
                        return;
                    }
                    await Task.Run(() => _patchZipFile.AddFile(srcFileName, destFileName));
                    SetBrowserEntries();
                    ReportMessage("Done.");
                }
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }

        public async Task AddCommus()
        {
            try
            {
                string srcFileName = _getFileName.OpenGetFileName("Open Commu Spreadsheet", "Excel Spreadsheet (*.xlsx)|*.xlsx");
                if (srcFileName != null)
                {
                    if (!File.Exists(srcFileName))
                    {
                        ReportMessage("Selected file not found.");
                        return;
                    }
                    await _patchZipFile.AddCommus(srcFileName, ProgressReporter, ProgressReporter);
                    SetBrowserEntries();
                    ReportMessage("Done.");
                }
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }

        public async Task AddParameter()
        {
            try
            {
                string srcFileName = _getFileName.OpenGetFileName("Open Parameter Spreadsheet", "Excel Spreadsheet (*.xlsx)|*.xlsx");
                if (srcFileName != null)
                {
                    if (!File.Exists(srcFileName))
                    {
                        ReportMessage("Selected file not found.");
                        return;
                    }
                    await Task.Run(() => _patchZipFile.AddParameterFiles(srcFileName));
                    SetBrowserEntries();
                    ReportMessage("Done.");
                }
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }

        public async Task AddImages()
        {
            try
            {
                string dirName = _getFileName.OpenGetFolderName("Open Image Folder");
                if (dirName != null)
                {
                    if (!File.Exists(dirName + "/filenames.xlsx"))
                    {
                        ReportMessage("filenames.xlsx not found.");
                        return;
                    }
                    await _patchZipFile.AddGtfs(dirName, ProgressReporter);
                    SetBrowserEntries();
                    ReportMessage("Done.");
                }
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }

        public async Task AddLyrics()
        {
            try
            {
                string dirName = _getFileName.OpenGetFolderName("Open Lyrics Folder");
                if (dirName != null)
                {
                    if (!Directory.Exists(dirName))
                    {
                        ReportMessage("Directory not found.");
                        return;
                    }
                    await _patchZipFile.AddLyrics(dirName, ProgressReporter);
                    SetBrowserEntries();
                    ReportMessage("Done.");
                }
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }

        #endregion Command Methods

        #region IDisposable

        private bool disposed = false;

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

        #endregion IDisposable
    }
}