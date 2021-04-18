using Imas.Gtf;
using Imas.UI;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImasArchiveApp
{
    class UITextBoxModel : UITypedControlModel<TextBox>
    {
        public static Font font;
        public static BitmapSource fontSource;
        public static async Task LoadFontFromStreamAsync(Stream stream)
        {
            font = await Font.CreateFromPar(stream);
            fontSource = BitmapSource.Create(
                2048, 2048,
                96, 96,
                PixelFormats.Bgra32,
                null,
                font.Gtf.BitmapDataPtr,
                4 * 2048 * 2048,
                4 * 2048);
        }
        public UITextBoxModel(UISubcomponentModel subcomponent, UIElementModel parent, TextBox control) : base(subcomponent, parent, control) { }

        protected override void RenderElementUntransformed(DrawingContext drawingContext, ColorMultiplier multiplier, bool isTop)
        {
            base.RenderElementUntransformed(drawingContext, multiplier, isTop);
            if (!CurrentVisibility && !isTop)
                return;
            multiplier.Scale(_control.TextRed / 255.0f, _control.TextGreen / 255.0f, _control.TextBlue / 255.0f);
            font.DrawByteArray(
                _control.textBuffer,
                _control.width,
                _control.height,
                new TextBoxAttributes() { 
                    xAlign = _control.XAlignment, 
                    yAlign = _control.YAlignment, 
                    multiline = _control.Multiline, 
                    wordWrap = _control.WordWrap },
                (outX, outY, x, y, width, height) => DrawChar(drawingContext, outX, outY, new Rect(x, y, width, height), multiplier));
        }

        private void DrawChar(DrawingContext drawingContext, float x, float y, Rect charSourceRect, ColorMultiplier multiplier)
        {
            ImageBrush imageBrush = new ImageBrush(fontSource)
            {
                Viewbox = charSourceRect,
                ViewboxUnits = BrushMappingMode.Absolute
            };
            if (multiplier.r == 1.0f &&
                multiplier.g == 1.0f &&
                multiplier.b == 1.0f)
            {
                drawingContext.DrawRectangle(imageBrush, null, new Rect(x, y, charSourceRect.Width, charSourceRect.Height));
            }
            else
            {
                drawingContext.PushOpacityMask(imageBrush);
                SolidColorBrush colorBrush = new SolidColorBrush(
                    Color.FromRgb(
                        (byte)(multiplier.r * 255), 
                        (byte)(multiplier.g * 255),
                        (byte)(multiplier.b * 255)));
                drawingContext.DrawRectangle(colorBrush, null, new Rect(x, y, charSourceRect.Width, charSourceRect.Height));
                drawingContext.Pop();
            }
        }
    }
}
