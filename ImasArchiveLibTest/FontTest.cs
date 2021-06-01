using Imas.Gtf;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading.Tasks;

namespace ImasArchiveLibTest
{
    [TestClass]
    public class FontTest
    {
        [DataTestMethod]
        [DataRow("font/im2nx_font.par", "font/font.png")]
        public async Task ReadFontImageTest(string inFile, string expectedFile)
        {
            using (FileStream inStream = new FileStream(inFile, FileMode.Open, FileAccess.Read))
            {
                using Font font = await Font.CreateFromPar(inStream);
                font.SaveBigBitmap("temp.png");
            }

            bool eq = Compare.CompareFiles(expectedFile, "temp.png");
            File.Delete("temp.png");
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("font/im2nx_font.par", "font/font_chars")]
        public async Task SaveFontCharsTest(string inFile, string expectedDir)
        {
            using (FileStream inStream = new FileStream(inFile, FileMode.Open, FileAccess.Read))
            {
                using Font font = await Font.CreateFromPar(inStream);
                font.SaveAllCharBitmaps("tempdir");
            }

            bool eq = Compare.CompareDirectories(expectedDir, "tempdir");
            Directory.Delete("tempdir", true);
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("font/font_chars", "font/font_fromFolder.png")]
        public void ReadFontFolderTest(string inDir, string expectedFile)
        {
            using Font font = Font.CreateFromCharsDir(inDir);
            font.CompressBitmap();
            font.SaveBigBitmap("temp.png");

            bool eq = Compare.CompareFiles(expectedFile, "temp.png");
            File.Delete("temp.png");
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("font/im2nx_font.par")]
        public async Task CheckTreeTest(string inFile)
        {
            using FileStream inStream = new FileStream(inFile, FileMode.Open, FileAccess.Read);
            using Font font = await Font.CreateFromPar(inStream);
            Assert.IsTrue(font.CheckTree());
        }

        [DataTestMethod]
        [DataRow("font/im2nx_font.par", "font/font_remade.png", "font/font_remade")]
        public async Task BuildBitmapTest(string inFile, string expectedFile, string expectedDir)
        {
            using (FileStream inStream = new FileStream(inFile, FileMode.Open, FileAccess.Read))
            {
                using Font font = await Font.CreateFromPar(inStream);
                font.CompressBitmap();
                font.SaveBigBitmap("temp.png");
                font.SaveAllCharBitmaps("tempdir");
            }

            bool eq = Compare.CompareFiles(expectedFile, "temp.png");
            eq &= Compare.CompareDirectories(expectedDir, "tempdir");
            File.Delete("temp.png");
            Directory.Delete("tempdir", true);
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("font/im2nx_font.par")]
        public async Task WriteParTest(string inFile)
        {
            using (FileStream inStream = new FileStream(inFile, FileMode.Open, FileAccess.Read))
            {
                using Font font = await Font.CreateFromPar(inStream);

                using FileStream outStream = new FileStream("temp.par", FileMode.Create, FileAccess.Write);
                await font.WriteFontPar(outStream);
            }

            bool eq = Compare.CompareFiles(inFile, "temp.par");
            File.Delete("temp.par");
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("patch/font")]
        public void WriteJSONTest(string inDir)
        {
            using Font font = Font.CreateFromCharsDir(inDir);
            font.AddDigraphs();
            Assert.IsTrue(font.CheckTree());
            font.SaveBigBitmap("../textbox-display/fontwhite.png");

            using StreamWriter writer = new StreamWriter("../textbox-display/fontdata.js");
            font.WriteJSON(writer);
        }

        [DataTestMethod]
        [DataRow("patch/font")]
        public void BlackFontTest(string inDir)
        {
            using Font font = Font.CreateFromCharsDir(inDir);
            font.UseBlackBitmaps();
            font.AddDigraphs();
            Assert.IsTrue(font.CheckTree());
            font.SaveBigBitmap("../textbox-display/fontblack.png");
        }

        [DataTestMethod]
        [DataRow("font/font_chars", "abcdefghijklmnopqrstuvwxyz", "font/digraphs")]
        public void SaveFontDigraphsTest(string inDir, string charset, string expectedDir)
        {
            using Font font = Font.CreateFromCharsDir(inDir);
            char[] set = charset.ToCharArray();
            font.CreateDigraphs(expectedDir, set, set);

            //bool eq = Compare.CompareDirectories(expectedDir, "tempdir");
            //Directory.Delete("tempdir", true);
            //Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("font/font_chars", "abcdefghijklmnopqrstuvwxyz", "font/digraphs.png", "font/digraphs.par")]
        public async Task AddSpecifiedDigraphsToFontTest(string inDir, string charset, string expectedPng, string expectedPar)
        {
            using Font font = Font.CreateFromCharsDir(inDir);
            char[] set = charset.ToCharArray();
            font.AddDigraphsToFont(set, set);
            font.CompressBitmap();
            font.SaveBigBitmap(expectedPng);
            using FileStream outStream = new FileStream(expectedPar, FileMode.Create, FileAccess.Write);
            await font.WriteFontPar(outStream);

            //bool eq = Compare.CompareFiles(expectedPar, "temp.par");
            //eq &= Compare.CompareFiles(expectedPng, "temp.png");
            //File.Delete("temp.par");
            //File.Delete("temp.png");
            //Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("font/im2nx_font.par", "font/newfont.png", "font/newfont.par")]
        public async Task AddAllDigraphsToFontTest(string inFile, string expectedPng, string expectedPar)
        {
            using (FileStream inStream = new FileStream(inFile, FileMode.Open, FileAccess.Read))
            {
                using Font font = await Font.CreateFromPar(inStream);
                font.AddDigraphs();
                Assert.IsTrue(font.CheckTree());
                font.SaveBigBitmap(expectedPng);
                using FileStream outStream = new FileStream(expectedPar, FileMode.Create, FileAccess.Write);
                await font.WriteFontPar(outStream, false);
            }

            //bool eq = Compare.CompareFiles(expectedPar, "temp.par") && Compare.CompareFiles(expectedPng, "temp.png");
            //File.Delete("temp.par");
            //File.Delete("temp.png");
            //Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("patch/font", "patch/font_fromFolder.par")]
        public async Task BuildFontDigraphsFromFolder(string inDir, string outFile)
        {
            using Font font = Font.CreateFromCharsDir(inDir);
            font.AddDigraphs();
            using FileStream outStream = new FileStream(outFile, FileMode.Create, FileAccess.Write);
            await font.WriteFontPar(outStream, false);
        }

        [DataTestMethod]
        [DataRow("font/im2nx_font.par", "font/FOT-RodinNTLGPro-DB.otf", "font/glyphs/glyph")]
        public async Task GenerateGlyphsTest(string parFile, string inFont, string outPath)
        {
            using Font font = await Font.CreateFromPar(File.OpenRead(parFile));
            FileInfo fileInfo = new FileInfo(inFont);
            font.GenerateGlyphs(fileInfo.FullName, "ABCDEFGHIJKLMNOP涙".ToCharArray(), outPath);
        }
    }
}