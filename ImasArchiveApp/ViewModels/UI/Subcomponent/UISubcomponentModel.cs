using System;
using System.Drawing;
using System.Threading.Tasks;
using Imas.Gtf;
using Imas.UI;

namespace ImasArchiveApp
{
    public class UISubcomponentModel : FileModel
    {
        public UISubcomponent Subcomponent { get; }

        public IGetFileName GetFileName { get; }
        public PauModel PauModel { get; }
        public PtaModel PtaModel { get; }
        public PaaModel PaaModel { get; }

        private int _selectedIndex;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                OnPropertyChanged();
            }
        }

        public UISubcomponentModel(IReport parent, UISubcomponent subcomponent, string filename, IGetFileName getFileName) : base(parent, filename)
        {
            GetFileName = getFileName;
            Subcomponent = subcomponent;
            PtaModel = new PtaModel(this);
            PauModel = new PauModel(this);
            PtaModel.UpdateRectangles();
            if (subcomponent.animationGroups != null)
            {
                PaaModel = new PaaModel(this);
            }
            PauModel.DisplayedModel = PauModel.ControlModel[0];
        }

        public GTF GetSpritesheet(int index) => Subcomponent.imageSource[index];

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
                    await Subcomponent.imageSource.ReplaceGTF(index, bitmap);
                    ReportMessage("Done.");
                }
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }
    }
}
