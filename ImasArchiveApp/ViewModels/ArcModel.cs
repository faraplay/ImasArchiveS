using Imas;
using Imas.Archive;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ImasArchiveApp
{
    public class ArcModel : ContainerFileModel
    {
        #region Properties

        public string ArcPath { get; }
        private ArcFile ArcFile { get; }
        protected override IContainerFile ContainerFile => ArcFile;

        #endregion Properties

        #region Constructors

        public ArcModel(IReport parent, string inPath, IGetFileName getFileName)
            : base(parent, inPath.Substring(inPath.LastIndexOf('\\') + 1), getFileName)
        {
            ArcPath = inPath;
            _fileModel = null;
            try
            {
                string truncFilename;
                string extension;
                (truncFilename, extension) = ArcFile.RemoveArcExtension(inPath);
                if (extension == null)
                {
                    throw new ArgumentException("Selected file does not have .arc or .arc.dat extension.");
                }
                ArcFile = new ArcFile(truncFilename, extension);
                SetBrowserEntries();
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        #endregion Constructors

        #region IDisposable

        private bool disposed = false;

        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                ArcFile?.Dispose();
            }
            disposed = true;
            base.Dispose(disposing);
        }

        #endregion IDisposable

        #region Commands

        private AsyncCommand _importCommand;

        public ICommand ImportCommand
        {
            get
            {
                if (_importCommand == null)
                {
                    _importCommand = new AsyncCommand(() => Import(CurrentFile), () => CanImport());
                }
                return _importCommand;
            }
        }

        public bool CanImport() => ArcFile != null && FileModel != null;

        private AsyncCommand _exportCommand;

        public ICommand ExportCommand
        {
            get
            {
                if (_exportCommand == null)
                {
                    _exportCommand = new AsyncCommand(() => Export(CurrentFile), () => CanExport());
                }
                return _exportCommand;
            }
        }

        public bool CanExport() => ArcFile != null && FileModel != null;

        private AsyncCommand _saveAsCommand;

        public ICommand SaveAsCommand
        {
            get
            {
                if (_saveAsCommand == null)
                {
                    _saveAsCommand = new AsyncCommand(() => SaveAs(), () => CanSaveAs());
                }
                return _saveAsCommand;
            }
        }

        public bool CanSaveAs() => ArcFile != null;

        private AsyncCommand _extractAllCommand;

        public ICommand ExtractAllCommand
        {
            get
            {
                if (_extractAllCommand == null)
                {
                    _extractAllCommand = new AsyncCommand(() => ExtractAll(), () => CanExtractAll());
                }
                return _extractAllCommand;
            }
        }

        public bool CanExtractAll() => ArcFile != null;

        private AsyncCommand _extractCommusCommand;

        public ICommand ExtractCommusCommand
        {
            get
            {
                if (_extractCommusCommand == null)
                {
                    _extractCommusCommand = new AsyncCommand(() => ExtractCommus(), () => CanExtractCommus());
                }
                return _extractCommusCommand;
            }
        }

        public bool CanExtractCommus() => ArcFile != null;

        private AsyncCommand _extractParameterCommand;

        public ICommand ExtractParameterCommand
        {
            get
            {
                if (_extractParameterCommand == null)
                {
                    _extractParameterCommand = new AsyncCommand(() => ExtractParameter(), () => CanExtractParameter());
                }
                return _extractParameterCommand;
            }
        }

        public bool CanExtractParameter() => ArcFile != null;

        private AsyncCommand _extractImagesCommand;

        public ICommand ExtractImagesCommand
        {
            get
            {
                if (_extractImagesCommand == null)
                {
                    _extractImagesCommand = new AsyncCommand(() => ExtractImages(), () => CanExtractImages());
                }
                return _extractImagesCommand;
            }
        }

        public bool CanExtractImages() => ArcFile != null;

        private AsyncCommand _extractLyricsCommand;

        public ICommand ExtractLyricsCommand
        {
            get
            {
                if (_extractLyricsCommand == null)
                {
                    _extractLyricsCommand = new AsyncCommand(() => ExtractLyrics(), () => CanExtractLyrics());
                }
                return _extractLyricsCommand;
            }
        }

        public bool CanExtractLyrics() => ArcFile != null;

        private AsyncCommand _patchFontCommand;

        public ICommand PatchFontCommand
        {
            get
            {
                if (_patchFontCommand == null)
                {
                    _patchFontCommand = new AsyncCommand(() => PatchFont(), () => CanPatchFont());
                }
                return _patchFontCommand;
            }
        }

        public bool CanPatchFont() => ArcFile != null;

        #endregion Commands

        #region Command Methods

        public async Task SaveAs()
        {
            try
            {
                ClearStatus();
                string fileName = _getFileName.SaveGetFileName("Save As", "", "Arc file (*.arc)|*.arc");
                if (fileName != null)
                {
                    await ArcFile.SaveAs(fileName[0..^4], ProgressReporter);
                    ReportMessage("Done.");
                }
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }

        public async Task ExtractAll()
        {
            try
            {
                ClearStatus();
                string fileName = _getFileName.SaveGetFileName("Extract to...", ArcFile.RemoveArcExtension(ArcPath).Item1, "");
                if (fileName != null)
                {
                    await ArcFile.ExtractAllAsync(fileName, false, ProgressReporter);
                    ReportMessage("Done.");
                }
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }

        public async Task ExtractCommus()
        {
            try
            {
                ClearStatus();
                string fileName = _getFileName.SaveGetFileName("Save As", ArcFile.RemoveArcExtension(ArcPath).Item1 + "commus.xlsx", "Excel spreadsheet (*.xlsx)|*.xlsx");
                if (fileName != null)
                {
                    await ArcFile.ExtractCommusToXlsx(fileName, true, ProgressReporter);
                    ReportMessage("Done.");
                }
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }

        public async Task ExtractParameter()
        {
            try
            {
                ClearStatus();
                string fileName = _getFileName.SaveGetFileName("Save As", ArcFile.RemoveArcExtension(ArcPath).Item1 + "_parameter.xlsx", "Excel spreadsheet (*.xlsx)|*.xlsx");
                if (fileName != null)
                {
                    await ArcFile.ExtractParameterToXlsx(fileName, true, ProgressReporter);
                    ReportMessage("Done.");
                }
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }

        public async Task ExtractImages()
        {
            try
            {
                ClearStatus();
                string fileName = _getFileName.SaveGetFileName("Select New Folder", ArcFile.RemoveArcExtension(ArcPath).Item1 + "_image", "");
                if (fileName != null)
                {
                    await ArcFile.ExtractAllImages(fileName, ProgressReporter);
                    ReportMessage("Done.");
                }
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }

        public async Task ExtractLyrics()
        {
            try
            {
                ClearStatus();
                string dirName = _getFileName.SaveGetFileName("Select New Folder", ArcFile.RemoveArcExtension(ArcPath).Item1 + "_lyrics", "");
                if (dirName != null)
                {
                    await ArcFile.ExtractLyrics(dirName, ProgressReporter);
                    ReportMessage("Done.");
                }
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }

        public async Task PatchFont()
        {
            try
            {
                ClearStatus();
                ContainerEntry fontEntry = ArcFile.GetEntry("im2nx_font.par");
                if (fontEntry == null)
                {
                    throw new FileNotFoundException("File im2nx_font.par not found. Make sure you are opening disc.arc");
                }
                using (Font font = new Font())
                {
                    using (Stream parStream = await fontEntry.GetData())
                    {
                        ReportMessage("Reading im2nx_font.par...");
                        await Task.Run(() => font.ReadFontPar(parStream));
                    }
                    ReportMessage("Patching font...");
                    await Task.Run(() => font.AddDigraphs());

                    ReportMessage("Writing font as par...");
                    using MemoryStream memStream = new MemoryStream();
                    await font.WriteFontPar(memStream, false);
                    memStream.Position = 0;
                    await fontEntry.SetData(memStream);
                }
                ReportMessage("Done.");
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }

        #endregion Command Methods
    }
}