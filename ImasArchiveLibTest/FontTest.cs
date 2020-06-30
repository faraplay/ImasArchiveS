using ImasArchiveLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing.Imaging;
using System.IO;

namespace ImasArchiveLibTest
{
    [TestClass]
    public class FontTest
    {

        [DataTestMethod]
        [DataRow("disc/im2nx_font.par", "other/font.png")]
        public void ReadFontImageTest(string inFile, string outFile)
        {
            using FileStream inStream = new FileStream(inFile, FileMode.Open, FileAccess.Read);
            using Font font = new Font();
            font.ReadFontPar(inStream);
            font.BigBitmap.Save(outFile, ImageFormat.Png);
        }

        [DataTestMethod]
        [DataRow("disc/im2nx_font.par", "font")]
        public void SaveFontCharsTest(string inFile, string outDir)
        {
            using FileStream inStream = new FileStream(inFile, FileMode.Open, FileAccess.Read);
            using Font font = new Font();
            font.ReadFontPar(inStream);
            font.SaveAllCharBitmaps(outDir);
        }

        [DataTestMethod]
        [DataRow("font", "other/font_fromFolder.png")]
        public void ReadFontFolderTest(string inDir, string outFile)
        {
            using Font font = new Font();
            font.LoadCharBitmaps(inDir);
            font.RecreateBigBitmap();
            font.BigBitmap.Save(outFile, ImageFormat.Png);
        }

        [DataTestMethod]
        [DataRow("disc/im2nx_font.par")]
        public void CheckTreeTest(string inFile)
        {
            using FileStream inStream = new FileStream(inFile, FileMode.Open, FileAccess.Read);
            using Font font = new Font();
            font.ReadFontPar(inStream);
            Assert.IsTrue(font.CheckTree());
        }

        [DataTestMethod]
        [DataRow("disc/im2nx_font.par", "other/font_remade.png", "font_remade")]
        public void BuildBitmapTest(string inFile, string outFile, string outDir)
        {
            using FileStream inStream = new FileStream(inFile, FileMode.Open, FileAccess.Read);
            using Font font = new Font();
            font.ReadFontPar(inStream);
            font.RecreateBigBitmap();
            font.BigBitmap.Save(outFile, ImageFormat.Png);
            font.SaveAllCharBitmaps(outDir);
        }

        [DataTestMethod]
        [DataRow("disc/im2nx_font.par")]
        public void WriteParTest(string inFile)
        {
            using (FileStream inStream = new FileStream(inFile, FileMode.Open, FileAccess.Read))
            {
                using Font font = new Font();
                font.ReadFontPar(inStream);

                using FileStream outStream = new FileStream("temp.par", FileMode.Create, FileAccess.Write);
                font.WriteFontPar(outStream);
            }
            bool eq = Compare.CompareFiles(inFile, "temp.par");
            File.Delete("temp.par");
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("font", "abcdefghijklmnopqrstuvwxyz", "digraphs")]
        public void SaveFontDigraphsTest(string inDir, string charset, string outDir)
        {
            using Font font = new Font();
            font.LoadCharBitmaps(inDir);
            char[] set = charset.ToCharArray();
            font.CreateDigraphs(outDir, set, set);
        }

        [DataTestMethod]
        [DataRow("font", "abcdefghijklmnopqrstuvwxyz", "other/digraphs.png", "other/digraphs.par")]
        public void AddSpecifiedDigraphsToFontTest(string inDir, string charset, string outPng, string outPar)
        {
            using Font font = new Font();
            font.LoadCharBitmaps(inDir);
            char[] set = charset.ToCharArray();
            font.AddDigraphsToFont(set, set);
            font.RecreateBigBitmap();
            font.BigBitmap.Save(outPng);
            using FileStream outStream = new FileStream(outPar, FileMode.Create, FileAccess.Write);
            font.WriteFontPar(outStream);
        }

        [DataTestMethod]
        [DataRow("disc/im2nx_font.par", "other/newfont.png", "other/newfont.par")]
        public void AddAllDigraphsToFontTest(string inFile, string outPng, string outPar)
        {
            using (FileStream inStream = new FileStream(inFile, FileMode.Open, FileAccess.Read))
            {
                using Font font = new Font();
                font.ReadFontPar(inStream);
                font.AddDigraphs();
                Assert.IsTrue(font.CheckTree());
                font.BigBitmap.Save(outPng);
                using FileStream outStream = new FileStream(outPar, FileMode.Create, FileAccess.Write);
                font.WriteFontPar(outStream, false);
            }
        }
    }
}
