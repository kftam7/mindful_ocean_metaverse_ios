using System;
using Auki.ConjureKit;
using Cysharp.Threading.Tasks;
using Dds.Communications;
using Dds.Model;
using Newtonsoft.Json;
using UnityEngine;

namespace Dds
{
    public class DdsHelper
    {
        private string _currentDomainId;
        private string _domainToken;
        private string _sessionId;
        private bool _shouldReportSession;

        private DateTime _lastTokenFetchTime;
        private DateTime _lastHeartbeatTime;
        private SharedSessionInfo _sharedSessionInfo;

        private const string SharedSessionEndpoint = "api/v1/shared-sessions";
        private const string SessionHeartbeatEndpoint = "api/v1/shared-sessions/{0}/heartbeat";
        private const int HeartbeatThreshold = 5; //in seconds
        private const int TokenThreshold = 600; //in seconds

        private readonly string _appKey;
        private readonly string _appSecret;
        private readonly IConjureKit _conjureKit;
        private readonly IDdsCommunication _ddsCommunication;

        public DdsHelper(string appKey, string appSecret, IConjureKit conjureKit)
        {
            _appKey = appKey;
            _appSecret = appSecret;
            _conjureKit = conjureKit;

            _ddsCommunication = new DdsCommunication();

            _lastHeartbeatTime = DateTime.Now.Subtract(TimeSpan.FromMilliseconds(HeartbeatThreshold*1000));
            _lastTokenFetchTime = DateTime.Now.Subtract(TimeSpan.FromMilliseconds(TokenThreshold*1000));

            _conjureKit.OnLeft += OnLeftSession;
        }

        #region Public Methods
        public void Update()
        {
            if (!_shouldReportSession || (DateTime.Now - _lastHeartbeatTime).TotalMilliseconds < (HeartbeatThreshold - 3)*1000) return;
            SendHeartbeat();
        }

        /// <summary>
        /// Method to host domain into a session or get hosted domain session
        /// </summary>
        /// <param name="domainId">domain id to host or join</param>
        /// <param name="onHosting">on hosting success callback</param>
        /// <param name="onSessionAvailable">on mapped session to the domain available callback,
        /// use this to connect the conjurekit into the passed session id</param>
        public async void DidSelectDomain(string domainId, Action onHosting, Action<string> onSessionAvailable)
        {
            await RefreshToken();

            if (string.IsNullOrEmpty(_domainToken))
            {
                //error
                return;
            }

            var sessionId = _conjureKit.GetSession().Id;

            var body = JsonConvert.SerializeObject(new SharedSessionRequest()
            {
                AppId = _appKey,
                DomainId = domainId,
                JoinSession = true,
                OnlySameApp = true,
                SessionGuid = $"{sessionId}:{domainId}",
                Threshold = HeartbeatThreshold,
                WithDomain = false
            });

            Logger.Log($"[DDS] Request Body {body}");

            var result = await _ddsCommunication.Send(SharedSessionEndpoint, DdsValues.POST, body, _domainToken);

            try
            {
                _sharedSessionInfo = JsonConvert.DeserializeObject<SharedSessionInfo>(result);
            }
            catch (Exception e)
            {
                Logger.LogError($"[DDS] Failed to parse organization session data: {e}");
                return;
            }

            Logger.Log($"[DDS] Result body {body}");

            var foundSessionId = _sharedSessionInfo.GetSessionId();

            if (!foundSessionId.Equals(sessionId))
            {
                _sessionId = foundSessionId;
                onSessionAvailable?.Invoke(foundSessionId);
                Logger.Log($"[DDS]Found session {foundSessionId}");
            }
            else
            {
                _sessionId = sessionId;
                onHosting?.Invoke();
                Logger.Log($"[DDS]Hosting session {sessionId}");
                StartHearbeat();
            }
        }

        /// <summary>
        /// For joiner/client to enable sending heartbeat to the server
        /// </summary>
        /// <param name="session">session joined</param>
        public void DidJoinSession(Session session)
        {
            Logger.Log($"[DDS] Attempt starting heartbeat: {session.Id} {_sessionId}");
            if (session.Id.Equals(_sessionId))
            {
                StartHearbeat();
            }
        }

        public void LeaveDomain()
        {
            OnLeftSession(null);
        }
        #endregion

        #region Private Methods
        private void OnLeftSession(Session session)
        {
            _shouldReportSession = false;
        }

        private void StartHearbeat()
        {
            _shouldReportSession = true;
        }

        private async void SendHeartbeat()
        {
            await RefreshToken();
            _lastHeartbeatTime = DateTime.Now;
            try
            {
                var endpoint = string.Format(SessionHeartbeatEndpoint, _sharedSessionInfo.Id);
                var response = await _ddsCommunication.Send(endpoint, DdsValues.POST, null, _domainToken);
                Logger.Log($"[DDS]Success sending heartbeat: {response}");
            } 
            catch (Exception e)
            {
                Logger.LogError($"[DDS]Error sending heartbeat: {e.Message}");
            }
        }

        private async UniTask RefreshToken()
        {
            var shouldRefresh = (DateTime.Now - _lastTokenFetchTime).TotalMilliseconds >= TokenThreshold*1000;

            if (shouldRefresh)
            {
                _lastTokenFetchTime = DateTime.Now;
                _domainToken = await _ddsCommunication.GetToken(_appKey, _appSecret);
                Logger.Log($"[DDS]Refreshed token:\n{_domainToken}");
            }
        }
        #endregion
    }
}