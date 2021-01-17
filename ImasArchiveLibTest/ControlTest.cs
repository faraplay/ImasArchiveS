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
        [DataRow("hdd.arc", "ui/menu/mainMenu/mainMenuComponent.par", "mainMenu")]
        [DataRow("hdd.arc", "ui/menu/autoSave/autoSaveComponent.par", "autoSave")]
        [DataRow("hdd.arc", "ui/menu/workPolicy/workPolicyComponent.par", "workPolicy")]
        [DataRow("hdd.arc", "ui/menu/option/optionComponent.par", "option")]
        public async Task ReadArcPau(string inArc, string parName, string componentName)
        {
            using (ArcFile arcFile = new ArcFile(inArc))
            {
                using (EntryStack entryStack = await arcFile.GetEntryRecursive(parName))
                {
                    using UIComponent component = await UIComponent.CreateUIComponent(await entryStack.Entry.GetData());
                    using UISubcomponent uIComponent = component[0];
                }
            }
        }

        [DataTestMethod]
        [DataRow("hdd.arc", "ui/menu/option/optionComponent.par", "playground/option.pau")]
        public async Task WritePau(string inArc, string parName, string outputName)
        {
            using (ArcFile arcFile = new ArcFile(inArc))
            {
                using (EntryStack entryStack = await arcFile.GetEntryRecursive(parName))
                {
                    using UIComponent component = await UIComponent.CreateUIComponent(await entryStack.Entry.GetData());
                    using UISubcomponent uIComponent = component[0];

                    using (FileStream outStream = new FileStream("temp.pau", FileMode.Create, FileAccess.Write))
                    {
                        uIComponent.WritePauStream(outStream);
                    }

                    Assert.IsTrue(Compare.CompareFiles("temp.pau", outputName));
                }
            }
        }

        [DataTestMethod]
        [DataRow("hdd.arc", "ui/menu/saveLoad/saveLoadComponent.par", "saveLoad", "playground/img/saveLoad.png")]
        [DataRow("hdd.arc", "ui/menu/saveLoad/saveLoadComponent.par", "nameCard", "playground/img/nameCard.png")]
        [DataRow("hdd.arc", "ui/menu/mainMenu/mainMenuComponent.par", "mainMenu", "playground/img/mainmenu.png")]
        [DataRow("hdd.arc", "ui/menu/topS4U/topS4UComponent.par", "topS4U", "playground/img/topS4U.png")]
        [DataRow("hdd.arc", "ui/menu/produceMenu/produceMenuComponent.par", "produceMenu", "playground/img/produceMenu.png")]
        [DataRow("hdd.arc", "ui/menu/option/optionComponent.par", "option", "playground/img/option.png")]
        public async Task DrawPicture(string inArc, string parName, string componentName, string outPng)
        {
            using (ArcFile arcFile = new ArcFile(inArc))
            {
                using (EntryStack entryStack = await arcFile.GetEntryRecursive(parName))
                {
                    using UIComponent component = await UIComponent.CreateUIComponent(await entryStack.Entry.GetData());
                    using UISubcomponent uIComponent = component[componentName];

                    using Stream fontStream = new FileStream("patch/font_fromFolder.par", FileMode.Open, FileAccess.Read);
                    TextBox.font = new Imas.Font();
                    await TextBox.font.ReadFontPar(fontStream);

                    var bitmap = new Bitmap(1280, 720);
                    Graphics graphics = Graphics.FromImage(bitmap);
                    uIComponent.control.Draw(graphics);
                    bitmap.Save(outPng, ImageFormat.Png);
                }
            }
        }
    }
}