using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Imas;
using Imas.Gtf;
using Imas.UI;

namespace ImasArchiveApp
{
    public class UISubcomponentModel : FileModel
    {
        private readonly UISubcomponent uiComponent;

        public IGetFileName GetFileName { get; }
        public ObservableCollection<UIControlModel> ControlModel { get; }
        public ObservableCollection<UISpriteSheetModel> SpriteSheets { get; }
        public ObservableCollection<UIElementPropertyModel> UIProperties { get; }
        public ObservableCollection<UIAnimationGroupModel> AnimationGroups { get; }
        public Dictionary<string, UIControlModel> ControlDictionary { get; }

        private UIAnimationGroupModel _selectedAnimationGroupModel;
        public UIAnimationGroupModel SelectedAnimationGroupModel
        {
            get => _selectedAnimationGroupModel;
            set
            {
                _selectedAnimationGroupModel?.RemoveAnimations();
                _selectedAnimationGroupModel = value;
                _selectedAnimationGroupModel?.ApplyAnimations();
                ForceRender();
                OnPropertyChanged();
            }
        }
        private UIModel _displayedModel;
        public UIModel DisplayedModel
        {
            get => _displayedModel;
            set
            {
                _displayedModel = value;
                OnPropertyChanged();
            }
        }
        private UIModel _selectedModel;
        public UIModel SelectedModel
        {
            get => _selectedModel;
            set
            {
                _selectedModel = value;
                RefreshPropertiesList();
                OnPropertyChanged();
            }
        }

        public UISubcomponentModel(IReport parent, UISubcomponent subcomponent, string filename, IGetFileName getFileName) : base(parent, filename)
        {
            GetFileName = getFileName;
            uiComponent = subcomponent;
            SpriteSheets = new ObservableCollection<UISpriteSheetModel>();
            for (int i = 0; i < uiComponent.imageSource.Count; i++)
            {
                SpriteSheets.Add(new UISpriteSheetModel(this, uiComponent.imageSource.Filenames[i], i, getFileName));
            }
            ControlModel = new ObservableCollection<UIControlModel>
            {
                new UIControlModel(uiComponent.rootControl, this, null)
            };
            ControlDictionary = new Dictionary<string, UIControlModel>();
            ControlModel[0].ForAll(control =>
            {
                if (!string.IsNullOrWhiteSpace(control.FileName))
                {
                    if (ControlDictionary.ContainsKey(control.FileName))
                        return;
                    ControlDictionary.Add(control.FileName, control);
                }
            });
            UIProperties = new ObservableCollection<UIElementPropertyModel>();
            PropertyChangedEventHandler = (sender, e) => ForceRender();
            foreach (var spritesheet in SpriteSheets)
            {
                spritesheet.UpdateRectangles();
            }
            if (subcomponent.animationGroups != null)
            {
                AnimationGroups = new ObservableCollection<UIAnimationGroupModel>();
                foreach (var animationGroup in subcomponent.animationGroups)
                {
                    AnimationGroups.Add(new UIAnimationGroupModel(this, animationGroup));
                }
            }
            DisplayedModel = ControlModel[0];
        }

        public GTF GetSpritesheet(int index) => uiComponent.imageSource[index];

        private void RefreshPropertiesList()
        {
            foreach (UIElementPropertyModel propertyModel in UIProperties)
            {
                propertyModel.PropertyChanged -= PropertyChangedEventHandler;
            }
            UIProperties.Clear();
            if (SelectedModel is UIElementModel uiElementModel)
            {
                UIElement element = uiElementModel.UIElement;
                foreach ((var property, var attr) in element.GetType()
                    .GetProperties()
                    .Select(p => (p, p.GetCustomAttributes(typeof(ListedAttribute), false)))
                    .Where(tuple => tuple.Item2.Length > 0)
                    .Select(tuple => (tuple.p, (ListedAttribute)tuple.Item2[0]))
                    .OrderBy(tuple => tuple.Item2.Order))
                {
                    UIElementPropertyModel propertyModel = new UIElementPropertyModel(property, element);
                    propertyModel.PropertyChanged += PropertyChangedEventHandler;
                    UIProperties.Add(propertyModel);
                }
            }
        }

        private readonly PropertyChangedEventHandler PropertyChangedEventHandler;
        public void ForceRender()
        {
            DisplayedModel?.ForceRender();
            SelectedModel?.ForceRender();
        }
        public void ResetDefaultValues()
        {
            SelectedAnimationGroupModel = null;
            ControlModel[0].ForAll(model => model.ResetAnimatedValues());
        }

        public async Task ReplaceImage(int index)
        {
            try
            {
                ClearStatus();
                string imgName = GetFileName.OpenGetFileName("Open Image", "Portable Network Graphic (*.png)|*.png");
                if (imgName != null)
                {
                    ReportMessage("Replacing image");
                    using Bitmap bitmap = new Bitmap(imgName);
                    await uiComponent.imageSource.ReplaceGTF(index, bitmap);
                    ReportMessage("Done.");
                }
                //DisplayedModel.LoadImage();
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }

        public void SavePau(string fileName)
        {
            using FileStream outStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            uiComponent.WritePauStream(outStream);
        }

        private RelayCommand _resetCommand;
        public ICommand ResetCommand
        {
            get
            {
                if (_resetCommand == null)
                    _resetCommand = new RelayCommand(
                        _ => ResetDefaultValues());
                return _resetCommand;
            }
        }
    }
}
