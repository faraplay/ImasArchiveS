﻿using ImasArchiveLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading.Tasks;

namespace ImasArchiveLibTest
{
    [TestClass]
    public class ParCommuTest
    {

        [DataTestMethod]
        [DataRow("hdd", "", "commu2/par/_week_00_002.par.gz", "hddcommu")]
        public void ArcEntryCommuTest(string arcName, string extension, string searchFile, string expectedDir)
        {
            using ArcFile arcFile = new ArcFile(arcName, extension);
            ArcEntry arcEntry = arcFile.GetEntry(searchFile);
            Assert.IsNotNull(arcEntry);
            Directory.CreateDirectory("tempdir");
            Assert.IsTrue(arcEntry.TryGetCommuText("tempdir"));
            string filename = searchFile.Substring(searchFile.LastIndexOf('/'))[0..^7] + "_m.txt";
            bool eq = Compare.CompareFiles("tempdir/" + filename, expectedDir + "/" + filename);
            Directory.Delete("tempdir", true);
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("hdd", "", "hddcommu")]
        public void ArcFileCommuTest(string arcName, string extension, string expectedDir)
        {
            using ArcFile arcFile = new ArcFile(arcName, extension);
            var arcEntries = arcFile.Entries;
            Directory.CreateDirectory("tempdir");
            foreach (ArcEntry arcEntry in arcEntries)
            {
                Assert.IsNotNull(arcEntry);
                arcEntry.TryGetCommuText("tempdir");
            }
            bool eq = Compare.CompareDirectories(expectedDir, "tempdir");
            Directory.Delete("tempdir", true);
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("hdd", "", "commu2/par/_week_00_002.par.gz", "other/translated.txt", "other/translated.par")]
        public async Task ArcEntryCommuReplaceTest(string arcName, string extension, string searchFile, 
            string replacementFile, string expectedFile)
        {
            using ArcFile arcFile = new ArcFile(arcName, extension);
            ArcEntry arcEntry = arcFile.GetEntry(searchFile);
            Assert.IsNotNull(arcEntry);
            await arcEntry.TryReplaceCommuText(replacementFile);

            using (FileStream fileStream = new FileStream("temp.par", FileMode.Create, FileAccess.Write))
            {
                using Stream stream = arcEntry.Open();
                stream.CopyTo(fileStream);
            }

            bool eq = Compare.CompareFiles(expectedFile, "temp.par");
            File.Delete("temp.par");
            Assert.IsTrue(eq);
        }

        [DataTestMethod]
        [DataRow("hdd", "", "other/translated.txt")]
        public async Task ArcFileCommuReplaceTest(string arcName, string extension, string replacementFile)
        {
            using ArcFile arcFile = new ArcFile(arcName, extension);
            await arcFile.ReplaceCommu(replacementFile);
            //await arcFile.SaveAs("test");
        }


        [DataTestMethod]
        [DataRow("hdd", "", "translated")]
        public async Task ArcFileMassReplaceTest(string arcName, string extension, string replacementDir)
        {
            using ArcFile arcFile = new ArcFile(arcName, extension);
            DirectoryInfo directoryInfo = new DirectoryInfo(replacementDir);
            FileInfo[] fileInfos = directoryInfo.GetFiles();
            foreach (FileInfo file in fileInfos)
            {
                await arcFile.ReplaceCommu(file.FullName);
            }
            await arcFile.SaveAs("../data/hdd");
        }
    }
}