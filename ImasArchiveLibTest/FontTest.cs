using Imas;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace ImasArchiveLibTest
{
    [TestClass]
    public class FontTest
    {

        [DataTestMethod]
        [DataRow("disc/im2nx_font.par", "other/font.png")]
        public async Task ReadFontImageTest(string inFile, string expectedFile)
        {
            using (FileStream inStream = new FileStream(inFile, FileMode.Open, FileAccess.Read))
            {
                using Font font = new Font();
                await font.ReadFontPar(inStream);
                font.BigBitmap.Save("temp.png", ImageFormat.Png);
            }

            bool eq = Compare.CompareFiles(expectedFile, "temp.png");
            File.Delete("temp.png");
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("disc/im2nx_font.par", "font")]
        public async Task SaveFontCharsTest(string inFile, string expectedDir)
        {
            using (FileStream inStream = new FileStream(inFile, FileMode.Open, FileAccess.Read))
            {
                using Font font = new Font();
                await font.ReadFontPar(inStream);
                font.SaveAllCharBitmaps("tempdir");
            }

            bool eq = Compare.CompareDirectories(expectedDir, "tempdir");
            Directory.Delete("tempdir", true);
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("font", "other/font_fromFolder.png")]
        public void ReadFontFolderTest(string inDir, string expectedFile)
        {
            using Font font = new Font();
            font.LoadCharBitmaps(inDir);
            font.RecreateBigBitmap();
            font.BigBitmap.Save("temp.png", ImageFormat.Png);

            bool eq = Compare.CompareFiles(expectedFile, "temp.png");
            File.Delete("temp.png");
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("disc/im2nx_font.par")]
        public async Task CheckTreeTest(string inFile)
        {
            using FileStream inStream = new FileStream(inFile, FileMode.Open, FileAccess.Read);
            using Font font = new Font();
            await font.ReadFontPar(inStream);
            Assert.IsTrue(font.CheckTree());
        }

        [DataTestMethod]
        [DataRow("disc/im2nx_font.par", "other/font_remade.png", "font_remade")]
        public async Task BuildBitmapTest(string inFile, string expectedFile, string expectedDir)
        {
            using (FileStream inStream = new FileStream(inFile, FileMode.Open, FileAccess.Read))
            {
                using Font font = new Font();
                await font.ReadFontPar(inStream);
                font.RecreateBigBitmap();
                font.BigBitmap.Save("temp.png", ImageFormat.Png);
                font.SaveAllCharBitmaps("tempdir");
            }

            bool eq = Compare.CompareFiles(expectedFile, "temp.png") && Compare.CompareDirectories(expectedDir, "tempdir");
            File.Delete("temp.png");
            Directory.Delete("tempdir", true);
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("disc/im2nx_font.par")]
        public async Task WriteParTest(string inFile)
        {
            using (FileStream inStream = new FileStream(inFile, FileMode.Open, FileAccess.Read))
            {
                using Font font = new Font();
                await font.ReadFontPar(inStream);

                using FileStream outStream = new FileStream("temp.par", FileMode.Create, FileAccess.Write);
                await font.WriteFontPar(outStream);
            }

            bool eq = Compare.CompareFiles(inFile, "temp.par");
            File.Delete("temp.par");
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("font", "abcdefghijklmnopqrstuvwxyz", "digraphs")]
        public void SaveFontDigraphsTest(string inDir, string charset, string expectedDir)
        {
            using Font font = new Font();
            font.LoadCharBitmaps(inDir);
            char[] set = charset.ToCharArray();
            font.CreateDigraphs("tempdir", set, set);

            bool eq = Compare.CompareDirectories(expectedDir, "tempdir");
            Directory.Delete("tempdir", true);
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("font", "abcdefghijklmnopqrstuvwxyz", "other/digraphs.png", "other/digraphs.par")]
        public async Task AddSpecifiedDigraphsToFontTest(string inDir, string charset, string expectedPng, string expectedPar)
        {
            using (Font font = new Font())
            {
                font.LoadCharBitmaps(inDir);
                char[] set = charset.ToCharArray();
                font.AddDigraphsToFont(set, set);
                font.RecreateBigBitmap();
                font.BigBitmap.Save("temp.png");
                using FileStream outStream = new FileStream("temp.par", FileMode.Create, FileAccess.Write);
                await font.WriteFontPar(outStream);
            }

            bool eq = Compare.CompareFiles(expectedPar, "temp.par") && Compare.CompareFiles(expectedPng, "temp.png");
            File.Delete("temp.par");
            File.Delete("temp.png");
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("disc/im2nx_font.par", "other/newfont.png", "other/newfont.par")]
        public async Task AddAllDigraphsToFontTest(string inFile, string expectedPng, string expectedPar)
        {
            using (FileStream inStream = new FileStream(inFile, FileMode.Open, FileAccess.Read))
            {
                using Font font = new Font();
                await font.ReadFontPar(inStream);
                font.AddDigraphs();
                Assert.IsTrue(font.CheckTree());
                font.BigBitmap.Save("temp.png");
                using FileStream outStream = new FileStream("temp.par", FileMode.Create, FileAccess.Write);
                await font.WriteFontPar(outStream, false);
            }

            bool eq = Compare.CompareFiles(expectedPar, "temp.par") && Compare.CompareFiles(expectedPng, "temp.png");
            File.Delete("temp.par");
            File.Delete("temp.png");
            Assert.IsTrue(eq);
        }


        [DataTestMethod]
        [DataRow("patch/font", "patch/font_fromFolder.par")]
        public async Task BuildFontDigraphsFromFolder(string inDir, string outFile)
        {
            using Font font = new Font();
            font.LoadCharBitmaps(inDir);
            font.AddDigraphs();
            using FileStream outStream = new FileStream(outFile, FileMode.Create, FileAccess.Write);
            await font.WriteFontPar(outStream, false);

        }
    }
}
