using Imas;
using Imas.Records;
using Imas.Spreadsheet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace ImasArchiveLibTest
{
    [TestClass]
    public class GTFTest
    {
        [DataTestMethod]
        [DataRow("../data/hdd4", "*.dds", "../data/gtf/hdd4_dds")]
        [DataRow("../data/hdd4", "*.gtf", "../data/gtf/hdd4_gtf")]
        public void ReadAllGtfTest(string dirName, string filter, string outDir)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(dirName);
            var files = directoryInfo.GetFiles(filter, SearchOption.AllDirectories);
            int i = 0;
            foreach (var file in files)
            {
                i++;
                try
                {
                    string relPath = file.FullName.Substring(directoryInfo.FullName.Length);
                    using FileStream inStream = file.OpenRead();
                    using Bitmap bitmap = GTF.ReadGTF(inStream);
                    string outPath = outDir + relPath[0..^4] + ".png";
                    Directory.CreateDirectory(outPath.Substring(0, outPath.LastIndexOf("\\")));
                    bitmap.Save(outPath, System.Drawing.Imaging.ImageFormat.Png);
                }
                catch (NotSupportedException)
                {
                    continue;
                }
            }
        }

        [DataTestMethod]
        //[DataRow(@"..\data\hdd4\appeal\burst_mik_par\eff_mg_mik_burst03_par\eff_mg_mik_burst03_pta\eff_firework02.gtf", "../data/gtf/firework2.png")]
        [DataRow(@"..\data\hdd4\bg3d\fes_006_par\logo_fes_006_C_00_par\logo_fes_006_C.TrueeGtf_pta\kd_fes005_logoc_d3m.gtf", "../data/gtf/logoc.png")]
        [DataRow(@"..\data\hdd4\character\acc\present\acc2_hand_047_pre_par\acc2_hand_047_pre_pta\acc2_hand047_baton_har_argb16.gtf", "../data/gtf/baton.png")]
        [DataRow(@"..\data\hdd4\effect\live\twi_eff_par\pat_par\teff_pfall01s_del_pta\ss_sakura_01.gtf", "../data/gtf/sakura.png")]
        [DataRow(@"..\data\hdd4\ui\menu\commonRankUp\commonRankUpComponent_par\idolRnkUp_pta\commonRankUp.gtf", "../data/gtf/commonRankUp.png")]
        [DataRow(@"..\data\hdd4\bg3d\commu_006_par\Bg3dModel_par\model.TrueeGtf_pta\ps_com004_books01_d1.gtf", "../data/gtf/books.png")]
        [DataRow(@"..\data\hdd4\tutorial\panel\panel_01_par\tutorial_01_1.gtf", "../data/gtf/tutorial.png")]
        public void ReadGtfTest(string fileName, string outPath)
        {
            using FileStream inStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            using Bitmap bitmap = GTF.ReadGTF(inStream);
            bitmap.Save(outPath, System.Drawing.Imaging.ImageFormat.Png);
        }

        [DataTestMethod]
        [DataRow("../data/hdd4", "*.dds", "../data/hdd4_dds.xlsx")]
        [DataRow("../data/hdd4", "*.gtf", "../data/hdd4_gtf.xlsx")]
        public void GetGtfDataTest(string dirName, string filter, string outDir)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(dirName);
            var files = directoryInfo.GetFiles(filter, SearchOption.AllDirectories);
            int i = 0;
            using XlsxWriter xlsxWriter = new XlsxWriter(outDir);
            foreach (var file in files)
            {
                i++;
                Record record = new Record("XXIssiiiiibbsissiiiiiibbsissiii");
                record[0] = file.FullName;
                record[1] = file.Name;
                record[2] = (int)file.Length;
                using (FileStream stream = file.OpenRead())
                {
                    record.Deserialise(stream);
                }
                xlsxWriter.AppendRow("gtf", record);
            }
        }
    }
}
