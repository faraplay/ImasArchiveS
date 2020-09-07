using Imas.Records;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading.Tasks;

namespace ImasArchiveLibTest
{
    [TestClass]
    public class XmbTest
    {
        [DataTestMethod]
        [DataRow("hdd/songinfo/fut/lyrics_fut.xmb", "xml/lyrics_fut.xml")]
        [DataRow("hdd/xmb/rsc_etc.xmb", "xml/rsc_etc.xml")]
        [DataRow("hdd/xmb/rsc_chara.xmb", "xml/rsc_chara.xml")]
        [DataRow("hdd/xmb/resource_dlc.xmb", "xml/resource_dlc.xml")]
        public void SongInfoReadTest(string binName, string xmlName)
        {
            using FileStream outStream = new FileStream(xmlName, FileMode.Create, FileAccess.Write);
            Xmb xmb = new Xmb();
            using (FileStream stream = new FileStream(binName, FileMode.Open, FileAccess.Read))
            {
                xmb.ReadXmb(stream);
                xmb.WriteXml(outStream);
            }
        }

        [DataTestMethod]
        [DataRow("xml/lyrics_fut.xmb", "xml/lyrics_fut.xml")]
        [DataRow("xml/rsc_etc.xmb", "xml/rsc_etc.xml")]
        [DataRow("xml/rsc_chara.xmb", "xml/rsc_chara.xml")]
        [DataRow("xml/resource_dlc.xmb", "xml/resource_dlc.xml")]
        public async Task SongInfoWriteTest(string binName, string xmlName)
        {
            using FileStream outStream = new FileStream(binName, FileMode.Create, FileAccess.Write);
            Xmb xmb = new Xmb();
            using (FileStream stream = new FileStream(xmlName, FileMode.Open, FileAccess.Read))
            {
                xmb.ReadXml(stream);
                await xmb.WriteXmb(outStream);
            }
        }
    }
}