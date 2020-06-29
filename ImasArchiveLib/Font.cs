using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace ImasArchiveLib
{
    class Font : IDisposable
    {
        CharData[] chars;
        int root;

        private readonly byte[] zeros = new byte[256];
        byte[] parHeader;
        public void ReadFontPar(Stream stream)
        {
            parHeader = new byte[16];
            stream.Read(parHeader);
            int gtfPos = Utils.GetInt32(stream);
            int nxPos = Utils.GetInt32(stream);
            int nxpPos = Utils.GetInt32(stream);

            stream.Position = gtfPos;
            BigBitmap = GTF.ReadGTF(stream);

            stream.Position = nxpPos + 8;
            int charCount = Utils.GetInt32(stream);
            root = Utils.GetInt32(stream);
            chars = new CharData[charCount];
            stream.Position = nxpPos + 48;
            for (int i = 0; i < charCount; i++)
            {
                chars[i] = new CharData
                {
                    index = i,
                    key = Utils.GetUShort(stream),
                    datawidth = Utils.GetByte(stream),
                    dataheight = Utils.GetByte(stream),
                    datax = Utils.GetInt16(stream),
                    datay = Utils.GetInt16(stream),
                    offsetx = Utils.GetInt16(stream),
                    offsety = Utils.GetInt16(stream),
                    width = Utils.GetInt16(stream),
                    blank = Utils.GetInt16(stream),
                    left = Utils.GetInt32(stream),
                    right = Utils.GetInt32(stream),
                    isEmoji = Utils.GetUShort(stream)
                };
                stream.Position += 6;
            }
        }
        public void WriteFontPar(Stream stream)
        {
            UseBigBitmap();
            BuildTree();

            long pos = stream.Position;
            stream.Write(parHeader);

            int gtfPos = 0x200;
            int gtfLen = 0x800080;
            int gtfPad = (-gtfLen) & 0x7F;
            int nxPos = gtfPos + gtfLen + gtfPad;
            int nxLen = 0x30 + 0x20 * chars.Length;
            int nxPad = (-nxLen) & 0x7F;
            int nxpPos = nxPos + nxLen + nxPad;
            int nxpLen = 0x30 + 0x20 * chars.Length;
            int nxpPad = (-nxpLen) & 0x7F;

            Utils.PutInt32(stream, gtfPos);
            Utils.PutInt32(stream, nxPos);
            Utils.PutInt32(stream, nxpPos);
            Utils.PutInt32(stream, 0);

            string gtfName = "im2nx.gtf";
            string nxName = "im2nx.paf";
            string nxpName = "im2nxp.paf";
            stream.Write(Encoding.ASCII.GetBytes(gtfName));
            stream.Write(zeros, 0, 0x80 - gtfName.Length);
            stream.Write(Encoding.ASCII.GetBytes(nxName));
            stream.Write(zeros, 0, 0x80 - nxName.Length);
            stream.Write(Encoding.ASCII.GetBytes(nxpName));
            stream.Write(zeros, 0, 0x80 - nxpName.Length);

            Utils.PutUInt(stream, 0);
            Utils.PutUInt(stream, 1);
            Utils.PutUInt(stream, 2);
            Utils.PutUInt(stream, 0);

            Utils.PutInt32(stream, gtfLen);
            Utils.PutInt32(stream, nxLen);
            Utils.PutInt32(stream, nxpLen);
            Utils.PutInt32(stream, 0);

            int pad = (int)(pos - stream.Position) & 0x7F;
            stream.Write(zeros, 0, pad);

            GTF.WriteGTF(stream, BigBitmap, 0x83);
            stream.Write(zeros, 0, gtfPad);

            WritePaf(stream, false);
            stream.Write(zeros, 0, nxPad);

            WritePaf(stream, true);
            stream.Write(zeros, 0, nxpPad);
        }

        private void WritePaf(Stream stream, bool isNxp)
        {
            Utils.PutUInt(stream, 0x70616601);
            Utils.PutUInt(stream, 0x0201001D);
            Utils.PutInt32(stream, chars.Length);
            Utils.PutInt32(stream, root);
            stream.Write(Encoding.ASCII.GetBytes("im2nx"));
            if (isNxp)
                stream.WriteByte(0x70);
            else
                stream.WriteByte(0);
            stream.Write(zeros, 0, 0xA);

            Utils.PutInt16(stream, 0x30);
            Utils.PutInt16(stream, 0x100);
            stream.Write(zeros, 0, 0xC);

            foreach (CharData c in chars)
            {
                Utils.PutUShort(stream, c.key);
                Utils.PutByte(stream, c.datawidth);
                Utils.PutByte(stream, c.dataheight);
                Utils.PutInt16(stream, c.datax);
                Utils.PutInt16(stream, c.datay);
                Utils.PutInt16(stream, c.offsetx);
                Utils.PutInt16(stream, c.offsety);
                Utils.PutInt16(stream, isNxp ? c.width : (short)0x20);
                Utils.PutInt16(stream, c.blank);
                Utils.PutInt32(stream, c.left);
                Utils.PutInt32(stream, c.right);
                Utils.PutUShort(stream, c.isEmoji);
                stream.Write(zeros, 0, 6);
            }
        }

        #region Bitmaps
        public Bitmap BigBitmap { get; private set; }
        public Bitmap[] charBitmaps { get; private set; }

        private Bitmap GetCharBitmap(CharData c)
        {
            Bitmap charBitmap = new Bitmap(c.datawidth, c.dataheight);
            for (int y = 0; y < c.dataheight; y++)
            {
                for (int x = 0; x < c.datawidth; x++)
                {
                    int xx = c.datax + x;
                    int yy = c.datay + y;
                    Color color;
                    if (xx < 0 || yy < 0 || xx >= BigBitmap.Width || yy >= BigBitmap.Height)
                        color = Color.Transparent;
                    else
                        color = BigBitmap.GetPixel(xx, yy);
                    charBitmap.SetPixel(x, y, color);
                }
            }
            return charBitmap;
        }

        private void UseBitmapArray()
        {
            if (charBitmaps == null && BigBitmap != null)
            {
                charBitmaps = new Bitmap[chars.Length];
                foreach (CharData c in chars)
                {
                    charBitmaps[c.index] = GetCharBitmap(c);
                }
            }
            BigBitmap?.Dispose();
            BigBitmap = null;
        }

        private void UseBigBitmap()
        {
            if (BigBitmap == null && charBitmaps != null)
            {
                BigBitmap = new Bitmap(2048, 2048);
                Array.Sort(chars, (c1, c2) => {
                    int result = c2.dataheight.CompareTo(c1.dataheight);
                    if (result != 0)
                        return result;
                    else
                        return c1.index.CompareTo(c2.index);
                    }
                );
                short x = 2049;
                short y = -1;
                byte lineHeight = 0;
                foreach (CharData c in chars)
                {
                    if (x + c.datawidth > 2048 + 1)
                    {
                        y += lineHeight;
                        lineHeight = c.dataheight;
                        lineHeight--;
                        x = -1;
                    }
                    CopyCharBitmap(BigBitmap, c, x, y);
                    c.datax = x;
                    c.datay = y;
                    x += c.datawidth;
                    x--;
                }
                Array.Sort(chars);
                foreach (Bitmap charBitmap in charBitmaps)
                {
                    charBitmap?.Dispose();
                }
                charBitmaps = null;
            }
        }

        private void CopyCharBitmap(Bitmap bigBitmap, CharData c, int x, int y)
        {
            for (int yy = 1; yy < c.dataheight - 1; yy++)
            {
                for (int xx = 1; xx < c.datawidth - 1; xx++)
                {
                    bigBitmap.SetPixel(x + xx, y + yy, charBitmaps[c.index].GetPixel(xx, yy));
                }
            }
        }

        public void SaveAllCharBitmaps(string destDir)
        {
            DirectoryInfo dInfo = new DirectoryInfo(destDir);
            dInfo.Create();
            UseBitmapArray();
            for (int i = 0; i < charBitmaps.Length; i++)
            {
                charBitmaps[i].Save(dInfo.FullName + "\\" + chars[i].key.ToString("X4") + ".png", ImageFormat.Png);
            }
        }

        public void RecreateBigBitmap()
        {
            UseBitmapArray();
            UseBigBitmap();
        }
        #endregion
        #region Tree
        private void BuildTree()
        {
            Array.Sort(chars);
            for (int i = 0; i < chars.Length; i++)
            {
                chars[i].index = i;
            }
            root = BuildSubTree(chars);
        }

        private int BuildSubTree(Span<CharData> span)
        {
            if (span.Length == 0)
                return -1;

            int midpoint = span.Length / 2;
            span[midpoint].left = BuildSubTree(span.Slice(0, midpoint));
            span[midpoint].right = BuildSubTree(span.Slice(midpoint + 1));
            return span[midpoint].index;
        }

        internal bool CheckTree()
        {
            if (chars[0].index != 0)
                return false;
            for (int i = 0; i < chars.Length - 1; i++)
            {
                if (chars[i + 1].index != i + 1 || chars[i].CompareTo(chars[i + 1]) >= 0)
                    return false;
            }
            return CheckSubTree(chars, root);
        }
        private bool CheckSubTree(Span<CharData> span, int root)
        {
            if (span.Length == 0)
                return (root == -1);

            int midpoint = span.Length / 2;
            if (span[midpoint].index != root)
                return false;

            return CheckSubTree(span.Slice(0, midpoint), span[midpoint].left) &&
                CheckSubTree(span.Slice(midpoint + 1), span[midpoint].right);
        }
        #endregion
        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                BigBitmap?.Dispose();
                BigBitmap = null;
                if (charBitmaps != null)
                {
                    foreach (Bitmap charBitmap in charBitmaps)
                    {
                        charBitmap?.Dispose();
                    }
                }
                charBitmaps = null;
            }
        }

        ~Font()
        {
            Dispose(false);
        }
        #endregion
    }

    class CharData : IComparable<CharData>
    {
        public ushort key;
        public byte datawidth;
        public byte dataheight;
        public short datax;
        public short datay;
        public short offsetx;
        public short offsety;
        public short width;
        public short blank;

        public int index;
        public int left;
        public int right;

        public ushort isEmoji;

        public int CompareTo(CharData other) => key.CompareTo(other.key);
    }
}
