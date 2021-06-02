using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Imas.Gtf
{
    public partial class Font
    {
        private const double fontSize = 29; // font size 29 gives the right values for advanceWidth (when rounded using x -> floor(x + 0.5))
        public void GenerateGlyphs(string fontPathAbsolute, char[] glyphs, string outPath)
        {
            GlyphTypeface glyphTypeface = new GlyphTypeface(new Uri($"file:///{fontPathAbsolute}"));
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
        private void UpdateCharData(GlyphTypeface glyphTypeface, CharData c)
        {
            if (!glyphTypeface.CharacterToGlyphMap.TryGetValue(c.key, out ushort cGlyph))
            {
                return;
            }
            double blackBoxWidth = glyphTypeface.AdvanceWidths[cGlyph] - glyphTypeface.LeftSideBearings[cGlyph] - glyphTypeface.RightSideBearings[cGlyph];
            double blackBoxHeight = glyphTypeface.AdvanceHeights[cGlyph] - glyphTypeface.TopSideBearings[cGlyph] - glyphTypeface.BottomSideBearings[cGlyph];
            double xOffset = fontSize * (0 - glyphTypeface.LeftSideBearings[cGlyph]);
            double yOffset = fontSize * (blackBoxHeight - glyphTypeface.DistancesFromHorizontalBaselineToBlackBoxBottom[cGlyph]);
            c.advance = (short)(fontSize * glyphTypeface.AdvanceWidths[cGlyph] + 0.5);
            c.bearingX = (short)(-1 - Math.Ceiling(xOffset));
            c.bearingY = (short)(-1 - Math.Ceiling(yOffset));
            c.paddedBbWidth = (byte)(2 + Math.Ceiling(xOffset) + Math.Ceiling(fontSize * blackBoxWidth - xOffset));
            c.paddedBbHeight = (byte)(2 + Math.Ceiling(yOffset) + Math.Ceiling(fontSize * blackBoxHeight - yOffset));
        }

        private int[] GenerateGlyph(GlyphTypeface glyphTypeface, CharData c)
        {
            if (!glyphTypeface.CharacterToGlyphMap.TryGetValue(c.key, out ushort cGlyph))
            {
                return null;
            }
            var geo = glyphTypeface.GetGlyphOutline(
                    cGlyph,
                    fontSize,
                    fontSize);
            DrawingVisual drawingVisual = new();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            drawingContext.PushTransform(new TranslateTransform(-c.bearingX, -c.bearingY));
            drawingContext.DrawGeometry(Brushes.White, null, geo);
            drawingContext.Pop();
            drawingContext.Close();
            RenderTargetBitmap renderTargetBitmap = new(
                c.paddedBbWidth, c.paddedBbHeight,
                96, 96, PixelFormats.Default);
            renderTargetBitmap.Render(drawingVisual);
            FormatConvertedBitmap formatConvertedBitmap = new(renderTargetBitmap, PixelFormats.Bgra32, null, 0);
            byte[] pixelData = new byte[c.paddedBbWidth * c.paddedBbHeight * 4];
            int[] intPixelData = new int[c.paddedBbWidth * c.paddedBbHeight];
            formatConvertedBitmap.CopyPixels(pixelData, c.paddedBbWidth * 4, 0);
            Buffer.BlockCopy(pixelData, 0, intPixelData, 0, c.paddedBbWidth * c.paddedBbHeight * 4);
            return intPixelData;
        }
        private Dictionary<ushort, int[]> GenerateCharBitmapsFromOtf(GlyphTypeface glyphTypeface, Func<char, bool> generateChar)
        {
            Dictionary<ushort, int[]> charBitmaps = new();
            foreach (CharData c in chars)
            {
                if (c.isEmoji == 0 && glyphTypeface.CharacterToGlyphMap.ContainsKey(c.key) && generateChar((char)c.key))
                {
                    UpdateCharData(glyphTypeface, c);
                    charBitmaps.Add(c.key, GenerateGlyph(glyphTypeface, c));
                }
                else
                {
                    charBitmaps.Add(c.key, GetCharPixelData(Gtf.PixelData, c));
                }
            }
            return charBitmaps;
        }

        public void GenerateAllFromOtf(string fontPathAbsolute)
        {
            var charBitmaps = GenerateCharBitmapsFromOtf(
                new GlyphTypeface(new Uri($"file:///{fontPathAbsolute}")),
                c => true);
            RebuildMyBitmap(charBitmaps, chars);
        }
        public void GenerateFromOtf(string fontPathAbsolute, char[] charsToGenerate)
        {
            var charBitmaps = GenerateCharBitmapsFromOtf(
                new GlyphTypeface(new Uri($"file:///{fontPathAbsolute}")),
                c => charsToGenerate.Contains(c));
            RebuildMyBitmap(charBitmaps, chars);
        }
    }
}