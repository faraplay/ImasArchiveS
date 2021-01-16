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
        public byte textAlpha, textRed, textGreen, textBlue;
        public uint textAttributes;
        public byte[] fontNameBuffer;
        public int charLimit;
        public int textLength;
        public byte[] textBuffer;

        public HorizontalAlignment XAlignment
        {
            get => (HorizontalAlignment)(textAttributes & 0x03u);
            set
            {
                textAttributes = (textAttributes & ~0x3u) ^ ((uint)value & 0x3u);
            }
        }
        public VerticalAlignment YAlignment
        {
            get => (VerticalAlignment)(textAttributes & 0xCu);
            set
            {
                textAttributes = (textAttributes & ~0xCu) ^ ((uint)value & 0xCu);
            }
        }
        public bool Multiline
        {
            get => (textAttributes & 0x10u) != 0;
            set
            {
                if (value)
                    textAttributes |= 0x10u;
                else
                    textAttributes &= ~0x10u;
            }
        }
        public bool WordWrap
        {
            get => (textAttributes & 0x20u) != 0;
            set
            {
                if (value)
                    textAttributes |= 0x20u;
                else
                    textAttributes &= ~0x20u;
            }
        }
        public string FontName
        {
            get => ImasEncoding.Ascii.GetString(fontNameBuffer);
            set => ImasEncoding.Ascii.GetBytes(value, fontNameBuffer);
        }
        public string Text
        {
            get => ImasEncoding.Custom.GetString(textBuffer);
            set
            {
                byte[] newBytes = ImasEncoding.Custom.GetBytes(value);
                textLength = newBytes.Length / 2;
                if (textLength > charLimit)
                {
                    charLimit = textLength;
                }
                int lengthWithPad = (2 * textLength + 15) & ~0xF;
                textBuffer = new byte[lengthWithPad];
                Array.Copy(newBytes, textBuffer, 2 * textLength);
            }
        }

        protected override void Deserialise(Stream stream)
        {
            type = 2;
            base.Deserialise(stream);

            textAlpha = Binary.ReadByte(stream, true);
            textRed = Binary.ReadByte(stream, true);
            textGreen = Binary.ReadByte(stream, true);
            textBlue = Binary.ReadByte(stream, true);
            textAttributes = Binary.ReadUInt32(stream, true);
            fontNameBuffer = new byte[16];
            stream.Read(fontNameBuffer);
            charLimit = Binary.ReadInt32(stream, true);
            textLength = Binary.ReadInt32(stream, true);

            int lengthWithPad = (2 * textLength + 15) & ~0xF;
            textBuffer = new byte[lengthWithPad];
            stream.Read(textBuffer);
        }

        public override void Serialise(Stream stream)
        {
            base.Serialise(stream);

            Binary.WriteByte(stream, true, textAlpha);
            Binary.WriteByte(stream, true, textRed);
            Binary.WriteByte(stream, true, textGreen);
            Binary.WriteByte(stream, true, textBlue);
            Binary.WriteUInt32(stream, true, textAttributes);
            stream.Write(fontNameBuffer);
            Binary.WriteInt32(stream, true, charLimit);
            Binary.WriteInt32(stream, true, textLength);
            stream.Write(textBuffer);
        }

        public override void Draw(Graphics g, Matrix transform, ColorMatrix color)
        {
            base.Draw(g, transform, color); // this changes the matrix transform but not the color

            using ImageAttributes imageAttributes = new ImageAttributes();
            ColorMatrix newColor = ScaleMatrix(color, alpha, red, green, blue);
            newColor = ScaleMatrix(newColor, textAlpha, textRed, textGreen, textBlue);
            imageAttributes.SetColorMatrix(newColor, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            TextBoxAttributes textBoxAttributes = new TextBoxAttributes { xAlign = XAlignment, yAlign = YAlignment, multiline = Multiline, wordWrap = WordWrap };

            if (string.IsNullOrWhiteSpace(Text))
            {
                byte[] placeholderText = new byte[2 * charLimit];
                Array.Fill(placeholderText, (byte)0xE1);
                font.DrawByteArray(g,
                    placeholderText,
                    imageAttributes,
                    width, height, textBoxAttributes);
            }
            else
                font.DrawByteArray(g, textBuffer, imageAttributes,
                    width, height, textBoxAttributes);
        }
    }
}
