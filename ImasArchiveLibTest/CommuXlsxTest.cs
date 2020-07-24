using Imas;
using Imas.Archive;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading.Tasks;

namespace ImasArchiveLibTest
{
    [TestClass]
    public class CommuXlsxTest
    {
        [DataTestMethod]
        [DataRow("other/commus.xlsx", "hdd", "other/commulist.txt")]
        public async Task WriteXlsxTest(string xlsxName, string arcName, string commuList)
        {
            using CommuToXlsx commu = new CommuToXlsx(xlsxName);
            using StreamReader streamReader = new StreamReader(commuList);
            using ArcFile arcFile = new ArcFile(arcName);
            while (!streamReader.EndOfStream)
            {
                string fileName = streamReader.ReadLine();
                using EntryStack entryStack = await arcFile.GetEntryRecursive(fileName);
                ContainerEntry entry = entryStack.Entry;
                using Stream memStream = await entry.GetData();
                commu.GetAndWriteCommu(memStream, fileName);
            }
        }

        [DataTestMethod]
        [DataRow("../data/editedcommus.xlsx", "../data/tlcommus.zip")]
        [DataRow("../data/hddcommus.xlsx", "../data/nocommus.zip")]
        public async Task WriteCommuTest(string xlsxName, string zipName)
        {
            using CommuFromXlsx commu = new CommuFromXlsx(xlsxName, zipName);
            await commu.GetAndWriteAllCommus();
        }
    }
}
