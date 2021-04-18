using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Imas.Gtf
{
    public partial class Font : IDisposable
    {
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