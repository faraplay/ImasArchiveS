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
        readonly Progress<ProgressData> progress = new Progress<ProgressData>(
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
        [DataRow("hdd", "", "songinfo/songResource.bin", "other/songResource.bin.gz.fbs")]
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
        [DataRow("hdd", "", "songinfo/songResource.bin", "other/songResource.bin")]
        [DataRow("hdd", "", "commu2/par/ami_bs2_c01.par", "other/ami_bs2_c01.par")]
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
            bool eq = Compare.CompareFiles(filename + ".arc" + extension, "temp.arc") && Compare.CompareFiles(filename + ".bin" + extension, "temp.bin");
            File.Delete("temp.arc");
            File.Delete("temp.bin");
            Assert.IsTrue(eq);
        }


        [DataTestMethod]
        [DataRow("disc", "", "system/chara_viewer_def.bin", "other/songResource.bin", "other/disc_edited")]
        public async Task EditThenSaveArcAsTwice(string filename, string extension, string entryPath, string replacementFile, string expectedFile)
        {
            using (ArcFile arcFile = new ArcFile(filename, extension))
            {
                ArcEntry arcEntry = arcFile.GetEntry(entryPath);
                if (arcEntry == null)
                    Assert.Fail("Entry not found.");
                using (FileStream fileStream = new FileStream(replacementFile, FileMode.Open, FileAccess.Read))
                {
                    await arcEntry.Replace(fileStream);
                }
                await arcFile.SaveAs("temp1", progress);
                await arcFile.SaveAs("temp2", progress);
            }
            bool eq1 = Compare.CompareFiles(expectedFile + ".arc" + extension, "temp1.arc");
            bool eq2 = Compare.CompareFiles(expectedFile + ".bin" + extension, "temp1.bin");
            bool eq3 = Compare.CompareFiles(expectedFile + ".arc" + extension, "temp2.arc");
            bool eq4 = Compare.CompareFiles(expectedFile + ".bin" + extension, "temp2.bin");
            File.Delete("temp1.arc");
            File.Delete("temp1.bin");
            File.Delete("temp2.arc");
            File.Delete("temp2.bin");
            Assert.IsTrue(eq1);
            Assert.IsTrue(eq2);
            Assert.IsTrue(eq3);
            Assert.IsTrue(eq4);
        }

        [DataTestMethod]
        [DataRow("disc", "", "system/chara_viewer_def.bin", "other/songResource.bin", "disc")]
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
            File.Move(expectedDir + "/" + entryFilepath, "backup");
            File.Copy(replacementFile, expectedDir + "/" + entryFilepath);
            bool eq = Compare.CompareDirectories(expectedDir, "tempdir");
            File.Delete(expectedDir + "/" + entryFilepath);
            File.Move("backup", expectedDir + "/" + entryFilepath);

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

