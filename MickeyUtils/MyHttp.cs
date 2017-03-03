using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace MickeyUtils
{
    public struct DownloadInfo
    {
        public string _url;
        public string _path;
        public string _sPath;
    }

    public class MyHttp
    {
        /// <summary>
        /// 下载进度(百分比)
        /// </summary>
        public float progress { get; private set; }

        public bool isStop = true;
        private Thread thread;

        private Queue<DownloadInfo> _downloadList = new Queue<DownloadInfo>();
        public Action callback;

        public delegate bool fileCheck(string path);

        public fileCheck check;

        public string curDownloadFile;

        public void Download(DownloadInfo info)
        {
            _downloadList.Enqueue(info);
            if (isStop)
            {
                isStop = false;
                thread = new Thread(DownloadThread);
                thread.IsBackground = true;
                thread.Start();
            }
        }

        private void DownloadThread()
        {
            while (!isStop && _downloadList.Count > 0)
            {
                DownloadInfo info = _downloadList.Dequeue();
                string dirPath = Path.GetDirectoryName(info._path);
                if (!string.IsNullOrEmpty(dirPath) && !Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);
                FileStream fileStream = null;
                if (File.Exists(info._path))
                    File.Delete(info._path);
                fileStream = File.Create(info._path);
                long fileLength = fileStream.Length;
                curDownloadFile = info._path;
                float totalLength = GetLength(info._url);
                if (fileLength < totalLength)
                {
                    HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(info._url);
                    request.AddRange((int)fileLength);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    fileStream.Seek(fileLength, SeekOrigin.Begin);
                    Stream httpStream = response.GetResponseStream();
                    byte[] buffer = new byte[1024];
                    int length = httpStream.Read(buffer, 0, buffer.Length);
                    while (length > 0)
                    {
                        if (isStop)
                            break;
                        fileStream.Write(buffer, 0, length);
                        fileLength += length;
                        progress = fileLength / totalLength * 100;
                        fileStream.Flush();
                        length = httpStream.Read(buffer, 0, buffer.Length);
                    }
                    httpStream.Close();
                    httpStream.Dispose();
                }
                else
                    progress = fileLength / totalLength * 100;
                fileStream.Close();
                fileStream.Dispose();
                if (check != null)
                {
                    if (!check(info._sPath))
                    {
                        _downloadList.Enqueue(info);
                    }
                }
            }

            Close();
            if (callback != null)
            {
                callback();
            }
        }

        /// <summary>
        /// 关闭线程
        /// </summary>
        public void Close()
        {
            thread = null;
            isStop = true;
            curDownloadFile = "";
        }

        private long GetLength(string _fileUrl)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(_fileUrl);
            request.Method = "HEAD";
            HttpWebResponse res = (HttpWebResponse)request.GetResponse();
            return res.ContentLength;
        }
    }
}