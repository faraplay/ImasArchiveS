using Imas.Gtf;
using System;

namespace Imas.UI
{
    [SerialisationDerivedType(2)]
    public class TextBox : Control
    {
        [SerialiseField(100)]
        public byte textAlpha;
        [Listed(100)]
        public byte TextAlpha { get => textAlpha; set => textAlpha = value; }
        [SerialiseField(101)]
        public byte textRed;
        [Listed(101)]
        public byte TextRed { get => textRed; set => textRed = value; }
        [SerialiseField(102)]
        public byte textGreen;
        [Listed(102)]
        public byte TextGreen { get => textGreen; set => textGreen = value; }
        [SerialiseField(103)]
        public byte textBlue;
        [Listed(103)]
        public byte TextBlue { get => textBlue; set => textBlue = value; }

        [SerialiseField(104)]
        public uint textAttributes;
        [Listed(104)]
        public uint TextAttributes { get => textAttributes; set => textAttributes = value; }

        [SerialiseField(105, FixedCount = 16)]
        public byte[] fontNameBuffer;
        [Listed(105)]
        public string FontName
        {
            get => ImasEncoding.Ascii.GetString(fontNameBuffer);
            set => ImasEncoding.Ascii.GetBytes(value, fontNameBuffer);
        }

        [SerialiseField(106)]
        public int charLimit;
        [Listed(106)]
        public int CharLimit { get => charLimit; set => charLimit = value; }
        [SerialiseField(107)]
        public int textLength;
        [Listed(107)]
        public int TextLength { get => textLength; set => textLength = value; }

        [SerialiseField(108, CountProperty = nameof(TextBufferLength))]
        public byte[] textBuffer;
        [Listed(108)]
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
        public int TextBufferLength
        {
            get => (2 * textLength + 15) & ~0xF;
        }
    }
}
