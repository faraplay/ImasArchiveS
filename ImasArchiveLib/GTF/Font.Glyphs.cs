using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Imas.Gtf
{
    public partial class Font
    {
        public void GenerateGlyphs(string fontPathAbsolute, char[] glyphs, string outPath)
        {
            GlyphTypeface glyphTypeface = new GlyphTypeface(new Uri($"file:///{fontPathAbsolute}"));
            double fontSize = 29; // font size 29 gives the right values for advanceWidth (when rounded using x -> floor(x + 0.5))
            var charToGlyphMap = glyphTypeface.CharacterToGlyphMap;
            for (int i = 0; i < glyphs.Length; ++i) {
                ushort glyph = charToGlyphMap[glyphs[i]];
                var geo = glyphTypeface.GetGlyphOutline(
                    glyph,
                    fontSize,
                    fontSize);
                double blackBoxWidth = glyphTypeface.AdvanceWidths[glyph] - glyphTypeface.LeftSideBearings[glyph] - glyphTypeface.RightSideBearings[glyph];
                double blackBoxHeight = glyphTypeface.AdvanceHeights[glyph] - glyphTypeface.TopSideBearings[glyph] - glyphTypeface.BottomSideBearings[glyph];
                DrawingVisual drawingVisual = new DrawingVisual();
                DrawingContext drawingContext = drawingVisual.RenderOpen();
                double xOffset = fontSize * (0 - glyphTypeface.LeftSideBearings[glyph]);
                double yOffset = fontSize * (blackBoxHeight - glyphTypeface.DistancesFromHorizontalBaselineToBlackBoxBottom[glyph]);
                drawingContext.PushTransform(new TranslateTransform(
                    1 + Math.Ceiling(xOffset),
                    1 + Math.Ceiling(yOffset)));
                drawingContext.DrawGeometry(Brushes.White, null, geo);
                drawingContext.Pop();
                drawingContext.Close();
                RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(
                    2 + (int)(Math.Ceiling(xOffset) + Math.Ceiling(fontSize * blackBoxWidth - xOffset)), 
                    2 + (int)(Math.Ceiling(yOffset) + Math.Ceiling(fontSize * blackBoxHeight- yOffset)), 
                    96, 96, PixelFormats.Default);
                renderTargetBitmap.Render(drawingVisual);
                BitmapEncoder bitmapEncoder = new PngBitmapEncoder();
                bitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
                using (FileStream outStream = File.Create($"{outPath}{(int)glyphs[i]}.png"))
                {
                    bitmapEncoder.Save(outStream);
                }
            }
        }
    }
}