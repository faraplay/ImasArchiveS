using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Imas.UI
{
    class TextBox : Control
    {
        public uint textColor;
        public int lineSpacing;
        public string fontName;
        public int charLimit;
        public int textLength;
        public string text;

        internal static TextBox CreateFromStream(Stream stream)
        {
            TextBox textBox = new TextBox();
            textBox.Deserialise(stream);
            return textBox;
        }

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
            buffer = new byte[lengthWithPad];
            stream.Read(buffer);
            text = ImasEncoding.Custom.GetString(buffer);
        }
    }
}
