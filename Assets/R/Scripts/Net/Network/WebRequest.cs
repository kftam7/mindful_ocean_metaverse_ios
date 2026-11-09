using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

namespace Network
{
    public class WebRequest
    {
        private static CustomYieldInstruction _waitForResumeGame
            = new WaitUntil(() => !WebConnectionChecker.Instance.IsPaused);

        private static Dictionary<string, string> _apiPathDic = new();

        private List<string> _headers;
        private readonly WWWForm _formData;
        private readonly string _jsonData;
        private MockServerDebugInfo _debugInfo;
        private string _jsonFileName;

        private UnityWebRequest _request;
        private Action<WebRequest> _onSuccess;
        private Action<WebRequest> _onFailure;
        private int _maxRetries;
        private float _timeout;
        private bool _suspendGameOnFail;
        private bool _isSuccessful;

        private WebRequest(UnityWebRequest request, bool suspendGameOnFail)
        {
            _request = request;
            _suspendGameOnFail = suspendGameOnFail;
        }

        private WebRequest(UnityWebRequest request, WWWForm formData, bool suspendGameOnFail)
        {
            _request = request;
            _formData = formData;
            _suspendGameOnFail = suspendGameOnFail;
        }

        private WebRequest(UnityWebRequest request, string jsonData, bool suspendGameOnFail)
        {
            _request = request;
            _jsonData = jsonData;
            _suspendGameOnFail = suspendGameOnFail;
        }

        public static WebRequest Get(string uri, bool suspendGameOnFail = false)
        {
            return new WebRequest(UnityWebRequest.Get(uri), suspendGameOnFail);
        }

        public static WebRequest Post(string uri, WWWForm formData, bool suspendGameOnFail = false)
        {
            return new WebRequest(UnityWebRequest.Post(uri, formData), formData, suspendGameOnFail);
        }

        public static WebRequest Post(string uri, string jsonData, bool suspendGameOnFail = false)
        {
            var request = CreateJsonRequest(uri, jsonData, UnityWebRequest.kHttpVerbPOST);
            return new WebRequest(request, jsonData, suspendGameOnFail);
        }

        public static WebRequest Put(string uri, string jsonData, bool suspendGameOnFail = false)
        {
            var request = CreateJsonRequest(uri, jsonData, UnityWebRequest.kHttpVerbPUT);
            return new WebRequest(request, jsonData, suspendGameOnFail);
        }

        public static WebRequest GetTexture(string url, bool suspendGameOnFail = false)
        {
            return new WebRequest(UnityWebRequestTexture.GetTexture(url), suspendGameOnFail);
        }

        private static UnityWebRequest CreateJsonRequest(string uri, string jsonData, string method = UnityWebRequest.kHttpVerbPOST)
        {
            var request = UnityWebRequest.Put(uri, jsonData);
            request.method = method;
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");
            request.disposeUploadHandlerOnDispose = true;
            request.disposeDownloadHandlerOnDispose = true;
            return request;
        }

        public WebRequest SetHeader(string name, string value)
        {
            if (_headers == null) _headers = new List<string>();
            _headers.Add(name);
            _headers.Add(value);
            _request.SetRequestHeader(name, value);
            return this;
        }

        public WebRequest SetMaxRetries(int value)
        {
            _maxRetries = value;
            return this;
        }

        public WebRequest SetTimeout(float value)
        {
            _timeout = value;
            return this;
        }

        public WebRequest SetDebugInfo(MockServerDebugInfo debugInfo)
        {
            Assert.IsNotNull(debugInfo);
            _debugInfo = debugInfo;
            _request.SetRequestHeader("debug-delay", debugInfo.Delay.ToString(CultureInfo.InvariantCulture));
            _request.SetRequestHeader("debug-fail", ((int)debugInfo.FailType).ToString());
            _request.SetRequestHeader("debug-retry-times", debugInfo.SucceedAfterRetries.ToString());
            return this;
        }

        public WebRequest OnSuccess(Action<WebRequest> onSuccess)
        {
            _onSuccess = onSuccess;
            return this;
        }

        public WebRequest OnFailure(Action<WebRequest> onFailure)
        {
            _onFailure = onFailure;
            return this;
        }

        public IEnumerator Send(MockJsonFileNameEnum jsonFileName, string token)
        {
            _jsonFileName = jsonFileName != MockJsonFileNameEnum.None ? jsonFileName.ToString() : null;

            var retriedTimes = 0;
            var elapse = 0f;

            _request.SetRequestHeader("debug-retried-times", retriedTimes.ToString());
            if (!string.IsNullOrEmpty(_jsonFileName))
            {
                _request.SetRequestHeader("request-name", _jsonFileName);
            }

            if (!string.IsNullOrEmpty(token))
            {
                token = "Bearer " + token;
                _request.SetRequestHeader("Authorization", token);
            }

            Debug.Log($"[{GetType().Name}] send request: {_request.url}");
            _request.SendWebRequest();
            float interval = 0.05f;
            while (true)
            {
                yield return new WaitForSeconds(interval);
                elapse += interval;
                var result = _request.result;
                if (result == UnityWebRequest.Result.Success)
                {
                    _isSuccessful = true;
                    if (!string.IsNullOrEmpty(_jsonFileName))
                    {
                        ServerResponseRecorder.Instance.SaveJsonFile(jsonFileName, _request.downloadHandler.text, true);
                    }

                    _onSuccess?.Invoke(this);
                    yield break;
                }

                if (result != UnityWebRequest.Result.InProgress || elapse >= _timeout)
                {
                    if (retriedTimes < _maxRetries)
                    {
                        elapse = 0f;
                        retriedTimes += 1;
                        ResendRequest(retriedTimes, token);
                    }
                    else if (_suspendGameOnFail)
                    {
                        Debug.Log($"[{GetType().Name}] Error {_jsonFileName} {_request.url} : {_request.error}");
                        WebConnectionChecker.Instance.SuspendWebRequest(); // 通知挂起
                        yield return _waitForResumeGame;
                        elapse = 0f;
                        retriedTimes = 0;
                        ResendRequest(retriedTimes, token);
                    }
                    else
                    {
                        Debug.Log($"[{GetType().Name}] Error {_jsonFileName} {_request.url} : {_request.error}");
                        _onFailure?.Invoke(this);
                        yield break;
                    }
                }
            }
        }

        private void ResendRequest(int retriedTimes, string token)
        {
            UnityWebRequest request;
            if (_request.method == "GET")
            {
                request = UnityWebRequest.Get(_request.uri);
            }
            else if (_request.method == "POST")
            {
                if (!string.IsNullOrEmpty(_jsonData))
                {
                    request = CreateJsonRequest(_request.uri.ToString(), _jsonData);
                }
                else
                {
                    request = UnityWebRequest.Post(_request.uri, _formData);
                }
            }
            else if (_request.method == "PUT")
            {
                request = CreateJsonRequest(_request.uri.ToString(), _jsonData, UnityWebRequest.kHttpVerbPUT);
            }
            else
            {
                Debug.LogError($"[{GetType().Name}] Unknown Http method {_jsonFileName}: {_request.method}");
                return;
            }

            if (_headers != null)
                for (int i = 0; i < _headers.Count - 2; i += 2)
                    request.SetRequestHeader(_headers[i], _headers[i + 1]);

            if (_debugInfo != null)
            {
                request.SetRequestHeader("debug-delay", _debugInfo.Delay.ToString(CultureInfo.InvariantCulture));
                request.SetRequestHeader("debug-fail", ((int)_debugInfo.FailType).ToString());
                request.SetRequestHeader("debug-retry-times", _debugInfo.SucceedAfterRetries.ToString());
            }

            request.SetRequestHeader("debug-retried-times", retriedTimes.ToString());
            if (!string.IsNullOrEmpty(_jsonFileName))
            {
                request.SetRequestHeader("request-name", _jsonFileName);
            }

            if (!string.IsNullOrEmpty(token))
            {
                request.SetRequestHeader("Authorization", token);
            }

            Debug.Log($"[{GetType().Name}] resend request: {_request.url}");
            request.SendWebRequest();
            _request.Abort();
            _request = request;
        }

        public string GetText()
        {
            return _request.downloadHandler.text;
        }

        public Texture2D GetTexture()
        {
            return DownloadHandlerTexture.GetContent(_request);
        }

        public byte[] GetData()
        {
            return _request.downloadHandler.data;
        }

        public bool IsSuccessful()
        {
            return _isSuccessful;
        }

        public string GetRequestUrl()
        {
            return _request.url;
        }

        public Uri GetRequestUri()
        {
            return _request.uri;
        }

        public static string GetApiUrl(string apiPath, string baseUrl = null)
        {
            if (!_apiPathDic.TryGetValue(apiPath, out string apiUrl))
            {
                string url = string.IsNullOrEmpty(baseUrl) ? NetworkSettings.Instance.GetLogicServerUrl() : baseUrl;
                apiUrl = new Uri(new Uri(url), apiPath).ToString();
                _apiPathDic.Add(apiPath, apiUrl);
            }

            return apiUrl;
        }
    }
}