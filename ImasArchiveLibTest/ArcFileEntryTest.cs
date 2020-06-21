using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImasArchiveLib;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace ImasArchiveLibTest
{
    [TestClass]
    public class ArcFileEntryTest
    {
        [DataTestMethod]
        [DataRow(@"C:\Users\harve\source\repos\imas_archive\test\hdd", "", 0x13DF)]
        [DataRow(@"C:\Users\harve\source\repos\imas_archive\test\disc", "", 0x1B8)]
        [DataRow(@"C:\Users\harve\source\repos\imas_archive\test\dlc\_dlc03", ".dat", 0x1D7)]
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
        [DataRow(@"C:\Users\harve\source\repos\imas_archive\test\hdd", "")]
        [DataRow(@"C:\Users\harve\source\repos\imas_archive\test\disc", "")]
        [DataRow(@"C:\Users\harve\source\repos\imas_archive\test\dlc\_dlc03", ".dat")]
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
        [DataRow(@"C:\Users\harve\source\repos\imas_archive\test\hdd", "")]
        [DataRow(@"C:\Users\harve\source\repos\imas_archive\test\disc", "")]
        [DataRow(@"C:\Users\harve\source\repos\imas_archive\test\dlc\_dlc03", ".dat")]
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
        [DataRow(@"C:\Users\harve\source\repos\imas_archive\test\hdd", "", "songinfo/songResource.bin.gz", @"C:\Users\harve\source\repos\imas_archive\test\songResource.bin.gz.fbs")]
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
            bool eq = CompareFiles(expectedFile, "temp.dat");
            File.Delete("temp.dat");
            Assert.IsTrue(eq);
        }


        [DataTestMethod]
        [DataRow(@"C:\Users\harve\source\repos\imas_archive\test\hdd", "", "songinfo/songResource.bin.gz", @"C:\Users\harve\source\repos\imas_archive\test\songResource.bin")]
        [DataRow(@"C:\Users\harve\source\repos\imas_archive\test\hdd", "", "commu2/par/ami_bs2_c01.par.gz", @"C:\Users\harve\source\repos\imas_archive\test\ami_bs2_c01.par")]
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
            bool eq = CompareFiles(expectedFile, "temp.dat");
            File.Delete("temp.dat");
            Assert.IsTrue(eq);
        }


        [DataTestMethod]
        [DataRow(@"C:\Users\harve\source\repos\imas_archive\test\hdd", "", @"C:\Users\harve\source\repos\imas_archive\test\hdd_out")]
        [DataRow(@"C:\Users\harve\source\repos\imas_archive\test\disc", "", @"C:\Users\harve\source\repos\imas_archive\test\disc_out")]
        [DataRow(@"C:\Users\harve\source\repos\imas_archive\test\dlc\_dlc03", ".dat", @"C:\Users\harve\source\repos\imas_archive\test\_dlc03_out")]
        public void ExtractAllUncompress(string filename, string extension, string expectedFolder)
        {
            using ArcFile arcFile = new ArcFile(filename, extension);
            arcFile.ExtractAll("test");
            bool eq = CompareDirectories(expectedFolder, "test");
            DirectoryInfo directoryInfo = new DirectoryInfo("test");
            directoryInfo.Delete(true);
            Assert.IsTrue(eq);
        }


        [DataTestMethod]
        [DataRow(@"C:\Users\harve\source\repos\imas_archive\test\hdd", "", @"C:\Users\harve\source\repos\imas_archive\test\hdd_out")]
        [DataRow(@"C:\Users\harve\source\repos\imas_archive\test\disc", "", @"C:\Users\harve\source\repos\imas_archive\test\disc_out")]
        [DataRow(@"C:\Users\harve\source\repos\imas_archive\test\dlc\_dlc03", ".dat", @"C:\Users\harve\source\repos\imas_archive\test\_dlc03_out")]
        public async Task ExtractAllUncompressAsync(string filename, string extension, string expectedFolder)
        {
            using ArcFile arcFile = new ArcFile(filename, extension);
            await Task.Run(() => arcFile.ExtractAllAsync("test"));
            bool eq = CompareDirectories(expectedFolder, "test");
            DirectoryInfo directoryInfo = new DirectoryInfo("test");
            directoryInfo.Delete(true);
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow(@"C:\Users\harve\source\repos\imas_archive\test\hdd", "", @"C:\Users\harve\source\repos\imas_archive\test\hdd")]
        //[DataRow(@"C:\Users\harve\source\repos\imas_archive\test\disc", "", @"C:\Users\harve\source\repos\imas_archive\test\disc")]
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
                directoryInfo.CreateSubdirectory(dirs);
                using FileStream fileStream = new FileStream(destDir + '/' + arcEntry.Filepath, FileMode.Create, FileAccess.Write);
                using Stream stream = arcEntry.OpenRaw();
                using FlowbishStream flowbishStream = new FlowbishStream(stream, FlowbishStreamMode.Decipher, arcEntry.Name);
                flowbishStream.CopyTo(fileStream);
            }
            bool eq = CompareDirectories(expectedFolder, "test");
            directoryInfo.Delete(true);
            Assert.IsTrue(eq);
        }

        public static bool CompareFiles(string file1, string file2)
        {
            using FileStream fileStream1 = new FileStream(file1, FileMode.Open, FileAccess.Read);
            using FileStream fileStream2 = new FileStream(file2, FileMode.Open, FileAccess.Read);
            if (fileStream1.Length != fileStream2.Length)
                return false;
            for (long i = 0; i < fileStream1.Length; i++)
            {
                if (fileStream1.ReadByte() != fileStream2.ReadByte())
                    return false;
            }
            return true;
        }

        public static bool CompareFileHashes(string file1, string file2)
        {
            using FileStream fileStream1 = new FileStream(file1, FileMode.Open, FileAccess.Read);
            using FileStream fileStream2 = new FileStream(file2, FileMode.Open, FileAccess.Read);
            if (fileStream1.Length != fileStream2.Length)
                return false;
            byte[] hash1, hash2;
            using (SHA256Managed sha = new SHA256Managed())
            {
                hash1 = sha.ComputeHash(fileStream1);
                hash2 = sha.ComputeHash(fileStream2);
            }
            for (int i=0; (i < hash1.Length); i++)
            {
                if (hash1[i] != hash2[i])
                    return false;
            }
            return true;
        }

        public static bool CompareDirectories(string dir1, string dir2)
        {
            DirectoryInfo directoryInfo1 = new DirectoryInfo(dir1);
            DirectoryInfo directoryInfo2 = new DirectoryInfo(dir2);
            foreach (var file in directoryInfo1.GetFiles("*", SearchOption.AllDirectories))
            {
                string relPath = file.FullName.Substring(directoryInfo1.FullName.Length);
                string file2 = directoryInfo2.FullName + relPath;
                if (!File.Exists(file2))
                    return false;
                if (!CompareFileHashes(file.FullName, file2))
                    return false;
            }
            return true;
        }
    }
}
