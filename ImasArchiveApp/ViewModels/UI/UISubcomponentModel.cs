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
    class UISubcomponentModel : FileModel
    {
        private UISubcomponent uiComponent;
        private ObservableCollection<UIControlModel> _controlModel;
        public ObservableCollection<UIControlModel> ControlModel { get => _controlModel; }

        public UISubcomponentModel(IReport report, UISubcomponent subcomponent, string filename) : base(report, filename)
        {
            uiComponent = subcomponent;
            _controlModel = new ObservableCollection<UIControlModel>
            {
                UIControlModel.CreateModel(uiComponent.control)
            };
        }
    }
}
