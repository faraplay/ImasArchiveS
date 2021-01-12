using Imas.Archive;
using Imas.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace ImasArchiveLibTest
{
    [TestClass]
    public class ControlTest
    {
        [DataTestMethod]
        [DataRow("other/topS4U.pau")]
        public void ReadPau(string inFile)
        {
            using (FileStream inStream = new FileStream(inFile, FileMode.Open, FileAccess.Read))
            {
                inStream.Seek(0x4, SeekOrigin.Begin);
                Control control = Control.Create(inStream);
                //SpriteGroup spriteGroup = SpriteGroup.CreateFromStream(inStream);
            }
        }

        [DataTestMethod]
        [DataRow("hdd.arc", "ui/menu/mainMenu/mainMenuComponent_par/mainMenu.pau")]
        [DataRow("hdd.arc", "ui/menu/autoSave/autoSaveComponent_par/autoSave.pau")]
        [DataRow("hdd.arc", "ui/menu/workPolicy/workPolicyComponent_par/workPolicy.pau")]
        [DataRow("hdd.arc", "ui/menu/option/optionComponent_par/option.pau")]
        public async Task ReadArcPau(string inArc, string fileName)
        {
            using (ArcFile arcFile = new ArcFile(inArc))
            {
                using (EntryStack entryStack = await arcFile.GetEntryRecursive(fileName))
                {
                    using Stream inStream = await entryStack.Entry.GetData();
                    inStream.Seek(0x4, SeekOrigin.Begin);
                    Control control = Control.Create(inStream);
                }
            }
        }
    }
}