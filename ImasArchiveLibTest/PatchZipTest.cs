using Imas.Archive;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading.Tasks;

namespace ImasArchiveLibTest
{
    [TestClass]
    public class PatchZipTest
    {
        [DataTestMethod]
        [DataRow("patch/patch.zip")]
        public void OpenPatchTest(string filename)
        {
            using PatchZipFile patchZipFile = new PatchZipFile(filename, PatchZipMode.Read);

        }

        [DataTestMethod]
        [DataRow("patch/patch.zip", "patch/patch_edited.zip", "patch/im2nx_text.ja_jp")]
        public async Task EditPatchTest(string refFile, string filename, string newFileName)
        {
            File.Copy(refFile, filename, true);
            using PatchZipFile patchZipFile = new PatchZipFile(filename, PatchZipMode.Update);
            using FileStream fileStream = new FileStream(newFileName, FileMode.Open, FileAccess.Read);
            await patchZipFile.AddFile(fileStream, "text/im2nx_text.ja_jp");
        }

        [DataTestMethod]
        [DataRow("patch/patch_new.zip", "patch/im2nx_text.ja_jp")]
        public async Task CreatePatchTest(string filename, string newFileName)
        {
            using PatchZipFile patchZipFile = new PatchZipFile(filename, PatchZipMode.Create);
            using FileStream fileStream = new FileStream(newFileName, FileMode.Open, FileAccess.Read);
            await patchZipFile.AddFile(fileStream, "text/im2nx_text.ja_jp");
        }

        [DataTestMethod]
        [DataRow("patch/patch_add.zip", 
            "other/newfont.par", 
            "patch/text_ja_jp_tl.xlsx", 
            "patch/parameter_tl.xlsx", 
            "patch/translatedcommu.xlsx", 
            "patch/images")]
        public async Task AddToPatchTest(string filename, string newFileName, string jajpName, string parameterName, string commuName, string gtfDir)
        {
            using PatchZipFile patchZipFile = new PatchZipFile(filename, PatchZipMode.Create);
            patchZipFile.AddFile(newFileName, "im2nx_font.par");
            patchZipFile.AddJaJp(jajpName, "text/im2nx_text.ja_jp");
            patchZipFile.AddParameterFiles(parameterName);
            await patchZipFile.AddCommus(commuName);
            await patchZipFile.AddGtfs(gtfDir);
        }
    }
}
