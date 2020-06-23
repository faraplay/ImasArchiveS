using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImasArchiveLib;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImasArchiveLibTest
{
    [TestClass]
    public class ArcFileEntryTest
    {
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
        [DataRow("hdd", "", "songinfo/songResource.bin.gz", "songResource.bin.gz.fbs")]
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
        [DataRow("hdd", "", "songinfo/songResource.bin.gz", "songResource.bin")]
        [DataRow("hdd", "", "commu2/par/ami_bs2_c01.par.gz", "ami_bs2_c01.par")]
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
        [DataRow("hdd", "", "hdd_out")]
        [DataRow("disc", "", "disc_out")]
        [DataRow("dlc\\_dlc03", ".dat", "_dlc03_out")]
        public void ExtractAllUncompress(string filename, string extension, string expectedFolder)
        {
            using ArcFile arcFile = new ArcFile(filename, extension);
            arcFile.ExtractAll("test");
            bool eq = Compare.CompareDirectories(expectedFolder, "test");
            DirectoryInfo directoryInfo = new DirectoryInfo("test");
            directoryInfo.Delete(true);
            Assert.IsTrue(eq);
        }


        [DataTestMethod]
        [DataRow("hdd", "", "hdd_out")]
        [DataRow("disc", "", "disc_out")]
        [DataRow("dlc\\_dlc03", ".dat", "_dlc03_out")]
        public async Task ExtractAllUncompressAsync(string filename, string extension, string expectedFolder)
        {
            using ArcFile arcFile = new ArcFile(filename, extension);
            await Task.Run(() => arcFile.ExtractAllAsync("test"));
            bool eq = Compare.CompareDirectories(expectedFolder, "test");
            DirectoryInfo directoryInfo = new DirectoryInfo("test");
            directoryInfo.Delete(true);
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("hdd", "", "hdd")]
        [DataRow("disc", "", "disc")]
        public void ExtractAllCompressed(string filename, string extension, string expectedFolder)
        {
            using ArcFile arcFile = new ArcFile(filename, extension);
            string destDir = "test";
            if (destDir.EndsWith('/'))
                destDir = destDir.Substring(0, destDir.Length - 1);
            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);
            DirectoryInfo directoryInfo = new DirectoryInfo(destDir);
            foreach (ArcEntry arcEntry in arcFile.Entries)
            {
                string dirs = arcEntry.Filepath.Substring(0, arcEntry.Filepath.LastIndexOf('/') + 1);
                if (dirs != "")
                    directoryInfo.CreateSubdirectory(dirs);
                using FileStream fileStream = new FileStream(destDir + '/' + arcEntry.Filepath, FileMode.Create, FileAccess.Write);
                using Stream stream = arcEntry.OpenRaw();
                using FlowbishStream flowbishStream = new FlowbishStream(stream, FlowbishStreamMode.Decipher, arcEntry.Name);
                flowbishStream.CopyTo(fileStream);
            }
            bool eq = Compare.CompareDirectories(expectedFolder, "test");
            directoryInfo.Delete(true);
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("hdd", "")]
        [DataRow("disc", "")]
        [DataRow("dlc\\_dlc03", ".dat")]
        public void SaveArcAs(string filename, string extension)
        {
            using (ArcFile arcFile = new ArcFile(filename, extension))
            {
                arcFile.SaveAs("test");
            }
            bool eq = Compare.CompareFileHashes(filename + ".arc" + extension, "test.arc") && Compare.CompareFileHashes(filename + ".bin" + extension, "test.bin");
            File.Delete("test.arc");
            File.Delete("test.bin");
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("disc", "", "system/chara_viewer_def.bin.gz", "songResource.bin", "disc_out")]
        [DataRow("hdd", "", "commu2/par/ami_bs2_c01.par.gz", "week4-3.bin", "hdd_out")]
        public void EditEntryReextractTest(string filename, string extension, string entryFilepath, string replacementFile, string expectedDir)
        {

            using (ArcFile arcFile = new ArcFile(filename, extension))
            {
                ArcEntry arcEntry = arcFile.GetEntry(entryFilepath);
                if (arcEntry == null)
                    Assert.Fail("Entry not found.");
                using (FileStream fileStream = new FileStream(replacementFile, FileMode.Open, FileAccess.Read))
                {
                    arcEntry.Replace(fileStream);
                }
                arcFile.SaveAs("test");
            }
            using (ArcFile arcFile = new ArcFile("test", ""))
            {
                Task task = arcFile.ExtractAllAsync("testdir");
                task.Wait();
            }
            File.Move(expectedDir + "/" + entryFilepath.Substring(0, entryFilepath.Length - 3), "backup");
            File.Copy(replacementFile, expectedDir + "/" + entryFilepath.Substring(0, entryFilepath.Length - 3));
            bool eq = Compare.CompareDirectories(expectedDir, "testdir");
            File.Delete(expectedDir + "/" + entryFilepath.Substring(0, entryFilepath.Length - 3));
            File.Move("backup", expectedDir + "/" + entryFilepath.Substring(0, entryFilepath.Length - 3));

            DirectoryInfo directoryInfo = new DirectoryInfo("testdir");
            directoryInfo.Delete(true);
            File.Delete("test.arc");
            File.Delete("test.bin");
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("little")]
        [DataRow("disc_out")]
        public void BuildNewArcTest(string dir)
        {
            ArcFile.BuildFromDirectory(dir, "test");
            using (ArcFile arcFile = new ArcFile("test"))
            {
                Task task = arcFile.ExtractAllAsync("testdir");
                task.Wait();
            }

            bool eq = Compare.CompareDirectories(dir, "testdir");

            DirectoryInfo directoryInfo = new DirectoryInfo("testdir");
            directoryInfo.Delete(true);
            File.Delete("test.arc");
            File.Delete("test.bin");

            Assert.IsTrue(eq);
        }

    }
}

