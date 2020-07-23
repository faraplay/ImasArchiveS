using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Imas;
using Imas.Archive;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ImasArchiveLibTest
{
    [TestClass]
    public class ParameterTest
    {
        [DataTestMethod]
        [DataRow("disc/parameter/profile.bin")]
        public void ReadProfileTest(string fileName)
        {
            List<ProfileParameter> list = new List<ProfileParameter>();
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                while (fileStream.Position < fileStream.Length)
                {
                    list.Add(ProfileParameter.Deserialise(fileStream));
                }
            }
            using (FileStream outStream = new FileStream("temp.bin", FileMode.Create, FileAccess.Write))
            {
                foreach (ProfileParameter record in list)
                {
                    record.Serialise(outStream);
                }
            }
            bool eq = Compare.CompareFiles("temp.bin", fileName);
            File.Delete("temp.bin");
            Assert.IsTrue(eq);
        }
    }
}
