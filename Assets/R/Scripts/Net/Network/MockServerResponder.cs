using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

namespace Network
{
    public partial class MockServerResponder
    {
        private readonly Dictionary<string, string> _successResponseDic = new();

        private Dictionary<string, MethodInfo> _webServiceDic;

        public MockServerResponder()
        {
            LoadAllResponseData();
            ConfigWebServices();
        }

        public void LoadAllResponseData()
        {
            _successResponseDic.Clear();
            foreach (var fileName in Enum.GetNames(typeof(MockJsonFileNameEnum)))
            {
                _successResponseDic.Add(fileName, LoadFile(fileName + "Success.json"));
            }
        }

        private string LoadFile(string fileName)
        {
            string filePath = Path.Combine(MockServer.Instance.ResponsePath, fileName);
            if (Application.platform == RuntimePlatform.Android)
            {
                UnityWebRequest www = UnityWebRequest.Get(filePath);
                www.SendWebRequest();
                while (!www.isDone) { }

                if (www.result == UnityWebRequest.Result.ProtocolError || www.result == UnityWebRequest.Result.ConnectionError)
                    return "";
                return www.downloadHandler.text;
            }

            return !File.Exists(filePath) ? "" : File.ReadAllText(filePath);
        }

        private void ConfigWebServices()
        {
            _webServiceDic = new Dictionary<string, MethodInfo>();
            Type type = GetType();
            foreach (var fileName in Enum.GetNames(typeof(MockJsonFileNameEnum)))
            {
                _webServiceDic.Add(fileName, type.GetMethod($"Respond{fileName}", BindingFlags.Instance | BindingFlags.NonPublic));
            }
        }

        public void Respond(IAsyncResult result)
        {
            var listener = result.AsyncState as HttpListener;
            Assert.IsNotNull(listener);

            HttpListenerContext context = listener.EndGetContext(result);
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            string apiPath = request.Url.AbsolutePath.Substring(1); // remove "/"

            if (!string.IsNullOrEmpty(apiPath))
            {
                Debug.Log($"[MockServer] {MockServer.Instance.GetMockServerUrl()}/{apiPath}");
            }

            float debugDelay = 0.2f;
            MockServerFailTypeEnum debugFailType = MockServerFailTypeEnum.None;
            int debugRetryTimes = 0;
            int debugRetriedTimes = 0;
            string requestName = "";

            NameValueCollection headers = request.Headers;
            requestName = headers.Get("request-name");

            if (headers.Get("debug-delay") != null)
            {
                debugDelay = Mathf.Max(debugDelay, float.Parse(headers.Get("debug-delay")));
                debugFailType = (MockServerFailTypeEnum)int.Parse(headers.Get("debug-fail"));
                debugRetryTimes = int.Parse(headers.Get("debug-retry-times"));
                debugRetriedTimes = int.Parse(headers.Get("debug-retried-times"));
                Debug.Log($"[MockServer] DebugInfo -- delay:{debugDelay}, failType:{debugFailType}, retryTimes:{debugRetryTimes}, retriedTimes:{debugRetriedTimes}");
            }

            bool keepRetring = debugRetryTimes <= 0 || debugRetriedTimes < debugRetryTimes;

            if ((debugFailType == MockServerFailTypeEnum.HttpError || debugFailType == MockServerFailTypeEnum.NetworkError)
                && keepRetring)
            {
#pragma warning disable 4014
                SendResponse(response, "500 Internal Server Error", 500, debugDelay);
            }
            else if (debugFailType == MockServerFailTypeEnum.EmptyData && keepRetring)
            {
                SendResponse(response, "", 200, debugDelay);
            }
            else if (debugFailType == MockServerFailTypeEnum.ResultRejected && keepRetring)
            {
                SendResponse(response, "{\"success\": false}", 200, debugDelay);
            }
            else if (debugFailType == MockServerFailTypeEnum.Timeout && keepRetring)
            {
                // do nothing
            }
            else if (debugFailType == MockServerFailTypeEnum.None || !keepRetring)
            {
                if (string.IsNullOrEmpty(apiPath))
                    SendResponse(response, "[MockServer] Welcome to Mock Server! ", 200, debugDelay);
                else if (_webServiceDic.ContainsKey(requestName) && _webServiceDic[requestName] != null)
                    SendResponse(response, (string)_webServiceDic[requestName].Invoke(this, new object[] { request }), 200, debugDelay);
                else
                    SendResponse(response, "404 not found", 404, debugDelay);
#pragma warning restore 4014
            }
        }

        private async Task SendResponse(HttpListenerResponse response, string data, int stateCode, float delay)
        {
            await Task.Delay(TimeSpan.FromSeconds(delay));
            response.StatusCode = stateCode;
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(data);
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.Close();
        }

        private Dictionary<string, string> ParseForm(HttpListenerRequest request)
        {
            string text;

            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                text = reader.ReadToEnd();
            }

            Dictionary<string, string> result = text.Split('&')
                .ToDictionary(c => c.Split('=')[0], c => Uri.UnescapeDataString(c.Split('=')[1]));

            return result;
        }

        private void PrintGetPostInfo(HttpListenerRequest request)
        {
            Debug.Log("--------- request.QueryString");
            foreach (string key in request.QueryString) Debug.Log($"{key}: {request.QueryString[key]}");
            Debug.Log("--------- request.InputStream");
            string text;
            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                text = reader.ReadToEnd();
            }

            Debug.Log($"{Uri.UnescapeDataString(text)}");
            Debug.Log("-------------------");
        }
    }
}