using Imas;
using Imas.Records;
using Imas.Spreadsheet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace ImasArchiveLibTest
{
    [TestClass]
    public class GTFTest
    {
        //[DataTestMethod]
        //[DataRow("../data/hdd4", "*.dds", "../data/hdd4_dds.xlsx")]
        //[DataRow("../data/hdd4", "*.gtf", "../data/hdd4_gtf.xlsx")]
        //public void GetGtfDataTest(string dirName, string filter, string outDir)
        //{
        //    DirectoryInfo directoryInfo = new DirectoryInfo(dirName);
        //    var files = directoryInfo.GetFiles(filter, SearchOption.AllDirectories);
        //    int i = 0;
        //    using XlsxWriter xlsxWriter = new XlsxWriter(outDir);
        //    foreach (var file in files)
        //    {
        //        i++;
        //        Record record = new Record("XXIssiiiiibbsissiiiiiibbsissiii");
        //        record[0] = file.FullName;
        //        record[1] = file.Name;
        //        record[2] = (int)file.Length;
        //        using (FileStream stream = file.OpenRead())
        //        {
        //            record.Deserialise(stream);
        //        }
        //        xlsxWriter.AppendRow("gtf", record);
        //    }
        //}

        //[DataTestMethod]
        //[DataRow("../data/gtf/hdd4_dds", "*.png", "../data/hdd4_dds_png.xlsx")]
        //[DataRow("../data/gtf/hdd4_gtf", "*.png", "../data/hdd4_gtf_png.xlsx")]
        //public void GetPngDataTest(string dirName, string filter, string outDir)
        //{
        //    DirectoryInfo directoryInfo = new DirectoryInfo(dirName);
        //    var files = directoryInfo.GetFiles(filter, SearchOption.AllDirectories);
        //    int i = 0;
        //    using XlsxWriter xlsxWriter = new XlsxWriter(outDir);
        //    using SHA256 sha = SHA256.Create();
        //    foreach (var file in files)
        //    {
        //        i++;
        //        Record record = new Record("XXX");
        //        record[0] = file.FullName;
        //        record[1] = file.Name;
        //        using (FileStream stream = file.OpenRead())
        //        {
        //            byte[] hash = sha.ComputeHash(stream);
        //            string s = "";
        //            foreach (byte b in hash)
        //            {
        //                s += b.ToString("X2");
        //            }
        //            record[2] = s;
        //        }
        //        xlsxWriter.AppendRow("png", record);
        //    }
        //}

        ////[DataTestMethod]
        ////[DataRow("../data/hdd4", "*.dds", "../data/gtf/hdd4_dds")]
        ////[DataRow("../data/hdd4", "*.gtf", "../data/gtf/hdd4_gtf")]
        //public async Task ReadAllGtfTest(string dirName, string filter, string outDir)
        //{
        //    DirectoryInfo directoryInfo = new DirectoryInfo(dirName);
        //    var files = directoryInfo.GetFiles(filter, SearchOption.AllDirectories);
        //    Dictionary<string, string> hashFileName = new Dictionary<string, string>();
        //    Dictionary<string, int> fileNameCount = new Dictionary<string, int>();

        //    Directory.CreateDirectory(outDir);
        //    using XlsxWriter xlsxWriter = new XlsxWriter(outDir + "/filenames.xlsx");
        //    using SHA256 sha = SHA256.Create();

        //    int i = 0;
        //    foreach (var file in files)
        //    {
        //        i++;
        //        try
        //        {
        //            using FileStream inStream = file.OpenRead();
        //            using GTF gtf = GTF.ReadGTF(inStream);
        //            string outNameNoExtend = file.Name[0..^4];
        //            using MemoryStream memStream = new MemoryStream();
        //            gtf.Bitmap.Save(memStream, System.Drawing.Imaging.ImageFormat.Png);
        //            memStream.Position = 0;

        //            byte[] hash = sha.ComputeHash(memStream);
        //            string hashString = "";
        //            foreach (byte b in hash)
        //            {
        //                hashString += b.ToString("X2");
        //            }

        //            string outName;
        //            if (hashFileName.ContainsKey(hashString))
        //            {
        //                outName = hashFileName[hashString] + ".png";
        //            }
        //            else
        //            {
        //                if (fileNameCount.ContainsKey(outNameNoExtend))
        //                {
        //                    fileNameCount[outNameNoExtend]++;
        //                    outNameNoExtend += "(" + fileNameCount[outNameNoExtend].ToString() + ")";
        //                }
        //                else
        //                {
        //                    fileNameCount.Add(outNameNoExtend, 1);
        //                }
        //                hashFileName.Add(hashString, outNameNoExtend);
        //                outName = outNameNoExtend + ".png";

        //                string outPath = outDir + "/" + outNameNoExtend + ".png";
        //                using FileStream outStream = new FileStream(outPath, FileMode.Create, FileAccess.Write);
        //                memStream.Position = 0;
        //                await memStream.CopyToAsync(outStream).ConfigureAwait(false);
        //            }

        //            Record record = new Record("XX");
        //            record[0] = file.FullName.Substring(directoryInfo.FullName.Length);
        //            record[1] = outName;
        //            xlsxWriter.AppendRow("filenames", record);
        //        }
        //        catch (NotSupportedException)
        //        {
        //            continue;
        //        }
        //    }
        //}

        [DataTestMethod]
        [DataRow(@"gtf\eff_firework02.gtf", "gtf/firework2.png")]
        [DataRow(@"gtf\kd_fes005_logoc_d3m.gtf", "gtf/logoc.png")]
        [DataRow(@"gtf\acc2_hand047_baton_har_argb16.gtf", "gtf/baton.png")]
        [DataRow(@"gtf\ss_sakura_01.gtf", "gtf/sakura.png")]
        [DataRow(@"gtf\commonRankUp.gtf", "gtf/commonRankUp.png")]
        [DataRow(@"gtf\ps_com004_books01_d1.gtf", "gtf/books.png")]
        [DataRow(@"gtf\tutorial_01_1.gtf", "gtf/tutorial.png")]
        [DataRow(@"gtf\acc2_body_035_sdw.gtf", "gtf/acc2.png")]
        [DataRow(@"gtf\mail_tex.gtf", "gtf/mail_tex.png")]
        [DataRow(@"gtf\kd_live006_treekakiwari01_d5.gtf", "gtf/tree.png")]
        public void ReadGtfTest(string fileName, string outPath)
        {
            using FileStream inStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            using GTF gtf = GTF.ReadGTF(inStream);
            gtf.Bitmap.Save(outPath, System.Drawing.Imaging.ImageFormat.Png);
        }

        [DataTestMethod]
        [DataRow("gtf/ideal/firework2.png", "gtf/rebuild/firework2.gtf", 0x85)]
        [DataRow("gtf/ideal/logoc.png", "gtf/rebuild/logoc.gtf", 0xA7)]
        [DataRow("gtf/ideal/baton.png", "gtf/rebuild/baton.gtf", 0x82)]
        [DataRow("gtf/ideal/sakura.png", "gtf/rebuild/sakura.gtf", 0xA3)]
        [DataRow("gtf/ideal/commonRankUp.png", "gtf/rebuild/commonRankUp.gtf", 0x81)]
        [DataRow("gtf/ideal/books.png", "gtf/rebuild/books.gtf", 0x85)]
        [DataRow("gtf/ideal/tutorial.png", "gtf/rebuild/tutorial.gtf", 0x81)]
        [DataRow("gtf/ideal/acc2.png", "gtf/rebuild/acc2.gtf", 0x86)]
        [DataRow("gtf/ideal/mail_tex.png", "gtf/rebuild/mail_tex.gtf", 0x81)]
        [DataRow("gtf/ideal/tree.png", "gtf/rebuild/tree.gtf", 0x88)]
        public async Task WriteGtfTest(string fileName, string outGtf, int type)
        {
            using Bitmap bitmap = new Bitmap(fileName);
            using FileStream outStream = new FileStream(outGtf, FileMode.Create, FileAccess.Write);
            await GTF.WriteGTF(outStream, bitmap, type);
        }

        [DataTestMethod]
        [DataRow("gtf/produceMenuIcon.png", "gtf/indexed/produceMenuIcon.png")]
        [DataRow("gtf/sakura.png", "gtf/indexed/sakura.png")]
        [DataRow("gtf/bg2d_0065.png", "gtf/indexed/bg2d_0065.png")]
        public void MyQuantiserTest(string fileName, string outName)
        {
            using Bitmap bitmap = new Bitmap(fileName);
            BitmapData bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);
            IntPtr bitmapPtr = bitmapData.Scan0;
            int[] pixelDataArray = new int[bitmap.Width * bitmap.Height];
            Marshal.Copy(bitmapPtr, pixelDataArray, 0, bitmap.Width * bitmap.Height);
            bitmap.UnlockBits(bitmapData);
            byte[] indexedData;
            uint[] palette;

            (indexedData, palette) = WuQuantizer.QuantizeImage(pixelDataArray, 256);

            IntPtr newPtr = Marshal.AllocHGlobal(bitmap.Width * bitmap.Height);
            Marshal.Copy(indexedData, 0, newPtr, bitmap.Width * bitmap.Height);
            Bitmap newBitmap = new Bitmap(bitmap.Width, bitmap.Height, bitmap.Width, PixelFormat.Format8bppIndexed, newPtr);
            ColorPalette colorPalette = newBitmap.Palette;
            for (int i = 0; i < 256; i++)
            {
                colorPalette.Entries[i] = (i < palette.Length) ? Color.FromArgb((int)palette[i]) : Color.Transparent;
            }
            newBitmap.Palette = colorPalette;
            newBitmap.Save(outName, ImageFormat.Png);
            Marshal.FreeHGlobal(newPtr);
        }
    }
}