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
                            en.Name);
                    }
                    streamWriter.WriteLine();
                    streamWriter.Flush();
                }
            }
        }

        [DataTestMethod]
        [DataRow("hdd/ui/commonCursor/commonCursorComponent.par", ".")]
        [DataRow("hdd/bg3d/fes_001.par", ".")]
        [DataRow("hdd/bg3d/gimmick.par", ".")]
        public async Task ParSaveTest(string inFile, string outDir)
        {
            using FileStream fileStream = new FileStream(inFile, FileMode.Open, FileAccess.Read);
            ParFile parFile = new ParFile(fileStream);
            await parFile.ExtractAll(outDir + inFile.Substring(inFile.LastIndexOf('/'))[0..^4] + "_par");

        }

        [DataTestMethod]
        [DataRow("hdd", "hdd4")]
        public async Task ParSaveAll(string inDir, string outDir)
        {
            DirectoryInfo dInfo = new DirectoryInfo(inDir);
            FileInfo[] files = dInfo.GetFiles("*", SearchOption.AllDirectories);


            foreach (FileInfo file in files)
            {
                if (file.Name.EndsWith(".par"))
                {
                    try
                    {
                        using FileStream fileStream = file.OpenRead();
                        ParFile parFile = new ParFile(fileStream);
                        await parFile.ExtractAll(outDir + file.FullName.Substring(dInfo.FullName.Length)[0..^4] + "_par");
                    }
                    catch (InvalidDataException)
                    {
                        continue;
                    }

                }
                else if (file.Name.EndsWith(".pta"))
                {
                    try
                    {
                        using FileStream fileStream = file.OpenRead();
                        ParFile parFile = new ParFile(fileStream);
                        await parFile.ExtractAll(outDir + file.FullName.Substring(dInfo.FullName.Length)[0..^4] + "_pta");
                    }
                    catch (InvalidDataException)
                    {
                        continue;
                    }

                }
                //else
                //{
                //    Directory.CreateDirectory(outDir + file.DirectoryName.Substring(dInfo.FullName.Length));
                //    File.Copy(file.FullName, outDir + file.FullName.Substring(dInfo.FullName.Length));
                //}
            }

        }
    }
}
