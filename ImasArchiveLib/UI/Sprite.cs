using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace Imas.UI
{
    public class Sprite : UIElement
    {
        [SerialiseField(0, FixedCount = 9)]
        // (9*4 bytes of 0s)
        public int[] start = new int[9];
        [Listed(0)]
        public int Start0 { get => start[0]; set => start[0] = value; }
        [Listed(1)]
        public int Start1 { get => start[1]; set => start[1] = value; }
        [Listed(2)]
        public int Start2 { get => start[2]; set => start[2] = value; }
        [Listed(3)]
        public int Start3 { get => start[3]; set => start[3] = value; }
        [Listed(4)]
        public int Start4 { get => start[4]; set => start[4] = value; }
        [Listed(5)]
        public int Start5 { get => start[5]; set => start[5] = value; }
        [Listed(6)]
        public int Start6 { get => start[6]; set => start[6] = value; }
        [Listed(7)]
        public int Start7 { get => start[7]; set => start[7] = value; }
        [Listed(8)]
        public int Start8 { get => start[8]; set => start[8] = value; }

        [SerialiseField(9)]
        public float xpos;
        [Listed(9)]
        public float Xpos { get => xpos; set => xpos = value; }
        [SerialiseField(10)]
        public float ypos;
        [Listed(10)]
        public float Ypos { get => ypos; set => ypos = value; }
        [SerialiseField(11)]
        public float width;
        [Listed(11)]
        public float Width { get => width; set => width = value; }
        [SerialiseField(12)]
        public float height;
        [Listed(12)]
        public float Height { get => height; set => height = value; }

        [SerialiseField(13)]
        public int a1;
        [Listed(13)]
        public int A1 { get => a1; set => a1 = value; }
        [SerialiseField(14)]
        public int a2;
        [Listed(14)]
        public int A2 { get => a2; set => a2 = value; }
        [SerialiseField(15)]
        public float b1;
        [Listed(15)]
        public float B1 { get => b1; set => b1 = value; }
        [SerialiseField(16)]
        public float b2;
        [Listed(16)]
        public float B2 { get => b2; set => b2 = value; }
        [SerialiseField(17)]
        public float b3;
        [Listed(17)]
        public float B3 { get => b3; set => b3 = value; }
        [SerialiseField(18)]
        public float b4;
        [Listed(18)]
        public float B4 { get => b4; set => b4 = value; }
        [SerialiseField(19)]
        public int srcImageID;
        [Listed(19)]
        public int SrcImageID { get => srcImageID; set => srcImageID = value; }

        [SerialiseField(20)]
        public byte alpha;
        [Listed(20)]
        public byte Alpha { get => alpha; set => alpha = value; }
        [SerialiseField(21)]
        public byte red;
        [Listed(21)]
        public byte Red { get => red; set => red = value; }
        [SerialiseField(22)]
        public byte green;
        [Listed(22)]
        public byte Green { get => green; set => green = value; }
        [SerialiseField(23)]
        public byte blue;
        [Listed(23)]
        public byte Blue { get => blue; set => blue = value; }

        [SerialiseField(24)]
        public float srcFracLeft;
        [Listed(24)]
        public float SrcFracLeft { get => srcFracLeft; set => srcFracLeft = value; }
        [SerialiseField(25)]
        public float srcFracTop;
        [Listed(25)]
        public float SrcFracTop { get => srcFracTop; set => srcFracTop = value; }
        [SerialiseField(26)]
        public float srcFracRight;
        [Listed(26)]
        public float SrcFracRight { get => srcFracRight; set => srcFracRight = value; }
        [SerialiseField(27)]
        public float srcFracBottom;
        [Listed(27)]
        public float SrcFracBottom { get => srcFracBottom; set => srcFracBottom = value; }

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
