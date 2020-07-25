using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace ImasArchiveLibTest
{
    public static class Compare
    {
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<DLL signature>")]
        static extern int memcmp(byte[] b1, byte[] b2, long count);

        public static bool CompareFiles(string file1, string file2)
        {
            const int bufferSize = 65536;
            using FileStream fileStream1 = new FileStream(file1, FileMode.Open, FileAccess.Read);
            using FileStream fileStream2 = new FileStream(file2, FileMode.Open, FileAccess.Read);
            if (fileStream1.Length != fileStream2.Length)
                return false;
            byte[] buffer1 = new byte[bufferSize];
            byte[] buffer2 = new byte[bufferSize];
            while (true)
            {
                int count1 = fileStream1.Read(buffer1);
                int count2 = fileStream2.Read(buffer2);
                if (count1 != count2)
                    return false;
                if (count1 == 0)
                    return true;

                if (memcmp(buffer1, buffer2, count1) != 0)
                    return false;
            }
        }

        public static bool CompareDirectories(string dir1, string dir2)
        {
            DirectoryInfo directoryInfo1 = new DirectoryInfo(dir1);
            DirectoryInfo directoryInfo2 = new DirectoryInfo(dir2);

            foreach (var file in directoryInfo1.GetFiles("*", SearchOption.AllDirectories))
            {
                string relPath = file.FullName.Substring(directoryInfo1.FullName.Length);
                string file2 = directoryInfo2.FullName + relPath;
                if (!File.Exists(file2))
                    return false;
                if (!CompareFiles(file.FullName, file2))
                    return false;
            }
            return true;
        }
    }
}

