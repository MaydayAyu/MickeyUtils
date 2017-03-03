using MickeyUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloadTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Updater up = new Updater();
            up.OnStateChanged = OnStateChanged;
            up.TryUpdate();
            while (!up.UpdateFinish)
            {
                //Console.WriteLine(up.UpdatedFileCount + "/" + up.UpdateFileCount + "  Cur:" + up.curUpdateFile + ":" + up.progress);
            }
            Console.WriteLine("下载完成");
        }

        private static void OnStateChanged(UpdateState us)
        {
            Console.WriteLine(us);
        }
    }
}