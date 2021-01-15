using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace Imas.UI
{
    public abstract class Control
    {
        protected UISubcomponent parent;

        public int type;
        public string name;
        public float xpos, ypos;
        public float width, height;
        public int a1, a2, a3, a4;
        public float b1, b2, b3, b4;
        public int c1, c2, c3, c4;
        public byte alpha, red, green, blue;
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
            alpha = binary.ReadByte();
            red = binary.ReadByte();
            green = binary.ReadByte();
            blue = binary.ReadByte();
            sourceLeft = binary.ReadFloat();
            sourceTop = binary.ReadFloat();
            sourceRight = binary.ReadFloat();
            sourceBottom = binary.ReadFloat();
            d1 = binary.ReadInt32();
            specialSprite = SpriteGroup.CreateFromStream(parent, stream);
        }

        public static Control Create(UISubcomponent parent, Stream stream)
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

        private static T CreateFromStream<T>(UISubcomponent parent, Stream stream) where T : Control, new()
        {
            T newControl = new T
            {
                parent = parent
            };
            newControl.Deserialise(stream);
            return newControl;
        }

        public void Draw(Graphics g)
        {
            using Matrix matrix = new Matrix();
            Draw(g, matrix, Identity);
        }

        public virtual void Draw(Graphics g, Matrix transform, ColorMatrix color)
        {
            transform.Translate(xpos, ypos);
            g.Transform = transform;
            g.DrawRectangle(Pens.Red, 0, 0, width, height);
            color = ScaleMatrix(color, alpha, red, green, blue);
            specialSprite.Draw(g, transform, color);
        }

        public Bitmap GetBitmap()
        {
            Bitmap bitmap = new Bitmap(1280, 720);
            using Graphics g = Graphics.FromImage(bitmap);
            Draw(g);
            return bitmap;
        }

        #region ColorMatrix

        public static ColorMatrix Identity 
        {
            get
            {
                float[][] colorMatrixElements = {
               new float[] {1, 0, 0, 0, 0},        // red scaling factor
               new float[] {0, 1, 0, 0, 0},        // green scaling factor
               new float[] {0, 0, 1, 0, 0},        // blue scaling factor
               new float[] {0, 0, 0, 1, 0},        // alpha scaling factor
               new float[] {0, 0, 0, 0, 1}};
                return new ColorMatrix(colorMatrixElements);
            }
        }

        public static ColorMatrix ScaleMatrix(ColorMatrix colorMatrix, byte a, byte r, byte g, byte b)
        {
            float[][] colorMatrixElements = {
               new float[] {colorMatrix.Matrix00 * r / 255f,  0,  0,  0, 0},        // red scaling factor
               new float[] {0, colorMatrix.Matrix11 * g / 255f,  0,  0, 0},        // green scaling factor
               new float[] {0,  0, colorMatrix.Matrix22 * b / 255f,  0, 0},        // blue scaling factor
               new float[] {0,  0,  0, colorMatrix.Matrix33 * a / 255f, 0},        // alpha scaling factor
               new float[] {0, 0, 0, 0, 1}};
            return new ColorMatrix(colorMatrixElements);
        }

        #endregion ColorMatrix
    }
}
