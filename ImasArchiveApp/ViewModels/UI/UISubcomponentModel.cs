using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Imas.UI;

namespace ImasArchiveApp
{
    public class UISubcomponentModel : FileModel
    {
        private readonly UISubcomponent uiComponent;
        private readonly IGetFileName _getfileName;
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

        public UISubcomponentModel(IReport parent, UISubcomponent subcomponent, string filename, IGetFileName getFileName) : base(parent, filename)
        {
            _getfileName = getFileName;
            uiComponent = subcomponent;
            ControlModel = new ObservableCollection<UIControlModel>
            {
                UIControlModel.CreateModel(this, null, uiComponent.control)
            };
            ControlModel[0].LoadImage();
            SelectedModel = ControlModel[0];
        }

    }
}
