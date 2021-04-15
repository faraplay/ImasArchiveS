using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
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

        public IGetFileName GetFileName { get; }
        public ObservableCollection<UIControlModel> ControlModel { get; }
        public ObservableCollection<UISpriteSheetModel> SpriteSheets { get; }
        public ObservableCollection<UIElementPropertyModel> UIProperties { get; }
        private UIModel _displayedModel;
        public UIModel DisplayedModel
        {
            get => _displayedModel;
            set
            {
                _displayedModel = value;
                _displayedModel?.LoadImage();
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
                _selectedModel?.LoadImage();
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
                UIControlModel.CreateModel(this, null, uiComponent.control)
            };
            UIProperties = new ObservableCollection<UIElementPropertyModel>();
            foreach (var spritesheet in SpriteSheets)
            {
                spritesheet.UpdateRectangles();
            }
            DisplayedModel = ControlModel[0];
        }

        public Bitmap GetSpritesheet(int index) => uiComponent.imageSource[index];

        private void RefreshPropertiesList()
        {
            UIProperties.Clear();
            if (SelectedModel is UIElementModel uiElementModel)
            {
                UIElement element = uiElementModel.MyUIElement;
                foreach ((var property, var attr) in element.GetType()
                    .GetProperties()
                    .Select(p => (p, p.GetCustomAttributes(typeof(ListedAttribute), false)))
                    .Where(tuple => tuple.Item2.Length > 0)
                    .Select(tuple => (tuple.p, (ListedAttribute)tuple.Item2[0]))
                    .OrderBy(tuple => tuple.Item2.Order))
                {
                    UIProperties.Add(new UIElementPropertyModel(property, element));
                }
            }
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
                DisplayedModel.LoadImage();
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
    }
}
