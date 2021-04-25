using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace ImasArchiveApp
{
    public class PauModel : SubcompPart<UIElementModel>
    {
        protected override int VisibilityIndex => 0;
        public ObservableCollection<UIControlModel> ControlModel { get; }
        public Dictionary<string, UIControlModel> ControlDictionary { get; private set; }
        private UIElementModel _displayedModel;
        public UIElementModel DisplayedModel
        {
            get => _displayedModel;
            set
            {
                _displayedModel = value;
                OnPropertyChanged();
            }
        }

        public PauModel(UISubcomponentModel subcomponentModel) : base(subcomponentModel)
        {
            ControlModel = new ObservableCollection<UIControlModel>
            {
                UIControlModel.CreateControlModel(subcomponentModel.Subcomponent.rootControl, subcomponentModel, null)
            };
            BuildDictionary();
        }

        public void BuildDictionary()
        {
            ControlDictionary = new Dictionary<string, UIControlModel>();
            ControlModel[0].ForAll(control =>
            {
                if (!string.IsNullOrWhiteSpace(control.ActualName))
                {
                    if (ControlDictionary.ContainsKey(control.ActualName))
                        return;
                    ControlDictionary.Add(control.ActualName, control);
                }
            });
        }

        public void ForceRender()
        {
            DisplayedModel?.ForceRender();
            SelectedModel?.ForceRender();
        }
        public void ResetDefaultValues()
        {
            ControlModel[0].ForAll(model => model.ResetAnimatedValues());
        }

        public void SavePau(string fileName)
        {
            using FileStream outStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            subcomponentModel.Subcomponent.WritePauStream(outStream);
        }
    }
}
