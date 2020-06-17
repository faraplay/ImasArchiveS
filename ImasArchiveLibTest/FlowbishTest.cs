using ImasArchiveLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace ImasArchiveLibTest
{
    [TestClass]
    public class FlowbishTest
    {

        [DataTestMethod]
        [DataRow(@"C:\Users\harve\source\repos\imas_archive\test\songResource.bin.gz.fbs", "songResource.bin.gz", @"C:\Users\harve\source\repos\imas_archive\test\songResource.bin.gz")]
        public void DecryptWholeFileTest(string inputFile, string name, string expectedFile)
        {
            using (FileStream fileStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
            {
                using FlowbishStream flowbishStream = new FlowbishStream(fileStream, FlowbishStreamMode.Decipher, name);
                using FileStream outStream = new FileStream("temp.dat", FileMode.Create, FileAccess.Write);
                flowbishStream.CopyTo(outStream);
            }
            bool eq = ArcFileEntryTest.CompareFiles(expectedFile, "temp.dat");
            File.Delete("temp.dat");
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow(@"C:\Users\harve\source\repos\imas_archive\test\songResource.bin.gz.fbs", "songResource.bin.gz", @"C:\Users\harve\source\repos\imas_archive\test\songResource.bin.gz")]
        public void DecryptReadByteTest(string inputFile, string name, string expectedFile)
        {
            using (FileStream fileStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
            {
                using FlowbishStream flowbishStream = new FlowbishStream(fileStream, FlowbishStreamMode.Decipher, name);
                using FileStream outStream = new FileStream("temp.dat", FileMode.Create, FileAccess.Write);
                int b;
                while ((b = flowbishStream.ReadByte()) != -1)
                {
                    outStream.WriteByte((byte)b);
                }
            }
            bool eq = ArcFileEntryTest.CompareFiles(expectedFile, "temp.dat");
            File.Delete("temp.dat");
            Assert.IsTrue(eq);
        }


        [DataTestMethod]
        [DataRow(@"C:\Users\harve\source\repos\imas_archive\test\songResource.bin.gz.fbs", "songResource.bin.gz", @"C:\Users\harve\source\repos\imas_archive\test\songResource.bin.gz", 32)]
        [DataRow(@"C:\Users\harve\source\repos\imas_archive\test\songResource.bin.gz.fbs", "songResource.bin.gz", @"C:\Users\harve\source\repos\imas_archive\test\songResource.bin.gz", 0x1040)]
        public void DecryptSeekTest(string inputFile, string name, string expectedFile, int offset)
        {
            using (FileStream fileStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
            {
                using FlowbishStream flowbishStream = new FlowbishStream(fileStream, FlowbishStreamMode.Decipher, name);
                using FileStream outStream = new FileStream("temp.dat", FileMode.Create, FileAccess.Write);
                flowbishStream.Seek(offset, SeekOrigin.Begin);
                flowbishStream.CopyTo(outStream);
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
