using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Imas.UI
{
    public static class Base64
    {
        public static string ToBase64(object obj)
        {
            using MemoryStream memStream = new MemoryStream();
            Serialiser.Serialise(new Binary(memStream, true), obj);
            return Convert.ToBase64String(memStream.ToArray());
        }

        public static object FromBase64(string data, Type type)
        {
            byte[] bytes = Convert.FromBase64String(data);
            using MemoryStream memoryStream = new MemoryStream(bytes);
            return Deserialiser.Deserialise(new Binary(memoryStream, true), type);
        }
    }
}
