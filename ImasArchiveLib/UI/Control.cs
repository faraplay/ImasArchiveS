using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;

namespace Imas.UI
{
    class Control
    {
        public int type;
        public string name;
        public float xpos, ypos;
        public float width, height;
        public int a1, a2, a3, a4;
        public float b1, b2, b3, b4;
        public int c1, c2, c3, c4;
        public uint ARGBMultiplier;
        public float sourceLeft, sourceTop, sourceRight, sourceBottom;
        public int d1; // 7 usually
        public SpriteGroup specialSprite;

        public override string ToString() => name;

        protected virtual void Deserialise(Stream stream)
        {
            Binary binary = new Binary(stream, true);
            byte[] buffer = new byte[16];
            stream.Read(buffer);
            name = ImasEncoding.Ascii.GetString(buffer);
            xpos = binary.ReadFloat();
            ypos = binary.ReadFloat();
            width = binary.ReadFloat();
            height = binary.ReadFloat();
            a1 = binary.ReadInt32();
            a2 = binary.ReadInt32();
            a3 = binary.ReadInt32();
            a4 = binary.ReadInt32();
            b1 = binary.ReadFloat();
            b2 = binary.ReadFloat();
            b3 = binary.ReadFloat();
            b4 = binary.ReadFloat();
            c1 = binary.ReadInt32();
            c2 = binary.ReadInt32();
            c3 = binary.ReadInt32();
            c4 = binary.ReadInt32();
            ARGBMultiplier = binary.ReadUInt32();
            sourceLeft = binary.ReadFloat();
            sourceTop = binary.ReadFloat();
            sourceRight = binary.ReadFloat();
            sourceBottom = binary.ReadFloat();
            d1 = binary.ReadInt32();
            specialSprite = SpriteGroup.CreateFromStream(stream);
        }

        public static Control Create(Stream stream)
        {
            int type = Binary.ReadInt32(stream, true);
            return type switch
            {
                2 => TextBox.CreateFromStream(stream),
                4 => GroupControl.CreateFromStream(stream),
                5 => Icon.CreateFromStream(stream),
                9 => Control9.CreateFromStream(stream),
                10 => SpriteCollection.CreateFromStream(stream),
                _ => throw new InvalidDataException("Unrecognised control type"),
            };
        }

        public virtual void Draw(Graphics g, ImageSource imageSource, Matrix transform)
        {
            transform.Translate(xpos, ypos);
            g.Transform = transform;
            g.DrawRectangle(Pens.Red, 0, 0, width, height);
            specialSprite.Draw(g, imageSource, transform);
        }
    }
}
