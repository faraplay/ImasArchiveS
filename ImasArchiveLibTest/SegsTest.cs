using ImasArchiveLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace ImasArchiveLibTest
{
    [TestClass]
    public class SegsTest
    {

        [DataTestMethod]
        [DataRow(@"C:\Users\harve\source\repos\imas_archive\test\songResource.bin.gz", @"C:\Users\harve\source\repos\imas_archive\test\songResource.bin")]
        [DataRow(@"C:\Users\harve\source\repos\imas_archive\test\_week_00_002.par.gz", @"C:\Users\harve\source\repos\imas_archive\test\_week_00_002.par")]
        public void DecryptWholeFileTest(string inputFile, string expectedFile)
        {
            using (FileStream fileStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
            {
                using SegsStream segsStream = new SegsStream(fileStream, SegsStreamMode.Decompress);
                using FileStream outStream = new FileStream("temp.dat", FileMode.Create, FileAccess.Write);
                segsStream.CopyTo(outStream);
            }
            bool eq = ArcFileEntryTest.CompareFiles(expectedFile, "temp.dat");
            File.Delete("temp.dat");
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow(@"C:\Users\harve\source\repos\imas_archive\test\songResource.bin.gz", @"C:\Users\harve\source\repos\imas_archive\test\songResource.bin")]
        [DataRow(@"C:\Users\harve\source\repos\imas_archive\test\_week_00_002.par.gz", @"C:\Users\harve\source\repos\imas_archive\test\_week_00_002.par")]
        public void DecryptReadByteTest(string inputFile, string expectedFile)
        {
            using (FileStream fileStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
            {
                using SegsStream segsStream = new SegsStream(fileStream, SegsStreamMode.Decompress);
                using FileStream outStream = new FileStream("temp.dat", FileMode.Create, FileAccess.Write);
                int b;
                while ((b = segsStream.ReadByte()) != -1)
                {
                    outStream.WriteByte((byte)b);
                }
            }
            bool eq = ArcFileEntryTest.CompareFiles(expectedFile, "temp.dat");
            File.Delete("temp.dat");
            Assert.IsTrue(eq);
        }


        [DataTestMethod]
        [DataRow(@"C:\Users\harve\source\repos\imas_archive\test\songResource.bin.gz", @"C:\Users\harve\source\repos\imas_archive\test\songResource.bin", 32)]
        [DataRow(@"C:\Users\harve\source\repos\imas_archive\test\_week_00_002.par.gz", @"C:\Users\harve\source\repos\imas_archive\test\_week_00_002.par", 0x1040)]
        [DataRow(@"C:\Users\harve\source\repos\imas_archive\test\_week_00_002.par.gz", @"C:\Users\harve\source\repos\imas_archive\test\_week_00_002.par", 0x50123)]
        public void DecryptSeekTest(string inputFile, string expectedFile, int offset)
        {
            using (FileStream fileStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
            {
                using SegsStream segsStream = new SegsStream(fileStream, SegsStreamMode.Decompress);
                using FileStream outStream = new FileStream("temp.dat", FileMode.Create, FileAccess.Write);
                segsStream.Seek(offset, SeekOrigin.Begin);
                segsStream.CopyTo(outStream);
            }
            using (FileStream fileStream1 = new FileStream(expectedFile, FileMode.Open, FileAccess.Read))
            {
                using FileStream outStream = new FileStream("temp_exp.dat", FileMode.Create, FileAccess.Write);
                fileStream1.Seek(offset, SeekOrigin.Begin);
                fileStream1.CopyTo(outStream);
            }
            bool eq = ArcFileEntryTest.CompareFiles("temp_exp.dat", "temp.dat");
            File.Delete("temp.dat");
            File.Delete("temp_exp.dat");
            Assert.IsTrue(eq);
        }
    }
}
