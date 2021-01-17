using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace Imas.UI
{
    public abstract class UIElement
    {
        protected UISubcomponent subcomponent;
        protected UIElement parent;

        public bool myVisible = true;

        protected static T CreateFromStream<T>(UISubcomponent subcomponent, UIElement parent, Stream stream) where T : UIElement, new()
        {
            T newControl = new T
            {
                subcomponent = subcomponent,
                parent = parent
            };
            newControl.Deserialise(stream);
            return newControl;
        }

        protected abstract void Deserialise(Stream stream);
        public abstract void Serialise(Stream stream);

        public abstract void Draw(Graphics g, Matrix transform, ColorMatrix color);
        public void DrawIfVisible(Graphics g, Matrix transform, ColorMatrix color)
        {
            if (myVisible)
                Draw(g, transform, color);
        }

        public void Draw(Graphics g)
        {
            using Matrix matrix = new Matrix();
            DrawIfVisible(g, matrix, Identity);
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
