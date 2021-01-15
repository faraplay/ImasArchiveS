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

        protected UIComponentModel(IReport parent, string filename) : base(parent, filename) { }
        public UIComponentModel(IReport parent, Stream stream, string fileName, IGetFileName getFileName)
            : base(parent, fileName)
        {
            try
            {
                parent.ClearStatus();
                parent.ReportMessage("Loading component " + fileName);
                _component = new UIComponent(stream);
                SubcomponentNames = new ObservableCollection<string>();
                foreach (string name in _component.SubcomponentNames)
                    SubcomponentNames.Add(name);
                parent.ReportMessage("Loaded component " + fileName);
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
                        () => LoadSubcomponent(SelectedName));
                }
                return _loadSubComponentCommand;
            }
        }

        public async Task LoadSubcomponent(string subName)
        {
            ClearStatus();
            if (subName != null)
            {
                try
                {
                    ClearStatus();
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
                    FileModel = new UISubcomponentModel(FileModelFactory.report, await _component.CreateComponent(subName), subName);
                    ReportMessage("Loaded subcomponent " + subName);
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
