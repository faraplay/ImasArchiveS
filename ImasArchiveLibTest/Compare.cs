using System.IO;
using System.Security.Cryptography;

namespace ImasArchiveLibTest
{
    public static class Compare
    {
        public static bool CompareFiles(string file1, string file2)
        {
            using FileStream fileStream1 = new FileStream(file1, FileMode.Open, FileAccess.Read);
            using FileStream fileStream2 = new FileStream(file2, FileMode.Open, FileAccess.Read);
            if (fileStream1.Length != fileStream2.Length)
                return false;
            for (long i = 0; i < fileStream1.Length; i++)
            {
                if (fileStream1.ReadByte() != fileStream2.ReadByte())
                    return false;
            }
            return true;
        }

        public static bool CompareFileHashes(string file1, string file2)
        {
            using FileStream fileStream1 = new FileStream(file1, FileMode.Open, FileAccess.Read);
            using FileStream fileStream2 = new FileStream(file2, FileMode.Open, FileAccess.Read);
            if (fileStream1.Length != fileStream2.Length)
                return false;
            byte[] hash1, hash2;
            using (SHA256Managed sha = new SHA256Managed())
            {
                hash1 = sha.ComputeHash(fileStream1);
                hash2 = sha.ComputeHash(fileStream2);
            }
            for (int i = 0; (i < hash1.Length); i++)
            {
                if (hash1[i] != hash2[i])
                    return false;
            }
            return true;
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
                if (!CompareFileHashes(file.FullName, file2))
                    return false;
            }
            return true;
        }
    }
}

