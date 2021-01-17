using Imas.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace ImasArchiveApp
{
    public class UISpriteSheetModel : UIModel
    {
        private readonly int index;
        private readonly IGetFileName _getfileName;
        public Bitmap bitmap;

        public List<UISpriteModel> Sprites { get; }

        private Dictionary<RectangleF, UISpriteSheetRectangleModel> rectLookup;

        public ObservableCollection<UISpriteSheetRectangleModel> Rectangles { get; }

        public UISpriteSheetModel(UISubcomponentModel subcomponent, string name, int index, IGetFileName getFileName) : base(subcomponent, name)
        {
            this.index = index;
            bitmap = subcomponent.GetSpritesheet(index);
            _getfileName = getFileName;
            Sprites = new List<UISpriteModel>();
            Rectangles = new ObservableCollection<UISpriteSheetRectangleModel>();
        }

        public void UpdateRectangles()
        {
            rectLookup = new Dictionary<RectangleF, UISpriteSheetRectangleModel>();
            Rectangles.Clear();
            foreach (UISpriteModel sprite in Sprites)
            {
                RectangleF rectangle = new RectangleF(sprite.SourceX, sprite.SourceY, sprite.SourceWidth, sprite.SourceHeight);
                if (rectLookup.TryGetValue(rectangle, out var model))
                {
                    model.Sprites.Add(sprite);
                }
                else
                {
                    model = new UISpriteSheetRectangleModel(subcomponent, this, rectangle);
                    model.Sprites.Add(sprite);
                    Rectangles.Add(model);
                    rectLookup.Add(rectangle, model);
                }
            }
        }

        protected override Bitmap GetBitmap()
        {
            Bitmap newBitmap = new Bitmap(bitmap.Width + 1, bitmap.Height + 1);
            using Graphics g = Graphics.FromImage(newBitmap);
            g.DrawImage(bitmap, new Point());
            foreach (var rectangle in Rectangles)
            {
                g.DrawRectangle(Pens.Yellow, Rectangle.Round(rectangle.Rectangle));
            }
            return newBitmap;
        }

        private AsyncCommand _replaceImageCommand;

        public ICommand ReplaceImageCommand
        {
            get
            {
                if (_replaceImageCommand == null)
                {
                    _replaceImageCommand = new AsyncCommand(
                        () => ReplaceImage());
                }
                return _replaceImageCommand;
            }
        }

        private async Task ReplaceImage()
        {
            await subcomponent.ReplaceImage(index);
            bitmap = subcomponent.GetSpritesheet(index);
        }
    }

}
