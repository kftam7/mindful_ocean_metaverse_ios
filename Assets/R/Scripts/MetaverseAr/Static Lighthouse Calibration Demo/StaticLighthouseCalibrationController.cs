using System;
using Auki.ConjureKit;
using Auki.Util;
using UnityEngine;

namespace DomainSessionDemo
{
    internal interface IServerCommunication
    {
        void Send(string endpoint, string method, string json, Action<string> onSuccess, Action<string> onFailure);
    }

    /// <summary>
    /// StaticLighthouseCalibrationController handles the logic of hosting or joining sessions associated with a domain 
    /// </summary>
    internal class StaticLighthouseCalibrationController
    {
        private readonly IConjureKit _conjureKit;
        private readonly IServerCommunication _serverCommunication;
        private string _targetDomainId;
        private string _targetSessionId;
        private bool _shouldReportSession;
        private const int SessionReportInterval = 30000;
        private DateTime _lastReportTime;
        private readonly Action<string> _showToast;

        public StaticLighthouseCalibrationController(
            string appKey, 
            string appSecret, 
            IConjureKit conjureKit, 
            Action<string> showToast)
        {
            _serverCommunication = new DSMSCommunication(appKey, appSecret);
            _conjureKit = conjureKit;
            _conjureKit.OnLeft += OnSessionLeft;
            _lastReportTime = DateTime.Now.Subtract(TimeSpan.FromMilliseconds(SessionReportInterval));
            _showToast = showToast;
        }

        /// <summary>
        /// Tracks time between session id reports to the DSMS.
        /// Should be invoked frequently (e.g. every frame).
        /// </summary>
        public void Update()
        {
            if (!_shouldReportSession || (DateTime.Now - _lastReportTime).TotalMilliseconds <= SessionReportInterval) return;
            _lastReportTime = ReportCurrentSession(_serverCommunication, _targetDomainId, _targetSessionId);
        }

        /// <summary>
        /// Should be invoked when the client is about to leave a DSMS session.
        /// If the client is the only one in the session it notifies the DSMS to remove the session from the sessions list.
        /// </summary>
        public void OnLeavingSession()
        {
            _shouldReportSession = false;

            var participants = _conjureKit.GetSession()?.GetParticipants();

            if (_conjureKit?.GetSession() == null || participants == null || participants.Count > 1) return;

            _serverCommunication.Send($"domains/{_targetDomainId}/spaces/{_targetSessionId}", "DELETE", null, null,
                error => AukiDebug.LogError($"Failed to send delete command to current session: {error}"));
        }

        /// <summary>
        /// Should be invoked when a participant scans a static Lighthouse
        /// </summary>
        /// <param name="domainId">The domain id of the static lighthouse</param>
        /// <param name="onHosting">Called if there are no sessions in this domain yet, and the current participant will host the session</param>
        /// <param name="onSessionAvailable">Called if another participant is already hosting a session in this domain</param>
        public void DidSelectDomain(string domainId, Action onHosting, Action<string> onSessionAvailable)
        {
            if (_targetDomainId == domainId) return;
            _targetDomainId = domainId;

            _serverCommunication.Send($"domains/{domainId}/spaces", "GET", null, response =>
            {
                SpacesListData sessionList;
                try
                {
                    sessionList = JsonUtility.FromJson<SpacesListData>(response);
                }
                catch (Exception e)
                {
                    AukiDebug.LogError($"Failed to parse organization session data: {e}");
                    return;
                }
                
                foreach (var session in sessionList.spaces)
                {
                    Debug.LogWarning($"[SL]Found session {session.id}, expire at {session.expires_at}");
                }

                if (sessionList.spaces != null && sessionList.spaces.Length > 0)
                {
                    _targetSessionId = sessionList.spaces[0].id;
                    if (_targetSessionId == _conjureKit.GetSession().Id) return;
                    onSessionAvailable?.Invoke(_targetSessionId);
                }
                else
                {
                    _targetSessionId = _conjureKit.GetSession().Id;
                    _showToast?.Invoke($"Hosting in {_targetSessionId}.");
                    onHosting?.Invoke();
                    StartReportingSession();
                }
            }, error => AukiDebug.LogError($"Failed to retrieve sessions for {domainId}: {error}"));
        }

        /// <summary>
        /// Should be called when the participant joined a new session
        /// </summary>
        public void DidJoinSession(Session session)
        {
            if (session.Id == _targetSessionId)
            {
                StartReportingSession();
            }
        }

        /// <summary>
        /// Start reporting the current session to the DSMS backend
        /// </summary>
        private void StartReportingSession()
        {
            _shouldReportSession = true;
        }

        private void OnSessionLeft(Session session)
        {
            _shouldReportSession = false;
            _targetDomainId = "";
        }

        /// <summary>
        /// Report the DSMS backend to associate the current session with the selected domain id
        /// </summary>
        private static DateTime ReportCurrentSession(IServerCommunication serverCommunication, string targetDomainId, string targetSessionId)
        {
            serverCommunication.Send($"domains/{targetDomainId}/spaces", "POST", JsonUtility.ToJson(new SpaceReportData(targetSessionId)), null,
                error => AukiDebug.LogError($"Failed to report current session: {error}"));
            return DateTime.Now;
        }
    }
}