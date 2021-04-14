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

        [SerialiseField(0)]
        public int start0;
        [SerialiseField(1)]
        public int start1;
        [SerialiseField(2)]
        public int start2;
        [SerialiseField(3)]
        public int start3;
        [SerialiseField(4)]
        public int start4;
        [SerialiseField(5)]
        public int start5;
        [SerialiseField(6)]
        public int start6;
        [SerialiseField(7)]
        public int start7;
        [SerialiseField(8)]
        public int start8;

        [SerialiseField(9)]
        public float xpos;
        [SerialiseField(10)]
        public float ypos;
        [SerialiseField(11)]
        public float width;
        [SerialiseField(12)]
        public float height;

        [SerialiseField(13)]
        public int a1;
        [SerialiseField(14)]
        public int a2;
        [SerialiseField(15)]
        public float b1;
        [SerialiseField(16)]
        public float b2;
        [SerialiseField(17)]
        public float b3;
        [SerialiseField(18)]
        public float b4;
        [SerialiseField(19)]
        public int srcImageID;

        [SerialiseField(20)]
        public byte alpha;
        [SerialiseField(21)]
        public byte red;
        [SerialiseField(20)]
        public byte green;
        [SerialiseField(20)]
        public byte blue;

        [SerialiseField(21)]
        public float srcFracLeft;
        [SerialiseField(22)]
        public float srcFracTop;
        [SerialiseField(23)]
        public float srcFracRight;
        [SerialiseField(24)]
        public float srcFracBottom;

        private Bitmap SourceImage => subcomponent.imageSource[srcImageID];
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

        protected override void Deserialise(Stream stream)
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

        public override void Serialise(Stream stream)
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
                    using Bitmap texture = new Bitmap((int)Math.Abs(SourceWidth), (int)Math.Abs(SourceHeight));
                    using (Graphics g2 = Graphics.FromImage(texture))
                    {
                        g2.DrawImage(SourceImage,
                            (SourceWidth < 0) ? Math.Abs(SourceWidth) : 0,
                            (SourceHeight < 0) ? Math.Abs(SourceHeight) : 0, 
                            new RectangleF(SourceX, SourceY, SourceWidth, SourceHeight), GraphicsUnit.Pixel);
                    }
                    TextureBrush textureBrush = new TextureBrush(texture,
                        new RectangleF(0, 0, Math.Abs(SourceWidth), Math.Abs(SourceHeight)),
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
