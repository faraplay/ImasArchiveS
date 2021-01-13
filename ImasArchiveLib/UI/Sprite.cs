using Imas.Archive;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace Imas.UI
{
    class Sprite
    {
        // (9*4 bytes of 0s)
        public int[] start = new int[9];

        public float xpos, ypos;
        public float width, height;
        public int a1, a2;
        public float b1, b2, b3, b4;
        public int srcImageID;
        public uint ARGBMultiplier;
        public float sourceLeft, sourceTop, sourceRight, sourceBottom;

        internal static Sprite CreateFromStream(Stream stream)
        {
            Sprite sprite = new Sprite();
            sprite.Deserialise(stream);
            return sprite;
        }
        private void Deserialise(Stream stream)
        {
            Binary binary = new Binary(stream, true);
            for (int i = 0; i < 9; i++)
                start[i] = binary.ReadInt32();

            xpos = binary.ReadFloat();
            ypos = binary.ReadFloat();
            width = binary.ReadFloat();
            height = binary.ReadFloat();
            a1 = binary.ReadInt32();
            a2 = binary.ReadInt32();
            b1 = binary.ReadFloat();
            b2 = binary.ReadFloat();
            b3 = binary.ReadFloat();
            b4 = binary.ReadFloat();
            srcImageID = binary.ReadInt32();
            ARGBMultiplier = binary.ReadUInt32();
            sourceLeft = binary.ReadFloat();
            sourceTop = binary.ReadFloat();
            sourceRight = binary.ReadFloat();
            sourceBottom = binary.ReadFloat();
        }

        public void Draw(Graphics g, ImageSource imageSource, Matrix transform)
        {
            transform.Translate(xpos, ypos);
            g.Transform = transform;
            if (srcImageID >= 0)
            {
                Bitmap srcImg = imageSource[srcImageID];
                int x1 = (int)(sourceLeft * srcImg.Width);
                int y1 = (int)(sourceTop * srcImg.Height);
                int x2 = (int)(sourceRight * srcImg.Width);
                int y2 = (int)(sourceBottom * srcImg.Height);

                g.DrawImage(
                    srcImg,
                    new Rectangle(new Point(0, 0), new Size((int)width, (int)height)),
                    x1, y1,
                    x2 - x1, y2 - y1,
                    GraphicsUnit.Pixel);
            }
            else
            {
                g.FillRectangle(Brushes.White, 0, 0, width, height);
            }
        }
    }
}
