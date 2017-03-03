using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace MickeyUtils
{
    public enum UpdateState
    {
        CheckVerFile,//检查版本
        Updating,//更新中
        Updated,//更新结束
        NoUpdate,//没有更新
    }

    public class Updater
    {
        public Action<UpdateState> OnStateChanged;
        private string url;// = "http://192.168.2.151/";
        private string fileDirectory;
        private MyHttp http;
        private Dictionary<string, string> verDataDict;
        private string verFileName = "ver.txt";

        public float progress
        {
            get
            {
                return http.progress;
            }
        }

        public string curUpdateFile
        {
            get
            {
                return http.curDownloadFile;
            }
        }

        public bool UpdateFinish = false;

        public UpdateState state;

        public int UpdateFileCount = 0;
        public int UpdatedFileCount = 0;

        public Updater(string _url = "http://192.168.2.151/", string _fileDirectory = "Update/")
        {
            fileDirectory = _fileDirectory;
            url = _url;
            http = new MyHttp();
        }

        public void TryUpdate()
        {
            ChangeState(UpdateState.CheckVerFile);
            http.callback = DoUpdate;
            Download(verFileName);
        }

        private void DoUpdateThreadCall()
        {
            url += "publish/";
            var verData = LoadVerData();
            List<string> removeFileList = new List<string>();
            var localHashs = GetLocalFileHashs();
            foreach (var item in localHashs)
            {
                string path = item.Key.Replace(fileDirectory, "");
                if (!verData.ContainsKey(path))
                {
                    removeFileList.Add(path);
                }
            }

            List<string> downloadFileList = new List<string>();
            foreach (var item in verData)
            {
                string path = fileDirectory + item.Key;
                if (!localHashs.ContainsKey(path))
                {
                    downloadFileList.Add(item.Key);
                }
                else if (item.Value != localHashs[path])
                {
                    downloadFileList.Add(item.Key);
                }
            }

            foreach (var item in removeFileList)
            {
                File.Delete(item);
            }

            UpdateFileCount = downloadFileList.Count;
            if (downloadFileList.Count == 0)
            {
                UpdateEnd(UpdateState.NoUpdate);
                return;
            }

            http.check = DownloadFileCheck;
            http.callback = UpdateEnd;
            ChangeState(UpdateState.Updating);
            foreach (var item in downloadFileList)
            {
                Download(item);
            }
        }

        private void DoUpdate()
        {
            Thread thread = new Thread(DoUpdateThreadCall);
            thread.Start();
        }

        private void UpdateEnd()
        {
            UpdateEnd(UpdateState.Updated);
        }

        private void UpdateEnd(UpdateState us)
        {
            ChangeState(us);
            UpdateFinish = true;
        }

        private bool DownloadFileCheck(string path)
        {
            if (verDataDict[path] == Hash.GetHash(fileDirectory + path))
            {
                UpdatedFileCount++;
                return true;
            }
            return false;
        }

        private Dictionary<string, string> LoadVerData()
        {
            verDataDict = new Dictionary<string, string>();
            string verData = File.ReadAllText(fileDirectory + verFileName);
            string[] verDatas = verData.Split('\n');
            int count = verDatas.Length / 2;
            for (int i = count - 1; i >= 0; i--)
            {
                verDataDict[verDatas[i * 2]] = verDatas[i * 2 + 1];
            }
            return verDataDict;
        }

        private Dictionary<string, string> GetLocalFileHashs()
        {
            Dictionary<string, string> retHashs = new Dictionary<string, string>();
            var localHashs = Hash.GetHashs(fileDirectory);
            foreach (var item in localHashs)
            {
                retHashs[item.Key.Replace('\\', '/')] = item.Value;
            }

            return retHashs;
        }

        private void Download(string path)
        {
            DownloadInfo info = new DownloadInfo();
            info._url = url + path;
            info._path = fileDirectory + path;
            info._sPath = path;
            http.Download(info);
        }

        private void ChangeState(UpdateState us)
        {
            state = us;
            if (OnStateChanged != null)
            {
                OnStateChanged(state);
            }
        }
    }
}