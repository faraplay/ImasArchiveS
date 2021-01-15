using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace Imas.UI
{
    public class TextBox : Control
    {
        public static Font font;
        public uint textColor;
        public int lineSpacing;
        public string fontName;
        public int charLimit;
        public int textLength;
        public byte[] textBuffer;
        public string text;

        protected override void Deserialise(Stream stream)
        {
            type = 2;
            base.Deserialise(stream);

            textColor = Binary.ReadUInt32(stream, true);
            lineSpacing = Binary.ReadInt32(stream, true);
            byte[] buffer = new byte[16];
            stream.Read(buffer);
            fontName = ImasEncoding.Ascii.GetString(buffer);
            charLimit = Binary.ReadInt32(stream, true);
            textLength = Binary.ReadInt32(stream, true);

            int lengthWithPad = (2 * textLength + 15) & ~0xF;
            textBuffer = new byte[lengthWithPad];
            stream.Read(textBuffer);
            text = ImasEncoding.Custom.GetString(textBuffer);
        }

        public override void Draw(Graphics g, Matrix transform)
        {
            base.Draw(g, transform); // this changes the matrix transform

            using ImageAttributes imageAttributes = new ImageAttributes();
            float[][] colorMatrixElements = {
               new float[] {((textColor >> 16) & 0xFF) / 255f,  0,  0,  0, 0},        // red scaling factor
               new float[] {0, ((textColor >> 8) & 0xFF) / 255f,  0,  0, 0},        // green scaling factor
               new float[] {0,  0, ((textColor >> 0) & 0xFF) / 255f,  0, 0},        // blue scaling factor
               new float[] {0,  0,  0, ((textColor >> 24) & 0xFF) / 255f, 0},        // alpha scaling factor
               new float[] {0, 0, 0, 0, 1}};    
            ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);
            imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            g.TranslateTransform(0f, 18f);
            g.ScaleTransform(0.65f, 0.65f);

            if (string.IsNullOrWhiteSpace(text))
                font.DrawByteArray(g, 
                    ImasEncoding.Custom.GetBytes("placeholder text"),
                    imageAttributes);
            else
                font.DrawByteArray(g, textBuffer, imageAttributes);
        }
    }
}
