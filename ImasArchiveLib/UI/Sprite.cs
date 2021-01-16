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
    public class Sprite : UIElement
    {
        // (9*4 bytes of 0s)
        public int[] start = new int[9];

        public float xpos, ypos;
        public float width, height;
        public int a1, a2;
        public float b1, b2, b3, b4;
        public int srcImageID;
        public byte alpha, red, green, blue;
        public float srcFracLeft, srcFracTop, srcFracRight, srcFracBottom;

        private Bitmap SourceImage => parent.imageSource[srcImageID];
        private int SrcImgWidth => (srcImageID == -1) ? 1 : SourceImage.Width;
        private int SrcImgHeight => (srcImageID == -1) ? 1 : SourceImage.Height;
        public float SourceX
        {
            get => srcFracLeft * SrcImgWidth;
            set
            {
                float srcFracWidth = srcFracRight - srcFracLeft;
                srcFracLeft = value / SrcImgWidth;
                srcFracRight = srcFracLeft + srcFracWidth;
            }
        }
        public float SourceY
        {
            get => srcFracTop * SrcImgHeight;
            set
            {
                float srcFracHeight = srcFracBottom - srcFracTop;
                srcFracTop = value / SrcImgHeight;
                srcFracBottom = srcFracTop + srcFracHeight;
            }
        }
        public float SourceWidth
        {
            get => (srcFracRight - srcFracLeft) * SrcImgWidth;
            set
            {
                srcFracRight = srcFracLeft + (value / SrcImgWidth);
            }
        }
        public float SourceHeight
        {
            get => (srcFracBottom - srcFracTop) * SrcImgHeight;
            set
            {
                srcFracBottom = srcFracTop + (value / SrcImgHeight);
            }
        }

        internal static Sprite CreateFromStream(UISubcomponent parent, Stream stream)
        {
            Sprite sprite = new Sprite
            {
                parent = parent
            };
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
            alpha = binary.ReadByte();
            red = binary.ReadByte();
            green = binary.ReadByte();
            blue = binary.ReadByte();
            srcFracLeft = binary.ReadFloat();
            srcFracTop = binary.ReadFloat();
            srcFracRight = binary.ReadFloat();
            srcFracBottom = binary.ReadFloat();
        }

        public void Serialise(Stream stream)
        {
            Binary binary = new Binary(stream, true);
            for (int i = 0; i < 9; i++)
                binary.WriteInt32(start[i]);

            binary.WriteFloat(xpos);
            binary.WriteFloat(ypos);
            binary.WriteFloat(width);
            binary.WriteFloat(height);
            binary.WriteInt32(a1);
            binary.WriteInt32(a2);
            binary.WriteFloat(b1);
            binary.WriteFloat(b2);
            binary.WriteFloat(b3);
            binary.WriteFloat(b4);
            binary.WriteInt32(srcImageID);
            binary.WriteByte(alpha);
            binary.WriteByte(red);
            binary.WriteByte(green);
            binary.WriteByte(blue);
            binary.WriteFloat(srcFracLeft);
            binary.WriteFloat(srcFracTop);
            binary.WriteFloat(srcFracRight);
            binary.WriteFloat(srcFracBottom);
        }

        public override void Draw(Graphics g, Matrix transform, ColorMatrix color)
        {
            transform.Translate(xpos, ypos);
            g.Transform = transform;

            using ImageAttributes imageAttributes = new ImageAttributes();
            ColorMatrix newColor = ScaleMatrix(color, alpha, red, green, blue);
            imageAttributes.SetColorMatrix(newColor, ColorMatrixFlag.Default, ColorAdjustType.Default);
            if (srcImageID == -1)
            {
                Brush brush = new SolidBrush(Color.FromArgb(
                    (int)(newColor.Matrix33 * 255),
                    (int)(newColor.Matrix00 * 255),
                    (int)(newColor.Matrix11 * 255),
                    (int)(newColor.Matrix22 * 255)));
                g.FillRectangle(brush, 0, 0, width, height);
            }
            else
            {
                if (start[0] != 1)
                {
                    g.DrawImage(
                        SourceImage,
                        new Rectangle(new Point(0, 0), new Size((int)width, (int)height)),
                        SourceX, SourceY,
                        SourceWidth, SourceHeight,
                        GraphicsUnit.Pixel,
                        imageAttributes);
                }
                else
                {
                    imageAttributes.SetWrapMode(WrapMode.Tile);
                    TextureBrush textureBrush = new TextureBrush(SourceImage,
                        new RectangleF(SourceX, SourceY, SourceWidth, SourceHeight),
                        imageAttributes);
                    g.FillRectangle(
                        textureBrush,
                        new Rectangle(new Point(0, 0), new Size((int)width, (int)height))
                        );
                }
            }
        }

        public Bitmap ShowInSpriteSheet()
        {
            if (srcImageID == -1)
            {
                Bitmap bitmap = new Bitmap(1, 1);
                bitmap.SetPixel(0, 0, Color.White);
                return bitmap;
            }
            else
            {
                Bitmap bitmap = new Bitmap(SrcImgWidth, SrcImgHeight);
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.DrawImage(SourceImage, new Point());
                    g.DrawRectangle(Pens.Red, SourceX, SourceY, SourceWidth - 1, SourceHeight - 1);
                }
                return bitmap;
            }
        }
    }
}
