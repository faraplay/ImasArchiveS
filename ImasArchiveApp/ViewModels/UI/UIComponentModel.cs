using Imas.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Resources;

namespace ImasArchiveApp
{
    class UIComponentModel : FileModel
    {
        private UIComponent _component;
        private IFileModel _fileModel;
        private readonly IGetFileName _getFileName;
        private string _selectedName;

        public ObservableCollection<string> SubcomponentNames { get; }

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

        public string SelectedName
        {
            get => _selectedName;
            set
            {
                _selectedName = value;
                OnPropertyChanged();
            }
        }

        private UIComponentModel(IReport parent, string fileName, IGetFileName getFileName)
            : base(parent, fileName)
        {
            _getFileName = getFileName;
            SubcomponentNames = new ObservableCollection<string>();
        }

        public static async Task<UIComponentModel> CreateComponentModel(IReport parent, Stream stream, string fileName, IGetFileName getFileName)
        {
            UIComponentModel model = null;
            try
            {
                model = new UIComponentModel(parent, fileName, getFileName)
                {
                    _component = await UIComponent.CreateUIComponent(stream)
                };
                foreach (string name in model._component.SubcomponentNames)
                    model.SubcomponentNames.Add(name);
                if (model.SubcomponentNames.Count == 1)
                {
                    model.SelectedName = model.SubcomponentNames[0];
                    await model.LoadSubcomponent(model.SelectedName);
                }
                return model;
            }
            catch
            {
                model?.Dispose();
                throw;
            }

        }

        private AsyncCommand _loadSubComponentCommand;

        public ICommand LoadSubcomponentCommand
        {
            get
            {
                if (_loadSubComponentCommand == null)
                {
                    _loadSubComponentCommand = new AsyncCommand(
                        () => LoadSubcomponent(SelectedName));
                }
                return _loadSubComponentCommand;
            }
        }

        public async Task LoadSubcomponent(string subName)
        {
            ClearStatus();
            if (subName == null)
            {
                FileModel = null;
            }
            else if (subName != FileModel?.FileName)
            {
                try
                {
                    if (TextBox.font == null)
                    {
                        ReportMessage("Loading font");
                        TextBox.font = new Imas.Font();
                        using (MemoryStream stream = new MemoryStream(FontResource.im2nx_font))
                        {
                            await TextBox.font.ReadFontPar(stream);
                        }
                        ReportMessage("Loaded font.");

                    }
                    ReportMessage("Loading subcomponent " + subName);
                    FileModel = new UISubcomponentModel(FileModelFactory.report, _component[subName], subName, _getFileName);
                    ReportMessage("Loaded subcomponent " + subName);
                }
                catch (Exception ex)
                {
                    ReportException(ex);
                    FileModel = null;
                }
            }
        }

        private AsyncCommand _saveAsCommand;

        public ICommand SaveAsCommand
        {
            get
            {
                if (_saveAsCommand == null)
                {
                    _saveAsCommand = new AsyncCommand(
                        () => SaveAs());
                }
                return _saveAsCommand;
            }
        }

        public async Task SaveAs()
        {
            try
            {
                string fileName = _getFileName.SaveGetFileName("Save As...", FileName, "Par files (*.par)|*.par");
                if (fileName != null)
                {
                    ClearStatus();
                    ReportMessage("Saving component...");
                    using (FileStream outStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                    {
                        await _component.SaveTo(outStream);
                    }
                    ReportMessage("Done.");
                }
            }
            catch (Exception ex)
            {
                ReportException(ex);
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
                _fileModel?.Dispose();
                _component?.Dispose();
            }
            disposed = true;
            base.Dispose(disposing);
        }

        #endregion IDisposable
    }
}
