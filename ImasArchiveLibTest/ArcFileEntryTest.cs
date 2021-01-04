using Imas;
using Imas.Archive;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ImasArchiveLibTest
{
    [TestClass]
    public class ArcFileEntryTest
    {
        private readonly Progress<ProgressData> progress = new Progress<ProgressData>(
            pair => Console.WriteLine(" {0} of {1}: {2} ", pair.count, pair.total, pair.filename));

        [AssemblyInitialize]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Specific signature needed for AssemblyInitialise>")]
        public static void SetupDirectory(TestContext testContext)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(System.Environment.CurrentDirectory + @"\..\..\..\..\..\test");
            System.Environment.CurrentDirectory = directoryInfo.FullName;
        }

        [DataTestMethod]
        [DataRow("hdd.arc", 0x13DF)]
        [DataRow("disc.arc", 0x1B8)]
        public void EntryCount(string filename, int expectedCount)
        {
            int actualCount;
            using (ArcFile arcFile = new ArcFile(filename))
            {
                actualCount = arcFile.Entries.Count;
            }

            Assert.AreEqual(expectedCount, actualCount);
        }

        [DataTestMethod]
        [DataRow("hdd.arc", "songinfo/songResource.bin", "other/songResource.bin")]
        [DataRow("hdd.arc", "commu2/par/ami_bs2_c01.par", "other/ami_bs2_c01.par")]
        public async Task GetEntryAndWriteToFile(string filename, string entryFilepath, string expectedFile)
        {
            using ArcFile arcFile = new ArcFile(filename);
            ContainerEntry arcEntry = arcFile.GetEntry(entryFilepath);
            if (arcEntry == null)
                Assert.Fail("Entry not found.");
            using (Stream stream = await arcEntry.GetData())
            {
                using FileStream fileStream = new FileStream("temp.dat", FileMode.Create, FileAccess.Write);
                stream.CopyTo(fileStream);
            }
            bool eq = Compare.CompareFiles(expectedFile, "temp.dat");
            File.Delete("temp.dat");
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("hdd.arc", "commu2/par/ami_bs2_c01_par/ami_bs2_c01_m.bin", "other/ami_bs2_c01_m.bin")]
        public async Task GetEntryRecursiveAndWriteToFile(string filename, string entryFilepath, string expectedFile)
        {
            using ArcFile arcFile = new ArcFile(filename);
            using EntryStack stack = await arcFile.GetEntryRecursive(entryFilepath);
            if (stack == null)
                Assert.Fail("Entry not found.");
            using (Stream stream = await stack.Entry.GetData())
            {
                using FileStream fileStream = new FileStream("temp.dat", FileMode.Create, FileAccess.Write);
                stream.CopyTo(fileStream);
            }
            bool eq = Compare.CompareFiles(expectedFile, "temp.dat");
            File.Delete("temp.dat");
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("disc.arc", false, "disc")]
        [DataRow("little.arc", true, "little_pars")]
        public async Task ExtractAllAsync(string filename, bool extractPar, string expectedFolder)
        {
            using ArcFile arcFile = new ArcFile(filename);
            await Task.Run(() => arcFile.ExtractAllAsync("temp", extractPar, progress));
            bool eq = Compare.CompareDirectories(expectedFolder, "temp");
            DirectoryInfo directoryInfo = new DirectoryInfo("temp");
            directoryInfo.Delete(true);
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("disc.arc")]
        public async Task ListFileNamesTest(string filename)
        {
            using ArcFile arcFile = new ArcFile(filename);
            using TextWriter textWriter = new StreamWriter("log_arcnames.txt");
            await arcFile.ForAll((entry, name) => textWriter.WriteLine(name));
        }

        [DataTestMethod]
        [DataRow("little.arc", "little_replace", "little_exp")]
        public async Task ReplaceTest(string filename, string replacementFolder, string expectedFolder)
        {
            using (ArcFile arcFile = new ArcFile(filename))
            {
                await arcFile.ReplaceEntries(new FileSource(replacementFolder), progress);
                await arcFile.SaveAs("temp.arc");
            }
            using (ArcFile arcFile = new ArcFile("temp.arc"))
            {
                await arcFile.ExtractAllAsync("tempdir", true, progress);
            }
            bool eq = Compare.CompareDirectories(expectedFolder, "tempdir");
            File.Delete("temp.arc");
            File.Delete("temp.bin");
            DirectoryInfo directoryInfo = new DirectoryInfo("tempdir");
            directoryInfo.Delete(true);
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("little.arc", "little_replace.zip", "little_exp")]
        public async Task ReplaceZipTest(string filename, string replacementZip, string expectedFolder)
        {
            using (ArcFile arcFile = new ArcFile(filename))
            {
                using ZipSourceParent zipSourceParent = new ZipSourceParent(replacementZip);
                await arcFile.ReplaceEntries(zipSourceParent.GetZipSource(), progress);
                await arcFile.SaveAs("temp.arc");
            }
            using (ArcFile arcFile = new ArcFile("temp.arc"))
            {
                await arcFile.ExtractAllAsync("tempdir", true, progress);
            }
            bool eq = Compare.CompareDirectories(expectedFolder, "tempdir");
            File.Delete("temp.arc");
            File.Delete("temp.bin");
            DirectoryInfo directoryInfo = new DirectoryInfo("tempdir");
            directoryInfo.Delete(true);
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("little.arc", "little_replace", "little_exp")]
        public async Task ReplaceSaveTest(string filename, string replacementFolder, string expectedFolder)
        {
            await ArcFile.PatchArcFromFolder(filename, replacementFolder, "temp.arc", progress);
            using (ArcFile arcFile = new ArcFile("temp.arc"))
            {
                await arcFile.ExtractAllAsync("tempdir", true, progress);
            }
            bool eq = Compare.CompareDirectories(expectedFolder, "tempdir");
            File.Delete("temp.arc");
            File.Delete("temp.bin");
            DirectoryInfo directoryInfo = new DirectoryInfo("tempdir");
            directoryInfo.Delete(true);
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("little.arc", "little_replace.zip", "little_exp")]
        public async Task ReplaceSaveZipTest(string filename, string replacementZip, string expectedFolder)
        {
            await ArcFile.PatchArcFromZip(filename, replacementZip, "temp.arc", progress);
            using (ArcFile arcFile = new ArcFile("temp.arc"))
            {
                await arcFile.ExtractAllAsync("tempdir", true, progress);
            }
            bool eq = Compare.CompareDirectories(expectedFolder, "tempdir");
            File.Delete("temp.arc");
            File.Delete("temp.bin");
            DirectoryInfo directoryInfo = new DirectoryInfo("tempdir");
            directoryInfo.Delete(true);
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("hdd.arc")]
        [DataRow("disc.arc")]
        public async Task SaveArcAs(string filename)
        {
            using (ArcFile arcFile = new ArcFile(filename))
            {
                await arcFile.SaveAs("temp.arc", progress);
            }
            bool eq = Compare.CompareFiles(filename, "temp.arc") && Compare.CompareFiles(ArcFile.GetBinName(filename), "temp.bin");
            File.Delete("temp.arc");
            File.Delete("temp.bin");
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("disc.arc", "system/chara_viewer_def.bin", "other/songResource.bin", "other/disc_edited.arc")]
        public async Task EditThenSaveArcAsTwice(string filename, string entryPath, string replacementFile, string expectedFile)
        {
            using (ArcFile arcFile = new ArcFile(filename))
            {
                ContainerEntry arcEntry = arcFile.GetEntry(entryPath);
                if (arcEntry == null)
                    Assert.Fail("Entry not found.");
                using (FileStream fileStream = new FileStream(replacementFile, FileMode.Open, FileAccess.Read))
                {
                    await arcEntry.SetData(fileStream);
                }
                await arcFile.SaveAs("temp1.arc", progress);
                await arcFile.SaveAs("temp2.arc", progress);
            }
            bool eq1 = Compare.CompareFiles(expectedFile, "temp1.arc");
            bool eq2 = Compare.CompareFiles(ArcFile.GetBinName(expectedFile), "temp1.bin");
            bool eq3 = Compare.CompareFiles(expectedFile, "temp2.arc");
            bool eq4 = Compare.CompareFiles(ArcFile.GetBinName(expectedFile), "temp2.bin");
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
        [DataRow("disc.arc", "system/chara_viewer_def.bin", "other/songResource.bin", "disc")]
        public async Task EditEntryReextractTest(string filename, string entryFilepath, string replacementFile, string expectedDir)
        {
            using (ArcFile arcFile = new ArcFile(filename))
            {
                ContainerEntry arcEntry = arcFile.GetEntry(entryFilepath);
                if (arcEntry == null)
                    Assert.Fail("Entry not found.");
                using (FileStream fileStream = new FileStream(replacementFile, FileMode.Open, FileAccess.Read))
                {
                    await arcEntry.SetData(fileStream);
                }
                await arcFile.SaveAs("temp.arc", progress);
            }
            using (ArcFile arcFile = new ArcFile("temp.arc"))
            {
                await arcFile.ExtractAllAsync("tempdir", false, progress);
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
            await ArcFile.BuildFromDirectory(dir, "temp.arc", progress);
            using (ArcFile arcFile = new ArcFile("temp.arc"))
            {
                await arcFile.ExtractAllAsync("tempdir", false, progress);
            }

            bool eq = Compare.CompareDirectories(dir, "tempdir");

            DirectoryInfo directoryInfo = new DirectoryInfo("tempdir");
            directoryInfo.Delete(true);
            File.Delete("temp.arc");
            File.Delete("temp.bin");

            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("hdd.arc", "other/commus.xlsx")]
        public async Task ArcFileCommuTest(string arcName, string outputXlsx)
        {
            using ArcFile arcFile = new ArcFile(arcName);
            await arcFile.ExtractCommusToXlsx(outputXlsx, true, progress);
        }

        [DataTestMethod]
        [DataRow("D:/OFA_arc/original/_dlc07.arc.dat", "other/parameter_dlc07.xlsx")]
        [DataRow("disc.arc", "other/parameter_disc.xlsx")]
        [DataRow("hdd.arc", "other/parameter_hdd.xlsx")]
        public async Task ArcParameterIndividualTest(string arcName, string outputXlsx)
        {
            using ArcFile arcFile = new ArcFile(arcName);
            await arcFile.ExtractParameterToXlsx(outputXlsx, true, progress);
        }

        [DataTestMethod]
        [DataRow("other/parameter.xlsx")]
        public async Task ArcParameterCombinedTest(string outputXlsx)
        {
            using (ArcFile arcFile = new ArcFile("disc.arc"))
            {
                await arcFile.ExtractParameterToXlsx(outputXlsx, true, progress);
            }
            using (ArcFile arcFile = new ArcFile("hdd.arc"))
            {
                await arcFile.ExtractParameterToXlsx(outputXlsx, false, progress);
            }
        }

        [DataTestMethod]
        [DataRow("disc.arc", "images/disc_images")]
        //[DataRow("hdd.arc", "images/hdd_images")]
        public async Task ExtractAllImagesTest(string arcName, string outDir)
        {
            using ArcFile arcFile = new ArcFile(arcName);
            await arcFile.ExtractAllImages(outDir);
        }

        [DataTestMethod]
        [DataRow("hdd.arc", "lyrics/hdd_lyrics")]
        public async Task ExtractLyricsTest(string arcName, string outDir)
        {
            using ArcFile arcFile = new ArcFile(arcName);
            await arcFile.ExtractLyrics(outDir);
        }
    }
}