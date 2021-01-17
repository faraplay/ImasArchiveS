using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace Imas.UI
{
    public abstract class Control : UIElement
    {

        public int type;
        public byte[] nameBuffer;
        public float xpos, ypos;
        public float width, height;
        public int a1, a2, a3, a4;
        public float b1, b2, b3, b4;
        public int c1, c2, c3, c4;
        public byte alpha, red, green, blue;
        public float scaleX, scaleY;
        public float sourceRight, sourceBottom;
        public int d1;
        public SpriteGroup specialSprite;
        public int[] extData;

        public string Name
        {
            get => ImasEncoding.Ascii.GetString(nameBuffer);
            set => ImasEncoding.Ascii.GetBytes(value, nameBuffer);
        }

        public bool HasExtData => (d1 & 0x04000000) != 0;

        public override string ToString() => Name;

        protected override void Deserialise(Stream stream)
        {
            Binary binary = new Binary(stream, true);
            nameBuffer = new byte[16];
            stream.Read(nameBuffer);
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
            alpha = binary.ReadByte();
            red = binary.ReadByte();
            green = binary.ReadByte();
            blue = binary.ReadByte();
            scaleX = binary.ReadFloat();
            scaleY = binary.ReadFloat();
            sourceRight = binary.ReadFloat();
            sourceBottom = binary.ReadFloat();
            d1 = binary.ReadInt32();
            specialSprite = CreateFromStream<SpriteGroup>(subcomponent, this, stream);

            extData = new int[4];
            if (d1 >> 16 != 0)
            {
                if (HasExtData)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        extData[i] = binary.ReadInt32();
                    }
                }
            }
        }

        public override void Serialise(Stream stream)
        {
            Binary binary = new Binary(stream, true);
            binary.WriteInt32(type);
            stream.Write(nameBuffer);
            binary.WriteFloat(xpos);
            binary.WriteFloat(ypos);
            binary.WriteFloat(width);
            binary.WriteFloat(height);
            binary.WriteInt32(a1);
            binary.WriteInt32(a2);
            binary.WriteInt32(a3);
            binary.WriteInt32(a4);
            binary.WriteFloat(b1);
            binary.WriteFloat(b2);
            binary.WriteFloat(b3);
            binary.WriteFloat(b4);
            binary.WriteInt32(c1);
            binary.WriteInt32(c2);
            binary.WriteInt32(c3);
            binary.WriteInt32(c4);
            binary.WriteByte(alpha);
            binary.WriteByte(red);
            binary.WriteByte(green);
            binary.WriteByte(blue);
            binary.WriteFloat(scaleX);
            binary.WriteFloat(scaleY);
            binary.WriteFloat(sourceRight);
            binary.WriteFloat(sourceBottom);
            binary.WriteInt32(d1);
            specialSprite.Serialise(stream);
            if (HasExtData)
            {
                for (int i = 0; i < 4; i++)
                {
                     binary.WriteInt32(extData[i]);
                }
            }
        }

        public static Control Create(UISubcomponent subcomponent, UIElement parent, Stream stream)
        {
            int type = Binary.ReadInt32(stream, true);
            Control control = type switch
            {
                2 => CreateFromStream<TextBox>(subcomponent, parent, stream),
                3 => CreateFromStream<Control3>(subcomponent, parent, stream),
                4 => CreateFromStream<GroupControl>(subcomponent, parent, stream),
                5 => CreateFromStream<Icon>(subcomponent, parent, stream),
                6 => CreateFromStream<ScrollControl>(subcomponent, parent, stream),
                9 => CreateFromStream<Control9>(subcomponent, parent, stream),
                10 => CreateFromStream<SpriteCollection>(subcomponent, parent, stream),
                _ => throw new InvalidDataException("Unrecognised control type"),
            };
            return control;
        }

        public override void Draw(Graphics g, Matrix transform, ColorMatrix color)
        {
            transform.Translate(xpos, ypos);
            transform.Scale(scaleX == 0 ? 1 : scaleX, scaleY);
            g.Transform = transform;
            specialSprite.DrawIfVisible(g, transform, ScaleMatrix(color, alpha, red, green, blue));
        }
    }
}
