using System;
using System.Collections.Generic;

namespace Imas.Gtf
{
    public partial class Font : IDisposable
    {
        private List<(CharData[], int)> GetLines(Span<byte> chars, float maxWidth, bool wordWrap)
        {
            List<(CharData[], int)> lines = new List<(CharData[], int)>();
            List<CharData> line = new List<CharData>();
            int lineWidth = 0;
            for (int i = 0; i < chars.Length; i += 2)
            {
                ushort cID = (ushort)(chars[i] * 0x100 + chars[i + 1]);
                if (cID == 0)
                    break;
                if (cID == '\n')
                {
                    lines.Add((line.ToArray(), lineWidth));
                    line.Clear();
                    lineWidth = 0;
                }
                CharData charData = Find(cID);
                if (charData != null)
                {
                    if (lineWidth + charData.advance > maxWidth && wordWrap)
                    {
                        lines.Add((line.ToArray(), lineWidth));
                        line.Clear();
                        lineWidth = 0;
                    }
                    line.Add(charData);
                    lineWidth += charData.advance;
                }
            }
            lines.Add((line.ToArray(), lineWidth));
            return lines;
        }

        //private void DrawChar(Graphics g, CharData charData, ImageAttributes imageAttributes, float x, float y)
        //{
        //    g.DrawImage(
        //        BigBitmap,
        //        new PointF[] {
        //            new PointF(x + charData.offsetx, y + charData.offsety),
        //            new PointF(x + charData.offsetx + charData.datawidth, y + charData.offsety),
        //            new PointF(x + charData.offsetx, y + charData.offsety + charData.dataheight),
        //        },
        //        new Rectangle(charData.datax, charData.datay, charData.datawidth, charData.dataheight),
        //        GraphicsUnit.Pixel,
        //        imageAttributes
        //        );
        //}
        private void DrawLine(CharData[] line, float x, float y, Action<float, float, int, int, int, int> drawChar)
        {
            foreach (CharData charData in line)
            {
                drawChar(
                    x + charData.bearingX,
                    y + charData.bearingY,
                    charData.dataX,
                    charData.dataY,
                    charData.paddedBbWidth,
                    charData.paddedBbHeight);
                x += charData.advance;
            }
        }

        public void DrawByteArray(Span<byte> chars, 
            float boxWidth, float boxHeight, 
            TextBoxAttributes textBoxAttributes,
            Action<float, float, int, int, int, int> drawChar)
        {
            var lines = GetLines(chars, boxWidth, textBoxAttributes.wordWrap);
            if (lines.Count == 0 ||
                textBoxAttributes.xAlign == HorizontalAlignment.None ||
                textBoxAttributes.yAlign == VerticalAlignment.None)
                return;
            if (!textBoxAttributes.multiline)
            {
                lines.RemoveRange(1, lines.Count - 1);
            }
            int lineHeight = 29;
            float y = textBoxAttributes.yAlign switch
            {
                VerticalAlignment.Top => 0,
                VerticalAlignment.Center => (boxHeight / 2) - (lines.Count * lineHeight / 2),
                VerticalAlignment.Bottom => boxHeight - (lines.Count * lineHeight),
                _ => 0,
            } + lineHeight;
            foreach (var (line, lineWidth) in lines)
            {
                float x = textBoxAttributes.xAlign switch
                {
                    HorizontalAlignment.Left => 0,
                    HorizontalAlignment.Center => (boxWidth / 2) - (lineWidth / 2),
                    HorizontalAlignment.Right => boxWidth - lineWidth,
                    _ => 0,
                };
                DrawLine(line, x, y, drawChar);
                y += lineHeight;
            }
        }
    }

    public struct TextBoxAttributes
    {
        public HorizontalAlignment xAlign;
        public VerticalAlignment yAlign;
        public bool multiline;
        public bool wordWrap;
    }

    public enum HorizontalAlignment
    {
        Left = 0,
        Center = 1,
        Right = 2,
        None = 3,
    }

    public enum VerticalAlignment
    {
        Top = 0,
        Center = 4,
        Bottom = 8,
        None = 12,
    }
}