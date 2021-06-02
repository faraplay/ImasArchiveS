using System;
using System.Collections.Generic;
using System.Drawing;
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
                    paddedBbWidth = c.paddedBbWidth,
                    paddedBbHeight = c.paddedBbHeight,
                    dataX = c.dataX,
                    dataY = c.dataY,
                    bearingX = c.bearingX,
                    bearingY = c.bearingY,
                    advance = (short)(c.advance + 0xA)
                });
                if (c.key != 0x20)
                {
                    charsNew.Add(new CharData
                    {
                        key = (ushort)(0xA000 + (c.key & 0xFF | 0x80)),
                        paddedBbWidth = c.paddedBbWidth,
                        paddedBbHeight = c.paddedBbHeight,
                        dataX = c.dataX,
                        dataY = c.dataY,
                        bearingX = (short)(c.bearingX + 0xA),
                        bearingY = c.bearingY,
                        advance = (short)(c.advance + 0xA)
                    });
                }
            }
            chars = charsNew.ToArray();
            BuildTree();
        }

        // Do not rebuild big bitmap after calling this function!
        private void AddAlternateASCIIToFont(char[] charset)
        {
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
                    paddedBbWidth = c.paddedBbWidth,
                    paddedBbHeight = c.paddedBbHeight,
                    dataX = c.dataX,
                    dataY = c.dataY,
                    bearingX = c.bearingX,
                    bearingY = c.bearingY,
                    advance = c.advance
                });
            }
            chars = charsNew.ToArray();
            BuildTree();
        }

        public void AddDigraphsToFont(char[] charset1, char[] charset2)
        {
            var charBitmaps = GetCharBitmaps();
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
                    (CharData newC, int[] newPixelData) = CreateDigraph(
                        c1, charBitmaps[c1.key],
                        c2, charBitmaps[c2.key]);
                    charBitmaps.Add(newC.key, newPixelData);
                    charsNew.Add(newC);
                }
            }
            chars = charsNew.ToArray();
            RebuildMyBitmap(charBitmaps, chars);
            BuildTree();
        }

        public void CreateDigraphs(string destDir, char[] charset1, char[] charset2)
        {
            var charBitmaps = GetCharBitmaps();
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
                    (CharData newC, int[] newPixelData) = CreateDigraph(
                        c1, charBitmaps[c1.key],
                        c2, charBitmaps[c2.key]);
                    using FileStream fileStream = new FileStream($"{dInfo.FullName}\\{c1.key:X4}{c2.key:X4}.png", FileMode.Create);
                    SavePngFromPixelData(fileStream, newPixelData, newC.paddedBbWidth, newC.paddedBbHeight);
                }
            }
        }

        private (CharData, int[]) CreateDigraph(CharData c1, int[] pix1, CharData c2, int[] pix2)
        {
            int offsetxmin = Math.Min(c1.bearingX, c2.bearingX + c1.advance);
            int offsetymin = Math.Min(c1.bearingY, c2.bearingY);
            int offsetxmax = Math.Max(c1.bearingX + c1.paddedBbWidth, c2.bearingX + c1.advance + c2.paddedBbWidth);
            int offsetymax = Math.Max(c1.bearingY + c1.paddedBbHeight, c2.bearingY + c2.paddedBbHeight);
            int width = offsetxmax - offsetxmin;
            int height = offsetymax - offsetymin;

            int[] newPixelData = new int[width * height];

            int x = c1.bearingX - offsetxmin;
            int y = c1.bearingY - offsetymin;
            for (int yy = 0; yy < c1.paddedBbHeight; ++yy)
            {
                for (int xx = 0; xx < c1.paddedBbWidth; ++xx)
                {
                    newPixelData[(y + yy) * width + (x + xx)] = pix1[yy * c1.paddedBbWidth + xx];
                }
            }
            x = c2.bearingX - offsetxmin + c1.advance;
            y = c2.bearingY - offsetymin;
            for (int yy = 0; yy < c2.paddedBbHeight; ++yy)
            {
                for (int xx = 0; xx < c2.paddedBbWidth; ++xx)
                {
                    Color colorBottom = Color.FromArgb(newPixelData[(y + yy) * width + (x + xx)]);
                    Color colorTop = Color.FromArgb(pix2[yy * c2.paddedBbWidth + xx]);
                    float bottomAFrac = (colorBottom.A / 255f);
                    float topAFrac = (colorTop.A / 255f);
                    float newA = (colorBottom.A * (1 - topAFrac)) + colorTop.A;
                    float newR = (colorBottom.R * bottomAFrac * (1f - topAFrac)) + (colorTop.R * topAFrac);
                    float newG = (colorBottom.G * bottomAFrac * (1f - topAFrac)) + (colorTop.G * topAFrac);
                    float newB = (colorBottom.B * bottomAFrac * (1f - topAFrac)) + (colorTop.B * topAFrac);
                    float newAFrac = newA / 255f;
                    Color newColor = newA == 0 ? Color.Transparent : Color.FromArgb(
                        (int)newA,
                        (int)(newR / newAFrac),
                        (int)(newG / newAFrac),
                        (int)(newB / newAFrac));
                    newPixelData[(y + yy) * width + (x + xx)] = newColor.ToArgb();
                }
            }

            return (new CharData
            {
                key = (ushort)(((c1.key & 0xFF | 0x80) << 8) + (c2.key & 0xFF | 0x80)),
                paddedBbWidth = (byte)width,
                paddedBbHeight = (byte)height,
                bearingX = (short)offsetxmin,
                bearingY = (short)offsetymin,
                advance = (short)(c1.advance + c2.advance)
            }, newPixelData);
        }
    }
}