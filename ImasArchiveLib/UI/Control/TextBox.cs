using Imas.Gtf;
using System;

namespace Imas.UI
{
    [SerialisationDerivedType(2)]
    public class TextBox : Control
    {
        [SerialiseProperty(100)]
        [Listed(100)]
        public byte TextAlpha { get; set; }
        [SerialiseProperty(101)]
        [Listed(101)]
        public byte TextRed { get; set; }
        [SerialiseProperty(102)]
        [Listed(102)]
        public byte TextGreen { get; set; }
        [SerialiseProperty(103)]
        [Listed(103)]
        public byte TextBlue { get; set; }

        [SerialiseProperty(104)]
        public uint TextAttributes { get; set; }

        [SerialiseProperty(105)]
        public byte[] FontNameBuffer { get; set; } = new byte[16];
        [Listed(105)]
        public string FontName
        {
            get => ImasEncoding.Ascii.GetString(FontNameBuffer);
            set => ImasEncoding.Ascii.GetBytes(value, FontNameBuffer);
        }

        [SerialiseProperty(106)]
        [Listed(106)]
        public int CharLimit { get; set; }

        private int _textLength;
        [SerialiseProperty(107)]
        [Listed(107)]
        public int TextLength
        {
            get => _textLength;
            set
            {
                _textLength = value;
                if (TextBufferLength == TextBuffer.Length)
                    return;
                byte[] newTextBuffer = new byte[TextBufferLength];
                Array.Copy(TextBuffer, newTextBuffer, Math.Min(TextBuffer.Length, newTextBuffer.Length));
                TextBuffer = newTextBuffer;
            }
        }
        public int TextBufferLength
        {
            get => (2 * TextLength + 15) & ~0xF;
        }

        [SerialiseProperty(108)]
        public byte[] TextBuffer { get; set; } = new byte[16];
        [Listed(108, StringMultiline = true)]
        public string Text
        {
            get => ImasEncoding.Custom.GetString(TextBuffer);
            set
            {
                byte[] newBytes = ImasEncoding.Custom.GetBytes(value);
                TextLength = newBytes.Length / 2;
                if (TextLength > CharLimit)
                {
                    CharLimit = TextLength;
                }
                TextBuffer = new byte[TextBufferLength];
                Array.Copy(newBytes, TextBuffer, 2 * TextLength);
            }
        }

        [Listed(109)]
        public HorizontalAlignment XAlignment
        {
            get => (HorizontalAlignment)(TextAttributes & 0x03u);
            set
            {
                TextAttributes = (TextAttributes & ~0x3u) ^ ((uint)value & 0x3u);
            }
        }
        [Listed(110)]
        public VerticalAlignment YAlignment
        {
            get => (VerticalAlignment)(TextAttributes & 0xCu);
            set
            {
                TextAttributes = (TextAttributes & ~0xCu) ^ ((uint)value & 0xCu);
            }
        }
        [Listed(111)]
        public bool Multiline
        {
            get => (TextAttributes & 0x10u) != 0;
            set
            {
                if (value)
                    TextAttributes |= 0x10u;
                else
                    TextAttributes &= ~0x10u;
            }
        }
        [Listed(112)]
        public bool WordWrap
        {
            get => (TextAttributes & 0x20u) != 0;
            set
            {
                if (value)
                    TextAttributes |= 0x20u;
                else
                    TextAttributes &= ~0x20u;
            }
        }
    }
}
