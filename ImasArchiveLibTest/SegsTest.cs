using Imas.Streams;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading.Tasks;

namespace ImasArchiveLibTest
{
    [TestClass]
    public class SegsTest
    {

        [DataTestMethod]
        [DataRow("other/songResource.bin.gz", "other/songResource.bin")]
        [DataRow("other/_week_00_002.par.gz", "other/_week_00_002.par")]
        public void DecryptWholeFileTest(string inputFile, string expectedFile)
        {
            using (FileStream fileStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
            {
                using SegsStream segsStream = new SegsStream(fileStream, SegsStreamMode.Decompress);
                using FileStream outStream = new FileStream("temp.dat", FileMode.Create, FileAccess.Write);
                segsStream.CopyTo(outStream);
            }
            bool eq = Compare.CompareFiles(expectedFile, "temp.dat");
            File.Delete("temp.dat");
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("other/songResource.bin.gz", "other/songResource.bin")]
        [DataRow("other/_week_00_002.par.gz", "other/_week_00_002.par")]
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
            bool eq = Compare.CompareFiles(expectedFile, "temp.dat");
            File.Delete("temp.dat");
            Assert.IsTrue(eq);
        }


        [DataTestMethod]
        [DataRow("other/songResource.bin.gz", "other/songResource.bin", 32)]
        [DataRow("other/_week_00_002.par.gz", "other/_week_00_002.par", 0x1040)]
        [DataRow("other/_week_00_002.par.gz", "other/_week_00_002.par", 0x50123)]
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
            bool eq = Compare.CompareFiles("temp_exp.dat", "temp.dat");
            File.Delete("temp.dat");
            File.Delete("temp_exp.dat");
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("other/songResource.bin")]
        [DataRow("other/_week_00_002.par")]
        [DataRow("other/ami_bs2_c01.par")]
        public async Task CompressUncompressTest(string inputFile)
        {
            using (FileStream fileStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
            {
                using FileStream outStream = new FileStream("temp.dat", FileMode.Create, FileAccess.Write);
                await SegsStream.CompressStream(fileStream, outStream);
            }
            using (FileStream compStream = new FileStream("temp.dat", FileMode.Open, FileAccess.Read))
            {
                using SegsStream segsStream = new SegsStream(compStream, SegsStreamMode.Decompress);
                using FileStream outStream = new FileStream("temp2.dat", FileMode.Create, FileAccess.Write);
                segsStream.CopyTo(outStream);
            }
            bool eq = Compare.CompareFiles(inputFile, "temp2.dat");
            File.Delete("temp.dat");
            File.Delete("temp2.dat");
            Assert.IsTrue(eq);
        }
    }
}
