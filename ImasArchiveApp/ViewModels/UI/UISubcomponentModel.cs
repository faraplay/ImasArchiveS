using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
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

        public IGetFileName GetFileName { get; }
        public ObservableCollection<UIControlModel> ControlModel { get; }
        public ObservableCollection<UISpriteSheetModel> SpriteSheets { get; }
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
            foreach (var spritesheet in SpriteSheets)
            {
                spritesheet.UpdateRectangles();
            }
            DisplayedModel = ControlModel[0];
        }

        public Bitmap GetSpritesheet(int index) => uiComponent.imageSource[index];

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
