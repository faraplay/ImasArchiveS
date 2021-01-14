using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;

namespace Imas.UI
{
    public abstract class Control
    {
        protected UIComponent parent;

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
            specialSprite = SpriteGroup.CreateFromStream(parent, stream);
        }

        public static Control Create(UIComponent parent, Stream stream)
        {
            int type = Binary.ReadInt32(stream, true);
            Control control = type switch
            {
                2 => CreateFromStream<TextBox>(parent, stream),
                4 => CreateFromStream<GroupControl>(parent, stream),
                5 => CreateFromStream<Icon>(parent, stream),
                9 => CreateFromStream<Control9>(parent, stream),
                10 => CreateFromStream<SpriteCollection>(parent, stream),
                _ => throw new InvalidDataException("Unrecognised control type"),
            };
            return control;
        }

        private static T CreateFromStream<T>(UIComponent parent, Stream stream) where T : Control, new()
        {
            T newControl = new T
            {
                parent = parent
            };
            newControl.Deserialise(stream);
            return newControl;
        }


        public virtual void Draw(Graphics g, Matrix transform)
        {
            transform.Translate(xpos, ypos);
            g.Transform = transform;
            g.DrawRectangle(Pens.Red, 0, 0, width, height);
            specialSprite.Draw(g, transform);
        }
    }
}
