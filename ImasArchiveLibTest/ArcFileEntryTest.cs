using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImasArchiveLib;
using System.IO;
using System.Linq;

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
    }
}
