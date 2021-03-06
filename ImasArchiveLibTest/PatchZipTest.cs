﻿using Imas;
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
        [DataRow("patch/patch.zip", "patch/patch_edited.zip", "patch/im2nx_font.par")]
        public async Task EditPatchTest(string refFile, string filename, string newFileName)
        {
            File.Copy(refFile, filename, true);
            using PatchZipFile patchZipFile = new PatchZipFile(filename, PatchZipMode.Update);
            using FileStream fileStream = new FileStream(newFileName, FileMode.Open, FileAccess.Read);
            await patchZipFile.AddFile(fileStream, "im2nx_font.par");
        }

        [DataTestMethod]
        [DataRow("patch/patch_new.zip", "patch/im2nx_font.par")]
        public async Task CreatePatchTest(string filename, string newFileName)
        {
            using PatchZipFile patchZipFile = new PatchZipFile(filename, PatchZipMode.Create);
            using FileStream fileStream = new FileStream(newFileName, FileMode.Open, FileAccess.Read);
            await patchZipFile.AddFile(fileStream, "im2nx_font.par");
        }

        [DataTestMethod]
        [DataRow("patch/patch_add.zip",
            "other/newfont.par",
            "patch/parameter_tl.xlsx",
            "patch/lyrics",
            "patch/translation.xlsx",
            "patch/images")]
        public async Task AddToPatchTest(
            string filename,
            string newFileName,
            string parameterName,
            string lyricDir,
            string commuName,
            string gtfDir)
        {
            using PatchZipFile patchZipFile = new PatchZipFile(filename, PatchZipMode.Create);
            patchZipFile.AddFile(newFileName, "im2nx_font.par");
            patchZipFile.AddParameterFiles(parameterName);
            await patchZipFile.AddLyrics(lyricDir);
            await patchZipFile.AddCommus(commuName, null, new System.Progress<ProgressData>(data => System.Console.WriteLine(data.ToString())));
            await patchZipFile.AddGtfs(gtfDir);
        }
    }
}