using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

namespace Network
{
    public class MockServer : Singleton<MockServer>
    {
        public const string MockServerBaseUrl = "http://127.0.0.1";
        public const string DefaultImage = "DefaultImage.png";
        public const string DownloadFolderName = "MockServer/Files";
        public const string ResponseFolderName = "MockServer/Responses";

        public string DownloadPath { get; private set; }
        public string ResponsePath { get; private set; }
        public string PortNumber = "2021";

        private HttpListener _apiListener;
        private HttpListener _downloadListener;
        private MockServerResponder _mockResponder;
        private bool _disconnect;

        public void Init()
        {
            DontDestroyOnLoad();
            PortNumber = Random.Range(2021, 22021).ToString();
            DownloadPath = Path.Combine(Application.streamingAssetsPath, DownloadFolderName);
            ResponsePath = Path.Combine(Application.streamingAssetsPath, ResponseFolderName);
            StartApiServer();
            StartDownloadServer();
        }

        private void OnDestroy()
        {
            if (_apiListener is { IsListening: true }) _apiListener.Close();
            if (_downloadListener is { IsListening: true }) _downloadListener.Close();
        }

        public void Disconnect()
        {
            _disconnect = true;
        }

        public void Reconnect()
        {
            _disconnect = false;
        }

        public string GetMockServerUrl()
        {
            return MockServerBaseUrl + ":" + PortNumber;
        }

        /// <summary>
        /// Used for debugging.
        /// </summary>
        public void ReloadAllResponseData()
        {
            _mockResponder.LoadAllResponseData();
        }


        private void StartApiServer()
        {
            _mockResponder = new MockServerResponder();

            _apiListener = new HttpListener();
            _apiListener.Prefixes.Add(GetMockServerUrl() + "/");
            _apiListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            _apiListener.Start();

            StartCoroutine(StartApiListener());
            StartCoroutine(SendTestRequest());
            Debug.Log($"[{GetType().Name}] Mock Server started: {GetMockServerUrl()}");
        }

        private IEnumerator StartApiListener()
        {
            while (true)
            {
                if (_disconnect) yield return null;
                _apiListener.BeginGetContext(r => _mockResponder.Respond(r), _apiListener);
                yield return null;
            }
        }

        private IEnumerator SendTestRequest()
        {
            UnityWebRequest www = UnityWebRequest.Get(GetMockServerUrl());
            www.SendWebRequest();
            yield return new WaitUntil(() => www.isDone);
            Debug.Log($"[{GetType().Name}] Test Mock Server: {www.result}");
            Debug.Log(www.downloadHandler.text);
        }

        private void StartDownloadServer()
        {
            _downloadListener = new HttpListener();
            _downloadListener.Prefixes.Add(Path.Combine(GetMockServerUrl() + "/", "download/"));
            _apiListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            _downloadListener.Start();
            StartCoroutine(StartDownloadListener());
        }

        private IEnumerator StartDownloadListener()
        {
            while (true)
            {
                if (_disconnect) yield return null;
                _downloadListener.BeginGetContext(HandleDownload, _downloadListener);
                yield return null;
            }
        }

        private void HandleDownload(IAsyncResult result)
        {
            var listener = result.AsyncState as HttpListener;
            Assert.IsNotNull(listener);
            HttpListenerContext context = listener.EndGetContext(result);

            var fileName = context.Request.QueryString["file"];
            string filePath = Path.Combine(DownloadPath, fileName);
            byte[] buffer;

            if (Application.platform == RuntimePlatform.Android)
            {
                UnityWebRequest www = UnityWebRequest.Get(filePath);
                www.SendWebRequest();
                while (!www.isDone) { }

                buffer = www.downloadHandler.data;
            }
            else
            {
                if (!File.Exists(filePath)) filePath = Path.Combine(DownloadPath, DefaultImage);

                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, (int)fs.Length);
                    fs.Close();
                }
            }

            SendResponse(context, buffer);
        }

        private async Task SendResponse(HttpListenerContext context, byte[] buffer)
        {
            await Task.Delay(TimeSpan.FromSeconds(0.2f));
            context.Response.StatusCode = 200;
            context.Response.ContentType = "image/png";
            context.Response.ContentLength64 = buffer.Length;
            Stream output = context.Response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
            context.Response.Close();
        }
    }
}