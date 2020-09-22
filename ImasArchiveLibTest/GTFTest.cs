//using Imas;
//using Imas.Records;
//using Imas.Spreadsheet;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.IO;
//using System.Security.Cryptography;
//using System.Threading.Tasks;

//namespace ImasArchiveLibTest
//{
//    [TestClass]
//    public class GTFTest
//    {
//        [DataTestMethod]
//        [DataRow("../data/hdd4", "*.dds", "../data/hdd4_dds.xlsx")]
//        [DataRow("../data/hdd4", "*.gtf", "../data/hdd4_gtf.xlsx")]
//        public void GetGtfDataTest(string dirName, string filter, string outDir)
//        {
//            DirectoryInfo directoryInfo = new DirectoryInfo(dirName);
//            var files = directoryInfo.GetFiles(filter, SearchOption.AllDirectories);
//            int i = 0;
//            using XlsxWriter xlsxWriter = new XlsxWriter(outDir);
//            foreach (var file in files)
//            {
//                i++;
//                Record record = new Record("XXIssiiiiibbsissiiiiiibbsissiii");
//                record[0] = file.FullName;
//                record[1] = file.Name;
//                record[2] = (int)file.Length;
//                using (FileStream stream = file.OpenRead())
//                {
//                    record.Deserialise(stream);
//                }
//                xlsxWriter.AppendRow("gtf", record);
//            }
//        }

//        [DataTestMethod]
//        [DataRow("../data/gtf/hdd4_dds", "*.png", "../data/hdd4_dds_png.xlsx")]
//        [DataRow("../data/gtf/hdd4_gtf", "*.png", "../data/hdd4_gtf_png.xlsx")]
//        public void GetPngDataTest(string dirName, string filter, string outDir)
//        {
//            DirectoryInfo directoryInfo = new DirectoryInfo(dirName);
//            var files = directoryInfo.GetFiles(filter, SearchOption.AllDirectories);
//            int i = 0;
//            using XlsxWriter xlsxWriter = new XlsxWriter(outDir);
//            using SHA256 sha = SHA256.Create();
//            foreach (var file in files)
//            {
//                i++;
//                Record record = new Record("XXX");
//                record[0] = file.FullName;
//                record[1] = file.Name;
//                using (FileStream stream = file.OpenRead())
//                {
//                    byte[] hash = sha.ComputeHash(stream);
//                    string s = "";
//                    foreach (byte b in hash)
//                    {
//                        s += b.ToString("X2");
//                    }
//                    record[2] = s;
//                }
//                xlsxWriter.AppendRow("png", record);
//            }
//        }

//        //[DataTestMethod]
//        //[DataRow("../data/hdd4", "*.dds", "../data/gtf/hdd4_dds")]
//        //[DataRow("../data/hdd4", "*.gtf", "../data/gtf/hdd4_gtf")]
//        public async Task ReadAllGtfTest(string dirName, string filter, string outDir)
//        {
//            DirectoryInfo directoryInfo = new DirectoryInfo(dirName);
//            var files = directoryInfo.GetFiles(filter, SearchOption.AllDirectories);
//            Dictionary<string, string> hashFileName = new Dictionary<string, string>();
//            Dictionary<string, int> fileNameCount = new Dictionary<string, int>();

//            Directory.CreateDirectory(outDir);
//            using XlsxWriter xlsxWriter = new XlsxWriter(outDir + "/filenames.xlsx");
//            using SHA256 sha = SHA256.Create();

//            int i = 0;
//            foreach (var file in files)
//            {
//                i++;
//                try
//                {
//                    using FileStream inStream = file.OpenRead();
//                    using GTF gtf = GTF.ReadGTF(inStream);
//                    string outNameNoExtend = file.Name[0..^4];
//                    using MemoryStream memStream = new MemoryStream();
//                    gtf.Bitmap.Save(memStream, System.Drawing.Imaging.ImageFormat.Png);
//                    memStream.Position = 0;

//                    byte[] hash = sha.ComputeHash(memStream);
//                    string hashString = "";
//                    foreach (byte b in hash)
//                    {
//                        hashString += b.ToString("X2");
//                    }

//                    string outName;
//                    if (hashFileName.ContainsKey(hashString))
//                    {
//                        outName = hashFileName[hashString] + ".png";
//                    }
//                    else
//                    {
//                        if (fileNameCount.ContainsKey(outNameNoExtend))
//                        {
//                            fileNameCount[outNameNoExtend]++;
//                            outNameNoExtend += "(" + fileNameCount[outNameNoExtend].ToString() + ")";
//                        }
//                        else
//                        {
//                            fileNameCount.Add(outNameNoExtend, 1);
//                        }
//                        hashFileName.Add(hashString, outNameNoExtend);
//                        outName = outNameNoExtend + ".png";

//                        string outPath = outDir + "/" + outNameNoExtend + ".png";
//                        using FileStream outStream = new FileStream(outPath, FileMode.Create, FileAccess.Write);
//                        memStream.Position = 0;
//                        await memStream.CopyToAsync(outStream).ConfigureAwait(false);
//                    }

//                    Record record = new Record("XX");
//                    record[0] = file.FullName.Substring(directoryInfo.FullName.Length);
//                    record[1] = outName;
//                    xlsxWriter.AppendRow("filenames", record);
//                }
//                catch (NotSupportedException)
//                {
//                    continue;
//                }
//            }
//        }

//        [DataTestMethod]
//        [DataRow(@"..\data\hdd4\appeal\burst_mik_par\eff_mg_mik_burst03_par\eff_mg_mik_burst03_pta\eff_firework02.gtf", "../data/gtf/firework2.png")]
//        [DataRow(@"..\data\hdd4\bg3d\fes_006_par\logo_fes_006_C_00_par\logo_fes_006_C.TrueeGtf_pta\kd_fes005_logoc_d3m.gtf", "../data/gtf/logoc.png")]
//        [DataRow(@"..\data\hdd4\character\acc\present\acc2_hand_047_pre_par\acc2_hand_047_pre_pta\acc2_hand047_baton_har_argb16.gtf", "../data/gtf/baton.png")]
//        [DataRow(@"..\data\hdd4\effect\live\twi_eff_par\pat_par\teff_pfall01s_del_pta\ss_sakura_01.gtf", "../data/gtf/sakura.png")]
//        [DataRow(@"..\data\hdd4\ui\menu\commonRankUp\commonRankUpComponent_par\idolRnkUp_pta\commonRankUp.gtf", "../data/gtf/commonRankUp.png")]
//        [DataRow(@"..\data\hdd4\bg3d\commu_006_par\Bg3dModel_par\model.TrueeGtf_pta\ps_com004_books01_d1.gtf", "../data/gtf/books.png")]
//        [DataRow(@"..\data\hdd4\tutorial\panel\panel_01_par\tutorial_01_1.gtf", "../data/gtf/tutorial.png")]
//        [DataRow(@"..\data\hdd4\character\acc\acc_body\acc2_body_035_par\acc2_body_035_par\acc2_body_035_pta\acc2_body_035_sdw.gtf", "../data/gtf/acc2.png")]
//        [DataRow(@"..\data\hdd4\ui\menu\mail\mailComponent_par\mail_pta\mail_tex.gtf", "../data/gtf/mail_tex.png")]
//        [DataRow(@"..\data\hdd4\bg3d\live_006_par\Bg3dModel_par\model.TrueeGtf_pta\kd_live006_treekakiwari01_d5.gtf", "../data/gtf/tree.png")]
//        public void ReadGtfTest(string fileName, string outPath)
//        {
//            using FileStream inStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
//            using GTF gtf = GTF.ReadGTF(inStream);
//            gtf.Bitmap.Save(outPath, System.Drawing.Imaging.ImageFormat.Png);
//        }

//        [DataTestMethod]
//        [DataRow("../data/gtf/firework2.png", "../data/gtf/rebuild/firework2.gtf", 0x85)]
//        [DataRow("../data/gtf/logoc.png", "../data/gtf/rebuild/logoc.gtf", 0xA7)]
//        [DataRow("../data/gtf/baton.png", "../data/gtf/rebuild/baton.gtf", 0x82)]
//        [DataRow("../data/gtf/sakura.png", "../data/gtf/rebuild/sakura.gtf", 0xA3)]
//        [DataRow("../data/gtf/commonRankUp.png", "../data/gtf/rebuild/commonRankUp.gtf", 0x81)]
//        [DataRow("../data/gtf/books.png", "../data/gtf/rebuild/books.gtf", 0x85)]
//        [DataRow("../data/gtf/tutorial.png", "../data/gtf/rebuild/tutorial.gtf", 0x81)]
//        [DataRow("../data/gtf/acc2.png", "../data/gtf/rebuild/acc2.gtf", 0x86)]
//        [DataRow("../data/gtf/mail_tex.png", "../data/gtf/rebuild/mail_tex.gtf", 0x81)]
//        [DataRow("../data/gtf/tree.png", "../data/gtf/rebuild/tree.gtf", 0x88)]
//        public async Task WriteGtfTest(string fileName, string outGtf, int type)
//        {
//            using Bitmap bitmap = new Bitmap(fileName);
//            using FileStream outStream = new FileStream(outGtf, FileMode.Create, FileAccess.Write);
//            await GTF.WriteGTF(outStream, bitmap, type);
//        }
//    }
//}