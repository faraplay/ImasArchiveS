using Imas.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ImasArchiveApp
{
    class UIComponentModel : FileModel
    {
        private UIComponent _component;
        private IFileModel _fileModel;
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

        protected UIComponentModel(IReport report, string filename) : base(report, filename) { }
        public UIComponentModel(IReport parent, Stream stream, string fileName, IGetFileName getFileName)
            : base(parent, fileName)
        {
            try
            {
                _component = new UIComponent(stream);
                SubcomponentNames = new ObservableCollection<string>();
                foreach (string name in _component.SubcomponentNames)
                    SubcomponentNames.Add(name);
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        internal static FileModelFactory.FileModelBuilder Builder { get; set; } =
            (report, filename, getFilename, stream) => new UIComponentModel(report, stream, filename, getFilename);

        private AsyncCommand _loadSubComponentCommand;

        public ICommand LoadSubcomponentCommand
        {
            get
            {
                if (_loadSubComponentCommand == null)
                {
                    _loadSubComponentCommand = new AsyncCommand(
                        () => LoadChildFileModel(SelectedName));
                }
                return _loadSubComponentCommand;
            }
        }

        public async Task LoadChildFileModel(string fileName)
        {
            ClearStatus();
            if (fileName != null)
            {
                try
                {
                    ReportMessage("Loading " + fileName);
                    FileModel = new UISubcomponentModel(FileModelFactory.report, await _component.CreateComponent(fileName), fileName);
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
