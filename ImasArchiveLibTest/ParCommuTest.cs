using ImasArchiveLib;
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
        public void ArcEntryCommuTest(string arcName, string extension, string searchFile, string outputDir)
        {
            using ArcFile arcFile = new ArcFile(arcName, extension);
            ArcEntry arcEntry = arcFile.GetEntry(searchFile);
            Assert.IsNotNull(arcEntry);
            Directory.CreateDirectory(outputDir);
            Assert.IsTrue(arcEntry.TryGetCommuText(outputDir));
        }

        [DataTestMethod]
        [DataRow("hdd", "", "hddcommu")]
        public void ArcFileCommuTest(string arcName, string extension, string outputDir)
        {
            using ArcFile arcFile = new ArcFile(arcName, extension);
            var arcEntries = arcFile.Entries;
            Directory.CreateDirectory(outputDir);
            foreach (ArcEntry arcEntry in arcEntries)
            {
                Assert.IsNotNull(arcEntry);
                arcEntry.TryGetCommuText(outputDir);
            }
        }

        [DataTestMethod]
        [DataRow("hdd", "", "commu2/par/_week_00_002.par.gz", "other/translated.txt", "other/translated.par")]
        public async Task ArcEntryCommuReplaceTest(string arcName, string extension, string searchFile, 
            string replacementFile, string outFile)
        {
            using ArcFile arcFile = new ArcFile(arcName, extension);
            ArcEntry arcEntry = arcFile.GetEntry(searchFile);
            Assert.IsNotNull(arcEntry);
            await arcEntry.TryReplaceCommuText(replacementFile);

            using (FileStream fileStream = new FileStream(outFile, FileMode.Create, FileAccess.Write))
            {
                using Stream stream = arcEntry.Open();
                stream.CopyTo(fileStream);
            }
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
