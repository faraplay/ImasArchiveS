using Imas.Archive;
using Imas.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.Drawing.Drawing2D;
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

        [DataTestMethod]
        [DataRow("hdd.arc", "ui/menu/saveLoad/saveLoadComponent_par/saveLoad", "playground/img/saveLoad.png")]
        [DataRow("hdd.arc", "ui/menu/saveLoad/saveLoadComponent_par/nameCard", "playground/img/nameCard.png")]
        [DataRow("hdd.arc", "ui/menu/mainMenu/mainMenuComponent_par/mainMenu", "playground/img/mainmenu.png")]
        [DataRow("hdd.arc", "ui/menu/topS4U/topS4UComponent_par/topS4U", "playground/img/topS4U.png")]
        [DataRow("hdd.arc", "ui/menu/produceMenu/produceMenuComponent_par/produceMenu", "playground/img/produceMenu.png")]
        [DataRow("hdd.arc", "ui/menu/option/optionComponent_par/option", "playground/img/option.png")]
        public async Task DrawPicture(string inArc, string fileName, string outPng)
        {
            using (ArcFile arcFile = new ArcFile(inArc))
            {
                using EntryStack entryStack = await arcFile.GetEntryRecursive(fileName + ".pau");
                using EntryStack imgSrcStack = await arcFile.GetEntryRecursive(fileName + ".pta");
                using Stream inStream = await entryStack.Entry.GetData();
                using Stream imgStream = await imgSrcStack.Entry.GetData();
                using Stream fontStream = new FileStream("patch/font_fromFolder.par", FileMode.Open, FileAccess.Read);
                using ParFile parFile = new ParFile(imgStream);
                ImageSource imageSource = await ImageSource.CreateImageSource(parFile);
                inStream.Seek(0x4, SeekOrigin.Begin);
                Control control = Control.Create(inStream);

                TextBox.font = new Imas.Font();
                await TextBox.font.ReadFontPar(fontStream);

                var bitmap = new Bitmap(1280, 720);
                Graphics graphics = Graphics.FromImage(bitmap);
                using Matrix matrix = new Matrix();
                control.Draw(graphics, imageSource, matrix);
                bitmap.Save(outPng, ImageFormat.Png);
            }
        }
    }
}