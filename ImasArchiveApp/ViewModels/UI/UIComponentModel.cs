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
    class UIComponentModel : FileModel
    {
        private UIComponent uiComponent;
        private ObservableCollection<UIControlModel> _controlModel;
        public ObservableCollection<UIControlModel> ControlModel { get => _controlModel; }

        protected UIComponentModel(IReport report, string filename) : base(report, filename) { }

        public static async Task<UIComponentModel> CreateUIComponentModel(IReport report, Stream parStream, string parName)
        {
            UIComponentModel model = new UIComponentModel(report, parName);
            string fileNameShort = parName[(parName.LastIndexOf('\\') + 1)..^13];
            model.uiComponent = await UIComponent.CreateComponent(parStream, fileNameShort);
            model._controlModel = new ObservableCollection<UIControlModel>();
            model._controlModel.Add(UIControlModel.CreateModel(model.uiComponent.control));
            return model;
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged
    }
}
