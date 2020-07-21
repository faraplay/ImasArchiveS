using Imas;
using Imas.Archive;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading.Tasks;

namespace ImasArchiveLibTest
{
    public class CommuToXlsxTest
    {
        [DataTestMethod]
        [DataRow("other/commus.xlsx", "hdd", "other/commulist.txt")]
        public async Task WriteXlsxTest(string xlsxName, string arcName, string commuList)
        {
            using CommuToXlsx commu = new CommuToXlsx(xlsxName, FileMode.Create);
            using StreamReader streamReader = new StreamReader(commuList);
            using ArcFile arcFile = new ArcFile(arcName);
            while (!streamReader.EndOfStream)
            {
                string fileName = streamReader.ReadLine();
                using EntryStack entryStack = await arcFile.GetEntryRecursive(fileName);
                ContainerEntry entry = entryStack.Entry;
                using Stream memStream = await entry.GetData();
                commu.AddCommuFromBin(memStream, fileName);
                commu.WriteCommuToXlsx();
            }
        }
    }
}
