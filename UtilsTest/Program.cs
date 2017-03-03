using MickeyUtils;
using System.IO;
using System.Text;

namespace VerHashMaker
{
    internal class Program
    {
        private static string verHashPath = "./publish/";
        private static string verFilePath = "ver.txt";

        private static void Main(string[] args)
        {
            if (args.Length == 2)
            {
                verHashPath = args[0];
                verFilePath = args[1];
            }
            var d = Hash.GetHashs(verHashPath);
            string data = "";
            foreach (var item in d)
            {
                data += item.Key.Replace('\\', '/').Replace(verHashPath, "") + '\n';
                data += item.Value + '\n';
            }
            File.WriteAllText(verFilePath, data, Encoding.UTF8);
        }
    }
}