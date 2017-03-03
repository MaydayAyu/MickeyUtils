using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace MickeyUtils
{
    public static class Hash
    {
        private static SHA256CryptoServiceProvider sha256;

        static Hash()
        {
            sha256 = new SHA256CryptoServiceProvider();
        }

        public static string GetHash(string filename)
        {
            using (Stream s = File.OpenRead(filename))
            {
                byte[] hash = sha256.ComputeHash(s);
                string shash = Convert.ToBase64String(hash);
                return shash;
            }
        }

        public static Dictionary<string, string> GetHashs(string[] filenames)
        {
            Dictionary<string, string> shashs = new Dictionary<string, string>();
            foreach (string filename in filenames)
            {
                shashs[filename] = GetHash(filename);
            }
            return shashs;
        }

        /// <summary>
        /// 获取文件夹下的所有hash值
        /// </summary>
        /// <param name="directory">文件夹路径</param>
        /// <returns></returns>
        public static Dictionary<string, string> GetHashs(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            try
            {
                string[] files = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories);
                return GetHashs(files);
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}