using ImasArchiveLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ImasArchiveLibTest
{
    [TestClass]
    public class ParTest
    {
        [DataTestMethod]
        [DataRow("hdd")]
        public void ParReadHeaderTest(string srcDir)
        {
            DirectoryInfo dInfo = new DirectoryInfo(srcDir);
            FileInfo[] files = dInfo.GetFiles("*.par", SearchOption.AllDirectories);

            using StreamWriter streamWriter = new StreamWriter("log_parheader.txt");

            foreach (FileInfo file in files)
            {
                using FileStream fileStream = file.OpenRead();
                ParFile parFile = new ParFile(fileStream);
                if (parFile.Entries.Count != 0)
                {
                    streamWriter.WriteLine(file.FullName);
                    foreach (var en in parFile.Entries)
                    {
                        streamWriter.WriteLine("\t{0}\t{1}\t{2}\t{3}",
                            en.Offset.ToString("X"),
                            en.Length.ToString("X"),
                            en.Property,
                            en.FileName);
                    }
                    streamWriter.WriteLine();
                    streamWriter.Flush();
                }
            }
        }

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
        [DataRow("hdd/ui/commonCursor/commonCursorComponent.par", ".")]
        public async Task WriteParTest(string inFile, string outDir)
        {
            using (FileStream fileStream = new FileStream(inFile, FileMode.Open, FileAccess.Read))
            {
                ParFile parFile = new ParFile(fileStream);
                await parFile.ExtractAll("temp1_par");
                using (FileStream outStream = new FileStream("temp.par", FileMode.Create, FileAccess.Write))
                {
                    await parFile.SaveTo(outStream).ConfigureAwait(false);
                }
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
                using (FileStream outStream = new FileStream("temp.par", FileMode.Create, FileAccess.Write))
                {
                    await parFile.SaveTo(outStream).ConfigureAwait(false);
                }
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

        //[DataTestMethod]
        //[DataRow("hdd", "hdd4")]
        //public async Task ParSaveAll(string inDir, string outDir)
        //{
        //    DirectoryInfo dInfo = new DirectoryInfo(inDir);
        //    FileInfo[] files = dInfo.GetFiles("*", SearchOption.AllDirectories);


        //    foreach (FileInfo file in files)
        //    {
        //        if (file.Name.EndsWith(".par"))
        //        {
        //            try
        //            {
        //                using FileStream fileStream = file.OpenRead();
        //                ParFile parFile = new ParFile(fileStream);
        //                await parFile.ExtractAll(outDir + file.FullName.Substring(dInfo.FullName.Length)[0..^4] + "_par");
        //            }
        //            catch (InvalidDataException)
        //            {
        //                continue;
        //            }

        //        }
        //        else if (file.Name.EndsWith(".pta"))
        //        {
        //            try
        //            {
        //                using FileStream fileStream = file.OpenRead();
        //                ParFile parFile = new ParFile(fileStream);
        //                await parFile.ExtractAll(outDir + file.FullName.Substring(dInfo.FullName.Length)[0..^4] + "_pta");
        //            }
        //            catch (InvalidDataException)
        //            {
        //                continue;
        //            }

        //        }
        //        //else
        //        //{
        //        //    Directory.CreateDirectory(outDir + file.DirectoryName.Substring(dInfo.FullName.Length));
        //        //    File.Copy(file.FullName, outDir + file.FullName.Substring(dInfo.FullName.Length));
        //        //}
        //    }

        //}
    }
}
