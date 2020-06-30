using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace ImasArchiveLib
{
    public class Font : IDisposable
    {
        CharData[] chars;
        int root;

        #region par
        private readonly byte[] zeros = new byte[256];
        byte[] parHeader;
        public void ReadFontPar(Stream stream)
        {
            parHeader = new byte[16];
            stream.Read(parHeader);
            int gtfPos = Utils.GetInt32(stream);
            _ = Utils.GetInt32(stream);
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
        public void WriteFontPar(Stream stream, bool nxFixedWidth = true)
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

            WritePaf(stream, false, nxFixedWidth);
            stream.Write(zeros, 0, nxPad);

            WritePaf(stream, true, nxFixedWidth);
            stream.Write(zeros, 0, nxpPad);
        }

        private void WritePaf(Stream stream, bool isNxp, bool nxFixedWidth = true)
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
                Utils.PutInt16(stream, (isNxp || !nxFixedWidth) ? c.width : (short)0x20);
                Utils.PutInt16(stream, c.blank);
                Utils.PutInt32(stream, c.left);
                Utils.PutInt32(stream, c.right);
                Utils.PutUShort(stream, c.isEmoji);
                stream.Write(zeros, 0, 6);
            }
        }
        #endregion
        #region Bitmaps
        public Bitmap BigBitmap { get; private set; }
        private bool charsHaveBitmaps;

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

        private void UseCharBitmaps()
        {
            if (!charsHaveBitmaps && BigBitmap != null)
            {
                foreach (CharData c in chars)
                {
                    c.bitmap = GetCharBitmap(c);
                }
            }
            BigBitmap?.Dispose();
            BigBitmap = null;
            charsHaveBitmaps = true;
        }

        // have 2 pixels of space between chars
        // 1 pixel led to artifacts around the chars :(
        private void UseBigBitmap()
        {
            if (BigBitmap == null && charsHaveBitmaps)
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
                        //lineHeight--;
                        x = -1;
                    }
                    CopyCharBitmap(BigBitmap, c, x, y);
                    c.datax = x;
                    c.datay = y;
                    x += c.datawidth;
                    //x--;
                }
                Array.Sort(chars);
                foreach (CharData c in chars)
                {
                    c.bitmap?.Dispose();
                    c.bitmap = null;
                }
                charsHaveBitmaps = false;
            }
        }

        private void CopyCharBitmap(Bitmap bigBitmap, CharData c, int x, int y)
        {
            for (int yy = 1; yy < c.dataheight - 1; yy++)
            {
                for (int xx = 1; xx < c.datawidth - 1; xx++)
                {
                    bigBitmap.SetPixel(x + xx, y + yy, c.bitmap.GetPixel(xx, yy));
                }
            }
        }

        public void RecreateBigBitmap()
        {
            UseCharBitmaps();
            UseBigBitmap();
        }
        #endregion
        #region Digraph
        // Do not rebuild big bitmap after calling this function!
        public void AddDigraphs()
        {
            char[] lowerCharset = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
            char[] allCharset = "!\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~".ToCharArray();
            AddDigraphsToFont(lowerCharset, lowerCharset);
            AddSpaceDigraphsToFont(allCharset);
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
                    key = (ushort)((c.key & 0xFF | 0x80) << 8 + 0xA0),
                    bitmap = null,
                    datawidth = c.datawidth,
                    dataheight = c.dataheight,
                    datax = c.datax,
                    datay = c.datay,
                    offsetx = c.offsetx,
                    offsety = c.offsety,
                    width = (short)(c.width + 0xA)
                });
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

            CopyCharBitmap(bitmap, c1, c1.offsetx - offsetxmin, c1.offsety - offsetymin);
            CopyCharBitmap(bitmap, c2, c2.offsetx + c1.width - offsetxmin, c2.offsety - offsetymin);

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
        #endregion
        #region Save/Load PNG
        public void SaveAllCharBitmaps(string destDir)
        {
            DirectoryInfo dInfo = new DirectoryInfo(destDir);
            dInfo.Create();
            UseCharBitmaps();
            SaveCharBitmapExtraData(dInfo.FullName + "/fontdata.dat");
            for (int i = 0; i < chars.Length; i++)
            {
                chars[i].bitmap.Save(dInfo.FullName + "\\" + chars[i].key.ToString("X4") + ".png", ImageFormat.Png);
            }
        }

        private void SaveCharBitmapExtraData(string outFile)
        {
            using FileStream stream = new FileStream(outFile, FileMode.Create, FileAccess.Write);
            Utils.PutInt32(stream, chars.Length);
            foreach (CharData c in chars)
            {
                Utils.PutUShort(stream, c.key);
                Utils.PutInt16(stream, c.offsetx);
                Utils.PutInt16(stream, c.offsety);
                Utils.PutInt16(stream, c.width);
                Utils.PutUShort(stream, c.isEmoji);
            }
        }

        public void LoadCharBitmaps(string srcDir)
        {
            DirectoryInfo dInfo = new DirectoryInfo(srcDir);
            if (!dInfo.Exists)
                throw new FileNotFoundException();
            LoadCharBitmapExtraData(dInfo.FullName + "/fontdata.dat");
            for (int i = 0; i < chars.Length; i++)
            {
                chars[i].bitmap = new Bitmap(dInfo.FullName + "\\" + chars[i].key.ToString("X4") + ".png");
                chars[i].datawidth = (byte)chars[i].bitmap.Width;
                chars[i].dataheight = (byte)chars[i].bitmap.Height;
            }
            charsHaveBitmaps = true;
            BuildTree();
        }

        private void LoadCharBitmapExtraData(string inFile)
        {
            using FileStream stream = new FileStream(inFile, FileMode.Open, FileAccess.Read);
            int charCount = Utils.GetInt32(stream);
            chars = new CharData[charCount];
            for (int i = 0; i < charCount; i++)
            {
                chars[i] = new CharData
                {
                    key = Utils.GetUShort(stream),
                    offsetx = Utils.GetInt16(stream),
                    offsety = Utils.GetInt16(stream),
                    width = Utils.GetInt16(stream),
                    isEmoji = Utils.GetUShort(stream)
                };
            }
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
                foreach (CharData c in chars)
                {
                    c?.Dispose();
                }
            }
        }

        ~Font()
        {
            Dispose(false);
        }
        #endregion
    }

    class CharData : IComparable<CharData>, IDisposable
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

        public Bitmap bitmap;
        public int CompareTo(CharData other) => key.CompareTo(other.key);


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
                bitmap?.Dispose();
                bitmap = null;
            }
        }

        ~CharData()
        {
            Dispose(false);
        }
        #endregion
    }
}
