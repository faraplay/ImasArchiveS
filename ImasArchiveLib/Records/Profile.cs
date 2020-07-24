using DocumentFormat.OpenXml.Spreadsheet;
using Imas.Spreadsheet;
using System.IO;

namespace Imas.Records
{
    class Profile : IRecordable
    {
        short ID;
        byte gender;
        byte age;
        byte height;
        byte weight;
        byte breast;
        byte waist;
        byte hip;
        byte month;
        byte date;
        byte bloodtype;
        byte zodiac;
        byte a2, a3, a4;

        readonly byte[] nameCode = new byte[0x10];
        readonly byte[] forename = new byte[0x20];
        readonly byte[] surname = new byte[0x20];
        readonly byte[] forename_read = new byte[0x20];
        readonly byte[] surname_read = new byte[0x20];
        readonly byte[] hobby1 = new byte[0x20];
        readonly byte[] hobby2 = new byte[0x20];
        readonly byte[] hobby3 = new byte[0x20];
        readonly byte[] title = new byte[0x40];
        readonly byte[] introduction = new byte[0x80];
        readonly byte[] memo = new byte[0x80];

        public void Deserialise(Stream inStream)
        {
            Binary binary = new Binary(inStream, true);
            ID = binary.GetInt16();
            gender = binary.GetByte();
            age = binary.GetByte();
            height = binary.GetByte();
            weight = binary.GetByte();
            breast = binary.GetByte();
            waist = binary.GetByte();
            hip = binary.GetByte();
            month = binary.GetByte();
            date = binary.GetByte();
            bloodtype = binary.GetByte();
            zodiac = binary.GetByte();
            a2 = binary.GetByte();
            a3 = binary.GetByte();
            a4 = binary.GetByte();
            inStream.Read(nameCode);
            inStream.Read(forename);
            inStream.Read(surname);
            inStream.Read(forename_read);
            inStream.Read(surname_read);
            inStream.Read(hobby1);
            inStream.Read(hobby2);
            inStream.Read(hobby3);
            inStream.Read(title);
            inStream.Read(introduction);
            inStream.Read(memo);
        }

        public void Serialise(Stream outStream)
        {
            Binary binary = new Binary(outStream, true);
            binary.PutInt16(ID);
            binary.PutByte(gender);
            binary.PutByte(age);
            binary.PutByte(height);
            binary.PutByte(weight);
            binary.PutByte(breast);
            binary.PutByte(waist);
            binary.PutByte(hip);
            binary.PutByte(month);
            binary.PutByte(date);
            binary.PutByte(bloodtype);
            binary.PutByte(zodiac);
            binary.PutByte(a2);
            binary.PutByte(a3);
            binary.PutByte(a4);
            outStream.Write(nameCode);
            outStream.Write(forename);
            outStream.Write(surname);
            outStream.Write(forename_read);
            outStream.Write(surname_read);
            outStream.Write(hobby1);
            outStream.Write(hobby2);
            outStream.Write(hobby3);
            outStream.Write(title);
            outStream.Write(introduction);
            outStream.Write(memo);
        }

        public void ReadRow(XlsxReader xlsx, Row row)
        {
            ID = xlsx.GetShort(row, "A");
            gender = xlsx.GetByte(row, "B");
            age = xlsx.GetByte(row, "C");
            height = xlsx.GetByte(row, "D");
            weight = xlsx.GetByte(row, "E");
            breast = xlsx.GetByte(row, "F");
            waist = xlsx.GetByte(row, "G");
            hip = xlsx.GetByte(row, "H");
            month = xlsx.GetByte(row, "I");
            date = xlsx.GetByte(row, "J");
            bloodtype = xlsx.GetByte(row, "K");
            zodiac = xlsx.GetByte(row, "L");
            a2 = xlsx.GetByte(row, "M");
            a3 = xlsx.GetByte(row, "N");
            a4 = xlsx.GetByte(row, "O");
            ImasEncoding.Ascii.GetBytes(xlsx.GetString(row, "P"), nameCode);
            ImasEncoding.Custom.GetBytes(xlsx.GetString(row, "Q"), forename);
            ImasEncoding.Custom.GetBytes(xlsx.GetString(row, "R"), surname);
            ImasEncoding.Custom.GetBytes(xlsx.GetString(row, "S"), forename_read);
            ImasEncoding.Custom.GetBytes(xlsx.GetString(row, "T"), surname_read);
            ImasEncoding.Custom.GetBytes(xlsx.GetString(row, "U"), hobby1);
            ImasEncoding.Custom.GetBytes(xlsx.GetString(row, "V"), hobby2);
            ImasEncoding.Custom.GetBytes(xlsx.GetString(row, "W"), hobby3);
            ImasEncoding.Custom.GetBytes(xlsx.GetString(row, "X"), title);
            ImasEncoding.Custom.GetBytes(xlsx.GetString(row, "Y"), introduction);
            ImasEncoding.Custom.GetBytes(xlsx.GetString(row, "Z"), memo);
        }

        public void WriteRow(XlsxWriter xlsx, Row row)
        {
            xlsx.AppendCell(row, "A", ID);
            xlsx.AppendCell(row, "B", gender);
            xlsx.AppendCell(row, "C", age);
            xlsx.AppendCell(row, "D", height);
            xlsx.AppendCell(row, "E", weight);
            xlsx.AppendCell(row, "F", breast);
            xlsx.AppendCell(row, "G", waist);
            xlsx.AppendCell(row, "H", hip);
            xlsx.AppendCell(row, "I", month);
            xlsx.AppendCell(row, "J", date);
            xlsx.AppendCell(row, "K", bloodtype);
            xlsx.AppendCell(row, "L", zodiac);
            xlsx.AppendCell(row, "M", a2);
            xlsx.AppendCell(row, "N", a3);
            xlsx.AppendCell(row, "O", a4);
            xlsx.AppendCell(row, "P", ImasEncoding.Ascii.GetString(nameCode));
            xlsx.AppendCell(row, "Q", ImasEncoding.Custom.GetString(forename));
            xlsx.AppendCell(row, "R", ImasEncoding.Custom.GetString(surname));
            xlsx.AppendCell(row, "S", ImasEncoding.Custom.GetString(forename_read));
            xlsx.AppendCell(row, "T", ImasEncoding.Custom.GetString(surname_read));
            xlsx.AppendCell(row, "U", ImasEncoding.Custom.GetString(hobby1));
            xlsx.AppendCell(row, "V", ImasEncoding.Custom.GetString(hobby2));
            xlsx.AppendCell(row, "W", ImasEncoding.Custom.GetString(hobby3));
            xlsx.AppendCell(row, "X", ImasEncoding.Custom.GetString(title));
            xlsx.AppendCell(row, "Y", ImasEncoding.Custom.GetString(introduction));
            xlsx.AppendCell(row, "Z", ImasEncoding.Custom.GetString(memo));
        }

        public void WriteFirstRow(XlsxWriter xlsx, Row row)
        {
            xlsx.AppendCell(row, "A", "ID");
            xlsx.AppendCell(row, "B", "Gender");
            xlsx.AppendCell(row, "C", "Age");
            xlsx.AppendCell(row, "D", "Height");
            xlsx.AppendCell(row, "E", "Weight");
            xlsx.AppendCell(row, "F", "Breast");
            xlsx.AppendCell(row, "G", "Waist");
            xlsx.AppendCell(row, "H", "Hip");
            xlsx.AppendCell(row, "I", "Month");
            xlsx.AppendCell(row, "J", "Date");
            xlsx.AppendCell(row, "K", "Blood Type");
            xlsx.AppendCell(row, "L", "Zodiac");
            xlsx.AppendCell(row, "M", "a2");
            xlsx.AppendCell(row, "N", "a3");
            xlsx.AppendCell(row, "O", "a4");
            xlsx.AppendCell(row, "P", "nameCode");
            xlsx.AppendCell(row, "Q", "forename");
            xlsx.AppendCell(row, "R", "surname");
            xlsx.AppendCell(row, "S", "forename_read");
            xlsx.AppendCell(row, "T", "surname_read");
            xlsx.AppendCell(row, "U", "hobby1");
            xlsx.AppendCell(row, "V", "hobby2");
            xlsx.AppendCell(row, "W", "hobby3");
            xlsx.AppendCell(row, "X", "title");
            xlsx.AppendCell(row, "Y", "introduction");
            xlsx.AppendCell(row, "Z", "memo");
        }
    }
}
