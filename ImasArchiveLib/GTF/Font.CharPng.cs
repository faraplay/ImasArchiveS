using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace Imas.Gtf
{
    public partial class Font : IDisposable
    {
        public void SaveAllCharBitmaps(string destDir)
        {
            DirectoryInfo dInfo = new DirectoryInfo(destDir);
            dInfo.Create();
            UpdateBigBitmapPixelData();
            var charBitmaps = GetCharBitmaps(BigBitmapPixelData, chars);
            SaveCharBitmapExtraData(dInfo.FullName + "/fontdata.dat");
            for (int i = 0; i < chars.Length; i++)
            {
                using FileStream fileStream = new FileStream($"{dInfo.FullName}\\{chars[i].key:X4}.png", FileMode.Create);
                SavePngFromPixelData(fileStream, charBitmaps[chars[i].key], chars[i].datawidth, chars[i].dataheight);
            }
        }

        private void SaveCharBitmapExtraData(string outFile)
        {
            using FileStream stream = new FileStream(outFile, FileMode.Create, FileAccess.Write);
            Binary binary = new Binary(stream, true);
            binary.WriteInt32(chars.Length);
            foreach (CharData c in chars)
            {
                binary.WriteUInt16(c.key);
                binary.WriteInt16(c.offsetx);
                binary.WriteInt16(c.offsety);
                binary.WriteInt16(c.width);
                binary.WriteUInt16(c.isEmoji);
            }
        }

        public void LoadCharBitmaps(string srcDir)
        {
            DirectoryInfo dInfo = new DirectoryInfo(srcDir);
            if (!dInfo.Exists)
                throw new FileNotFoundException();
            LoadCharBitmapExtraData(dInfo.FullName + "/fontdata.dat");
            Dictionary<ushort, int[]> charBitmaps = new Dictionary<ushort, int[]>();
            for (int i = 0; i < chars.Length; i++)
            {
                using Bitmap bitmap = new Bitmap($"{dInfo.FullName}\\{chars[i].key:X4}.png");
                charBitmaps.Add(chars[i].key,
                    GetPixelData(
                        bitmap,
                        out int width,
                        out int height));
                if (width > 255 || height > 255)
                    throw new Exception("Bitmap is too big for a character (should be at most 255x255 px).");
                chars[i].datawidth = (byte)width;
                chars[i].dataheight = (byte)height;
            }
            BuildBigBitmap(charBitmaps, chars);
            BuildTree();
        }
        private Dictionary<ushort, int[]> CharBitmapsFromFile(string srcDir)
        {
            DirectoryInfo dInfo = new DirectoryInfo(srcDir);
            if (!dInfo.Exists)
                throw new FileNotFoundException();
            LoadCharBitmapExtraData(dInfo.FullName + "/fontdata.dat");
            Dictionary<ushort, int[]> charBitmaps = new Dictionary<ushort, int[]>();
            for (int i = 0; i < chars.Length; i++)
            {
                using Bitmap bitmap = new Bitmap($"{dInfo.FullName}\\{chars[i].key:X4}.png");
                charBitmaps.Add(chars[i].key,
                    GetPixelData(
                        bitmap,
                        out int width,
                        out int height));
                if (width > 255 || height > 255)
                    throw new Exception("Bitmap is too big for a character (should be at most 255x255 px).");
                chars[i].datawidth = (byte)width;
                chars[i].dataheight = (byte)height;
            }
            BuildTree();
            return charBitmaps;
        }

        private void LoadCharBitmapExtraData(string inFile)
        {
            using FileStream stream = new FileStream(inFile, FileMode.Open, FileAccess.Read);
            Binary binary = new Binary(stream, true);
            int charCount = binary.ReadInt32();
            chars = new CharData[charCount];
            for (int i = 0; i < charCount; i++)
            {
                chars[i] = new CharData
                {
                    key = binary.ReadUInt16(),
                    offsetx = binary.ReadInt16(),
                    offsety = binary.ReadInt16(),
                    width = binary.ReadInt16(),
                    isEmoji = binary.ReadUInt16()
                };
            }
        }
    }
}