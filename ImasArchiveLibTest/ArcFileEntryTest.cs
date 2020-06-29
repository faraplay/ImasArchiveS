using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImasArchiveLib;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace ImasArchiveLibTest
{
    [TestClass]
    public class ArcFileEntryTest
    {
        Progress<ProgressData> progress = new Progress<ProgressData>(
            pair => Console.WriteLine(" {0} of {1}: {2} ", pair.count, pair.total, pair.filename));

        [AssemblyInitialize]
        public static void SetupDirectory(TestContext testContext)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(System.Environment.CurrentDirectory + @"\..\..\..\..\..\test");
            System.Environment.CurrentDirectory = directoryInfo.FullName;
        }

        [DataTestMethod]
        [DataRow("hdd", "", 0x13DF)]
        [DataRow("disc", "", 0x1B8)]
        [DataRow("dlc\\_dlc03", ".dat", 0x1D7)]
        public void EntryCount(string filename, string extension, int expectedCount)
        {
            int actualCount;
            using (ArcFile arcFile = new ArcFile(filename, extension))
            {
                actualCount = arcFile.Entries.Count;
            }

            Assert.AreEqual(expectedCount, actualCount);
        }

        [DataTestMethod]
        [DataRow("hdd", "")]
        [DataRow("disc", "")]
        [DataRow("dlc\\_dlc03", ".dat")]
        public void Entry_StreamHeaderIsFBS(string filename, string extension)
        {
            byte[] expectedHeader = new byte[] { 0, 0x46, 0x42, 0x53, 0, 0, 0, 0 };
            using ArcFile arcFile = new ArcFile(filename, extension);
            foreach (ArcEntry arcEntry in arcFile.Entries)
            {
                byte[] actualHeader = new byte[8];
                using Stream stream = arcEntry.OpenRaw();
                stream.Read(actualHeader);
                bool eq = expectedHeader.SequenceEqual(actualHeader);
                Assert.IsTrue(eq);
            }
        }

        [DataTestMethod]
        [DataRow("hdd", "")]
        [DataRow("disc", "")]
        [DataRow("dlc\\_dlc03", ".dat")]
        public void Entry_StreamLengthAgrees(string filename, string extension)
        {
            using ArcFile arcFile = new ArcFile(filename, extension);
            foreach (ArcEntry arcEntry in arcFile.Entries)
            {
                MemoryStream memoryStream = new MemoryStream();
                using Stream stream = arcEntry.OpenRaw();
                stream.CopyTo(memoryStream);

                Assert.AreEqual(arcEntry.Length, memoryStream.Length);
            }
        }

        [DataTestMethod]
        [DataRow("hdd", "", "songinfo/songResource.bin.gz", "other/songResource.bin.gz.fbs")]
        public void GetEntryRawAndWriteToFile(string filename, string extension, string entryFilepath, string expectedFile)
        {
            using ArcFile arcFile = new ArcFile(filename, extension);
            ArcEntry arcEntry = arcFile.GetEntry(entryFilepath);
            if (arcEntry == null)
                Assert.Fail("Entry not found.");
            using (Stream stream = arcEntry.OpenRaw()) 
            { 
                using FileStream fileStream = new FileStream("temp.dat", FileMode.Create, FileAccess.Write);
                stream.CopyTo(fileStream);
            }
            bool eq = Compare.CompareFiles(expectedFile, "temp.dat");
            File.Delete("temp.dat");
            Assert.IsTrue(eq);
        }


        [DataTestMethod]
        [DataRow("hdd", "", "songinfo/songResource.bin.gz", "other/songResource.bin")]
        [DataRow("hdd", "", "commu2/par/ami_bs2_c01.par.gz", "other/ami_bs2_c01.par")]
        public void GetEntryAndWriteToFile(string filename, string extension, string entryFilepath, string expectedFile)
        {
            using ArcFile arcFile = new ArcFile(filename, extension);
            ArcEntry arcEntry = arcFile.GetEntry(entryFilepath);
            if (arcEntry == null)
                Assert.Fail("Entry not found.");
            using (Stream stream = arcEntry.Open())
            {
                using FileStream fileStream = new FileStream("temp.dat", FileMode.Create, FileAccess.Write);
                stream.CopyTo(fileStream);
            }
            bool eq = Compare.CompareFiles(expectedFile, "temp.dat");
            File.Delete("temp.dat");
            Assert.IsTrue(eq);
        }


        [DataTestMethod]
        [DataRow("disc", "", "disc")]
        [DataRow("hdd", "", "hdd")]
        [DataRow("dlc\\_dlc03", ".dat", "_dlc03")]
        public async Task ExtractAllUncompressAsync(string filename, string extension, string expectedFolder)
        {
            using ArcFile arcFile = new ArcFile(filename, extension);
            await Task.Run(() => arcFile.ExtractAllAsync("temp", progress));
            bool eq = Compare.CompareDirectories(expectedFolder, "temp");
            DirectoryInfo directoryInfo = new DirectoryInfo("temp");
            directoryInfo.Delete(true);
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("hdd", "")]
        [DataRow("disc", "")]
        [DataRow("dlc\\_dlc03", ".dat")]
        public async Task SaveArcAs(string filename, string extension)
        {
            using (ArcFile arcFile = new ArcFile(filename, extension))
            {
                await arcFile.SaveAs("temp", progress);
            }
            bool eq = Compare.CompareFileHashes(filename + ".arc" + extension, "temp.arc") && Compare.CompareFileHashes(filename + ".bin" + extension, "temp.bin");
            File.Delete("temp.arc");
            File.Delete("temp.bin");
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("disc", "", "system/chara_viewer_def.bin.gz", "other/songResource.bin", "disc")]
        [DataRow("hdd", "", "commu2/par/ami_bs2_c01.par.gz", "other/week4-3.bin", "hdd")]
        public async Task EditEntryReextractTest(string filename, string extension, string entryFilepath, string replacementFile, string expectedDir)
        {

            using (ArcFile arcFile = new ArcFile(filename, extension))
            {
                ArcEntry arcEntry = arcFile.GetEntry(entryFilepath);
                if (arcEntry == null)
                    Assert.Fail("Entry not found.");
                using (FileStream fileStream = new FileStream(replacementFile, FileMode.Open, FileAccess.Read))
                {
                    await arcEntry.Replace(fileStream);
                }
                await arcFile.SaveAs("temp", progress);
            }
            using (ArcFile arcFile = new ArcFile("temp", ""))
            {
                await arcFile.ExtractAllAsync("tempdir", progress);
            }
            File.Move(expectedDir + "/" + entryFilepath.Substring(0, entryFilepath.Length - 3), "backup");
            File.Copy(replacementFile, expectedDir + "/" + entryFilepath.Substring(0, entryFilepath.Length - 3));
            bool eq = Compare.CompareDirectories(expectedDir, "tempdir");
            File.Delete(expectedDir + "/" + entryFilepath.Substring(0, entryFilepath.Length - 3));
            File.Move("backup", expectedDir + "/" + entryFilepath.Substring(0, entryFilepath.Length - 3));

            DirectoryInfo directoryInfo = new DirectoryInfo("tempdir");
            directoryInfo.Delete(true);
            File.Delete("temp.arc");
            File.Delete("temp.bin");
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("little")]
        [DataRow("disc")]
        public async Task BuildNewArcTest(string dir)
        {
            await ArcFile.BuildFromDirectory(dir, "temp", progress);
            using (ArcFile arcFile = new ArcFile("temp"))
            {
                await arcFile.ExtractAllAsync("tempdir", progress);
            }

            bool eq = Compare.CompareDirectories(dir, "tempdir");

            DirectoryInfo directoryInfo = new DirectoryInfo("tempdir");
            directoryInfo.Delete(true);
            File.Delete("temp.arc");
            File.Delete("temp.bin");

            Assert.IsTrue(eq);
        }

    }
}

