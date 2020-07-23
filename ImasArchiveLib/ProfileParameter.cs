using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace Imas
{
    class ProfileParameter
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
        byte a1, a2, a3, a4;

        byte[] nameCode;
        byte[] forename;
        byte[] surname;
        byte[] forename_read;
        byte[] surname_read;
        byte[] hobby1;
        byte[] hobby2;
        byte[] hobby3;
        byte[] title;
        byte[] introduction;
        byte[] memo;

        public static ProfileParameter Deserialise(Stream inStream)
        {
            Binary binary = new Binary(inStream, true);
            var result = new ProfileParameter
            {
                ID = binary.GetInt16(),
                gender = binary.GetByte(),
                age = binary.GetByte(),
                height = binary.GetByte(),
                weight = binary.GetByte(),
                breast = binary.GetByte(),
                waist = binary.GetByte(),
                hip = binary.GetByte(),
                month = binary.GetByte(),
                date = binary.GetByte(),
                bloodtype = binary.GetByte(),
                a1 = binary.GetByte(),
                a2 = binary.GetByte(),
                a3 = binary.GetByte(),
                a4 = binary.GetByte(),
                nameCode = new byte[0x10],
                forename = new byte[0x20],
                surname = new byte[0x20],
                forename_read = new byte[0x20],
                surname_read = new byte[0x20],
                hobby1 = new byte[0x20],
                hobby2 = new byte[0x20],
                hobby3 = new byte[0x20],
                title = new byte[0x40],
                introduction = new byte[0x80],
                memo = new byte[0x80]
            };
            inStream.Read(result.nameCode);
            inStream.Read(result.forename);
            inStream.Read(result.surname);
            inStream.Read(result.forename_read);
            inStream.Read(result.surname_read);
            inStream.Read(result.hobby1);
            inStream.Read(result.hobby2);
            inStream.Read(result.hobby3);
            inStream.Read(result.title);
            inStream.Read(result.introduction);
            inStream.Read(result.memo);
            return result;
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
            binary.PutByte(a1);
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
    }
}
