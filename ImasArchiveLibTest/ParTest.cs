using Imas.Archive;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading.Tasks;

namespace ImasArchiveLibTest
{
    [TestClass]
    public class ParTest
    {
        [DataTestMethod]
        [DataRow("hdd/ui/commonCursor/commonCursorComponent.par", "par")]
        [DataRow("hdd/bg3d/fes_001.par", "par")]
        [DataRow("hdd/bg3d/gimmick.par", "par")]
        public async Task ParExtractTest(string inFile, string expectedDirParent)
        {
            using FileStream fileStream = new FileStream(inFile, FileMode.Open, FileAccess.Read);
            ParFile parFile = new ParFile(fileStream);
            await parFile.ExtractAll("temp_par");
            bool eq = Compare.CompareDirectories(expectedDirParent + inFile.Substring(inFile.LastIndexOf('/'))[0..^4] + "_par", "temp_par");
            Directory.Delete("temp_par", true);
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("hdd/bg3d/fes_001.par")]
        public async Task ParListFilenamesTest(string inFile)
        {
            using FileStream parStream = new FileStream(inFile, FileMode.Open, FileAccess.Read);
            ParFile parFile = new ParFile(parStream);
            using TextWriter textWriter = new StreamWriter("log_parnames.txt");
            await parFile.ForAll((entry, name) => textWriter.WriteLine(name));
        }

        [DataTestMethod]
        [DataRow("hdd/ui/commonCursor/commonCursorComponent.par")]
        public async Task WriteParTest(string inFile)
        {
            using (FileStream fileStream = new FileStream(inFile, FileMode.Open, FileAccess.Read))
            {
                ParFile parFile = new ParFile(fileStream);
                await parFile.ExtractAll("temp1_par");
                using FileStream outStream = new FileStream("temp.par", FileMode.Create, FileAccess.Write);
                await parFile.SaveTo(outStream).ConfigureAwait(false);
            }
            using (FileStream fileStream = new FileStream("temp.par", FileMode.Open, FileAccess.Read))
            {
                ParFile parFile = new ParFile(fileStream);
                await parFile.ExtractAll("temp2_par");
            }
            bool eq = Compare.CompareDirectories("temp1_par", "temp2_par");
            File.Delete("temp.par");
            Directory.Delete("temp1_par", true);
            Directory.Delete("temp2_par", true);
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("hdd/bg3d/fes_001.par", "fes_001.bin__", "other/week4-3.bin", "par/fes_001_edited_par")]
        public async Task EditParTest(string inFile, string nameToReplace, string replacementFile, string expectedDir)
        {
            using (FileStream fileStream = new FileStream(inFile, FileMode.Open, FileAccess.Read))
            {
                ParFile parFile = new ParFile(fileStream);
                using (FileStream replaceStream = new FileStream(replacementFile, FileMode.Open, FileAccess.Read))
                {
                    await parFile.GetEntry(nameToReplace).SetData(replaceStream).ConfigureAwait(false);
                }
                using FileStream outStream = new FileStream("temp.par", FileMode.Create, FileAccess.Write);
                await parFile.SaveTo(outStream).ConfigureAwait(false);
            }
            using (FileStream fileStream = new FileStream("temp.par", FileMode.Open, FileAccess.Read))
            {
                ParFile parFile = new ParFile(fileStream);
                await parFile.ExtractAll("temp_par").ConfigureAwait(false);
            }
            bool eq = Compare.CompareDirectories("temp_par", expectedDir);
            File.Delete("temp.par");
            Directory.Delete("temp_par", true);
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("hdd/bg3d/fes_001.par", "par/fes_replace1", "par/fes_001_edited_par")]
        [DataRow("hdd/bg3d/fes_001.par", "par/fes_replace2", "par/fes_001_replace2_par")]
        public async Task ReplaceEntriesAndSaveToParTest(string inFile, string replacementDir, string expectedDir)
        {
            using (FileStream fileStream = new FileStream(inFile, FileMode.Open, FileAccess.Read))
            {
                ParFile parFile = new ParFile(fileStream);
                using FileStream outStream = new FileStream("temp.par", FileMode.Create, FileAccess.Write);
                await parFile.ReplaceEntriesAndSaveTo(outStream, new FileSource(replacementDir)).ConfigureAwait(false);
            }
            using (FileStream fileStream = new FileStream("temp.par", FileMode.Open, FileAccess.Read))
            {
                ParFile parFile = new ParFile(fileStream);
                await parFile.ExtractAll("temp_par").ConfigureAwait(false);
            }
            bool eq = Compare.CompareDirectories("temp_par", expectedDir);
            File.Delete("temp.par");
            Directory.Delete("temp_par", true);
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("hdd/bg3d/fes_001.par", "par/fes_replace1", "par/fes_001_edited_par")]
        [DataRow("hdd/bg3d/fes_001.par", "par/fes_replace2", "par/fes_001_replace2_par")]
        public async Task ReplaceEntriesParTest(string inFile, string replacementDir, string expectedDir)
        {
            using (FileStream fileStream = new FileStream(inFile, FileMode.Open, FileAccess.Read))
            {
                ParFile parFile = new ParFile(fileStream);
                using FileStream outStream = new FileStream("temp.par", FileMode.Create, FileAccess.Write);
                await parFile.ReplaceEntries(new FileSource(replacementDir)).ConfigureAwait(false);
                await parFile.SaveTo(outStream).ConfigureAwait(false);
            }
            using (FileStream fileStream = new FileStream("temp.par", FileMode.Open, FileAccess.Read))
            {
                ParFile parFile = new ParFile(fileStream);
                await parFile.ExtractAll("temp_par").ConfigureAwait(false);
            }
            bool eq = Compare.CompareDirectories("temp_par", expectedDir);
            File.Delete("temp.par");
            Directory.Delete("temp_par", true);
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("hdd/bg3d/fes_001.par", "par/fes_replace1.zip", "par/fes_001_edited_par")]
        [DataRow("hdd/bg3d/fes_001.par", "par/fes_replace2.zip", "par/fes_001_replace2_par")]
        public async Task ReplaceEntriesZipAndSaveToParTest(string inFile, string replacementZip, string expectedDir)
        {
            using (FileStream fileStream = new FileStream(inFile, FileMode.Open, FileAccess.Read))
            {
                ParFile parFile = new ParFile(fileStream);
                using FileStream outStream = new FileStream("temp.par", FileMode.Create, FileAccess.Write);
                using ZipSourceParent parent = new ZipSourceParent(replacementZip);
                await parFile.ReplaceEntriesAndSaveTo(outStream, parent.GetZipSource()).ConfigureAwait(false);
            }
            using (FileStream fileStream = new FileStream("temp.par", FileMode.Open, FileAccess.Read))
            {
                ParFile parFile = new ParFile(fileStream);
                await parFile.ExtractAll("temp_par").ConfigureAwait(false);
            }
            bool eq = Compare.CompareDirectories("temp_par", expectedDir);
            File.Delete("temp.par");
            Directory.Delete("temp_par", true);
            Assert.IsTrue(eq);
        }
    }
}