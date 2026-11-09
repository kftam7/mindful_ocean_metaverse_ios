using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Network
{
    public class ServerResponseRecorder : Singleton<ServerResponseRecorder>
    {
        public bool Enable = true;

        public void Init()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void SaveJsonFile(MockJsonFileNameEnum name, string jsonData, bool success)
        {
            if (!Enable
                || NetworkSettings.Instance.IsMock
                || !Application.isEditor) return;
            string pattern = @"http[^\""]+?/([^/]+\.(png|jpg|jpeg))";
            string replacement = @"http://127.0.0.1:2021/download?file=$1";
            jsonData = Regex.Replace(jsonData, pattern, replacement);

            string state = success ? "Success" : "Fail";
            string fileName = $"{name}{state}.json";

            if (!Directory.Exists(MockServer.Instance.ResponsePath))
            {
                Directory.CreateDirectory(MockServer.Instance.ResponsePath);
            }

            Debug.Log($"[{GetType().Name}] SaveJsonFile: {fileName}");
            var fileInfo = new FileInfo(MockServer.Instance.ResponsePath + "//" + fileName);
            StreamWriter sw = fileInfo.CreateText();
            sw.WriteLine(jsonData);
            sw.Close();
            sw.Dispose();
        }

        public void SaveMockImageFile(string fileName, Texture2D tex)
        {
            if (!Enable
                || NetworkSettings.Instance.IsMock
                || !Application.isEditor) return;

            Debug.Log($"[{GetType().Name}] SaveMockImageFile: {fileName}");
            string ext = Path.GetExtension(fileName).ToLower();

            if (!Directory.Exists(MockServer.Instance.DownloadPath))
            {
                Directory.CreateDirectory(MockServer.Instance.DownloadPath);
            }

            byte[] bytes = ext == "jpg" ? tex.EncodeToJPG() : tex.EncodeToPNG();
            File.WriteAllBytes(Path.Combine(MockServer.Instance.DownloadPath, fileName), bytes);
        }
    }
}