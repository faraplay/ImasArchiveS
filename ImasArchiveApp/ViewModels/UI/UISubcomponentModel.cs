using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Imas.UI;

namespace ImasArchiveApp
{
    public class UISubcomponentModel : FileModel
    {
        private UISubcomponent uiComponent;
        private UIElementModel _selectedModel;
        public ObservableCollection<UIControlModel> ControlModel { get; }
        public UIElementModel SelectedModel
        {
            get => _selectedModel;
            set
            {
                _selectedModel = value;
                OnPropertyChanged();
            }
        }

        public UISubcomponentModel(IReport parent, UISubcomponent subcomponent, string filename) : base(parent, filename)
        {
            parent.ClearStatus();
            parent.ReportMessage("Loading subcomponent " + filename);
            uiComponent = subcomponent;
            ControlModel = new ObservableCollection<UIControlModel>
            {
                UIControlModel.CreateModel(this, uiComponent.control)
            };
            parent.ReportMessage("Loaded.");
        }
    }
}
