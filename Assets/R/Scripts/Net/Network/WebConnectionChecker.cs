using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public class WebConnectionChecker : Singleton<WebConnectionChecker>
    {
        public event Action ReconnectedEvent;
        public event Action LostConnectionEvent;
        public event Action SuspendWebRequestEvent;

        private bool _isConnected = true;
        private string _uri = "https://www.bing.com/";
        private bool _isInChecking;
        private bool _isSuspended;

        public bool IsConnected => _isConnected;
        public bool IsPaused => _isSuspended;

        public void Init(string uri = null)
        {
            DontDestroyOnLoad();
            if (!string.IsNullOrEmpty(uri)) _uri = uri;
        }

        public void StartChecking()
        {
            if (_isInChecking) return;
            _isInChecking = true;
            StartCoroutine(ConnectToTarget());
        }

        public void SuspendWebRequest()
        {
            _isSuspended = true;
            SuspendWebRequestEvent?.Invoke();
        }

        public void ResumeWebRequest()
        {
            _isSuspended = false;
        }

        private IEnumerator ConnectToTarget()
        {
            var request = UnityWebRequest.Get(_uri);
            request.timeout = 2;
            request.SendWebRequest();

            yield return new WaitUntil(() => request.isDone);

            if (request.result == UnityWebRequest.Result.Success)
            {
                if (!_isConnected) ReconnectedEvent?.Invoke();
                _isConnected = true;
                _isInChecking = false;
                yield break;
            }

            if (_isConnected) LostConnectionEvent?.Invoke();
            _isConnected = false;
            _isInChecking = false;
            StartChecking();
        }
    }
}