using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Imas.Gtf
{
    public partial class Font : IDisposable
    {
        // Do not rebuild big bitmap after calling this function!
        public void AddDigraphs()
        {
            char[] lowerCharset = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
            char[] asciiCharset = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~".ToCharArray();
            AddDigraphsToFont(lowerCharset, lowerCharset);
            AddSpaceDigraphsToFont(asciiCharset);
            AddAlternateASCIIToFont(asciiCharset);
        }

        // Do not rebuild big bitmap after calling this function!
        private void AddSpaceDigraphsToFont(char[] charset)
        {
            UseBigBitmap();
            List<CharData> list = new List<CharData>();
            foreach (char char1 in charset)
            {
                CharData c = Array.Find(chars, (c) => (c.key == char1));
                if (c != null && c.key < 0x80)
                    list.Add(c);
            }

            List<CharData> charsNew = new List<CharData>(chars);
            foreach (CharData c in list)
            {
                charsNew.Add(new CharData
                {
                    key = (ushort)(((c.key & 0xFF | 0x80) << 8) + 0xA0),
                    bitmap = null,
                    datawidth = c.datawidth,
                    dataheight = c.dataheight,
                    datax = c.datax,
                    datay = c.datay,
                    offsetx = c.offsetx,
                    offsety = c.offsety,
                    width = (short)(c.width + 0xA)
                });
                if (c.key != 0x20)
                {
                    charsNew.Add(new CharData
                    {
                        key = (ushort)(0xA000 + (c.key & 0xFF | 0x80)),
                        bitmap = null,
                        datawidth = c.datawidth,
                        dataheight = c.dataheight,
                        datax = c.datax,
                        datay = c.datay,
                        offsetx = (short)(c.offsetx + 0xA),
                        offsety = c.offsety,
                        width = (short)(c.width + 0xA)
                    });
                }
            }
            chars = charsNew.ToArray();
            BuildTree();
        }

        // Do not rebuild big bitmap after calling this function!
        private void AddAlternateASCIIToFont(char[] charset)
        {
            UseBigBitmap();
            List<CharData> list = new List<CharData>();
            foreach (char char1 in charset)
            {
                CharData c = Array.Find(chars, (c) => (c.key == char1));
                if (c != null && c.key < 0x80)
                    list.Add(c);
            }

            List<CharData> charsNew = new List<CharData>(chars);
            foreach (CharData c in list)
            {
                charsNew.Add(new CharData
                {
                    key = (ushort)((c.key & 0xFF | 0x80) << 8),
                    bitmap = null,
                    datawidth = c.datawidth,
                    dataheight = c.dataheight,
                    datax = c.datax,
                    datay = c.datay,
                    offsetx = c.offsetx,
                    offsety = c.offsety,
                    width = c.width
                });
            }
            chars = charsNew.ToArray();
            BuildTree();
        }

        public void AddDigraphsToFont(char[] charset1, char[] charset2)
        {
            UseCharBitmaps();
            List<CharData> list1 = new List<CharData>();
            foreach (char char1 in charset1)
            {
                CharData c1 = Array.Find(chars, (c) => (c.key == char1));
                if (c1 != null && c1.key < 0x80)
                    list1.Add(c1);
            }
            List<CharData> list2 = new List<CharData>();
            foreach (char char2 in charset2)
            {
                CharData c2 = Array.Find(chars, (c) => (c.key == char2));
                if (c2 != null && c2.key < 0x80)
                    list2.Add(c2);
            }

            List<CharData> charsNew = new List<CharData>(chars);
            foreach (CharData c1 in list1)
            {
                foreach (CharData c2 in list2)
                {
                    charsNew.Add(CreateDigraph(c1, c2));
                }
            }
            chars = charsNew.ToArray();
            BuildTree();
        }

        public void CreateDigraphs(string destDir, char[] charset1, char[] charset2)
        {
            UseCharBitmaps();
            DirectoryInfo dInfo = new DirectoryInfo(destDir);
            dInfo.Create();
            List<CharData> list1 = new List<CharData>();
            foreach (char char1 in charset1)
            {
                CharData c1 = Array.Find(chars, (c) => (c.key == char1));
                if (c1 != null && c1.key < 0x80)
                    list1.Add(c1);
            }
            List<CharData> list2 = new List<CharData>();
            foreach (char char2 in charset2)
            {
                CharData c2 = Array.Find(chars, (c) => (c.key == char2));
                if (c2 != null && c2.key < 0x80)
                    list2.Add(c2);
            }
            foreach (CharData c1 in list1)
            {
                foreach (CharData c2 in list2)
                {
                    using CharData c = CreateDigraph(c1, c2);
                    c.bitmap.Save(dInfo.FullName + "\\" + c1.key.ToString("X4") + c2.key.ToString("X4") + ".png",
                        ImageFormat.Png);
                }
            }
        }

        private CharData CreateDigraph(CharData c1, CharData c2)
        {
            int offsetxmin = Math.Min(c1.offsetx, c2.offsetx + c1.width);
            int offsetymin = Math.Min(c1.offsety, c2.offsety);
            int offsetxmax = Math.Max(c1.offsetx + c1.datawidth, c2.offsetx + c1.width + c2.datawidth);
            int offsetymax = Math.Max(c1.offsety + c1.dataheight, c2.offsety + c2.dataheight);
            int width = offsetxmax - offsetxmin;
            int height = offsetymax - offsetymin;
            Bitmap bitmap = new Bitmap(width, height);

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.DrawImage(c1.bitmap, c1.offsetx - offsetxmin, c1.offsety - offsetymin);
                graphics.DrawImage(c2.bitmap, c2.offsetx + c1.width - offsetxmin, c2.offsety - offsetymin);
            }

            //    CopyCharBitmap(bitmap, c1, c1.offsetx - offsetxmin, c1.offsety - offsetymin);
            //CopyCharBitmap(bitmap, c2, c2.offsetx + c1.width - offsetxmin, c2.offsety - offsetymin);

            return new CharData
            {
                key = (ushort)(((c1.key & 0xFF | 0x80) << 8) + (c2.key & 0xFF | 0x80)),
                bitmap = bitmap,
                datawidth = (byte)width,
                dataheight = (byte)height,
                offsetx = (short)offsetxmin,
                offsety = (short)offsetymin,
                width = (short)(c1.width + c2.width)
            };
        }
    }
}