using System;
using System.Text;
using Auki.Util;
using UnityEngine;
using UnityEngine.Networking;

namespace DomainSessionDemo
{
    /// <summary>
    /// DSMSCommunication handles the communication to the Auki Domain-Session Mapping Service (DSMS)
    /// </summary>
    internal class DSMSCommunication : IServerCommunication
    {
        private readonly string _appKey;
        private readonly string _appSecret;
        
        private const string ApiURL = "https://api.aukiverse.com/";
        private const string DemoServiceURL = "https://dsm.aukiverse.com/";

        private string _accessToken;
        private const double TokenExpirationTolerance = 5 * 60.0f;

        public DSMSCommunication(string appKey, string appSecret)
        {
            _appKey = appKey;
            _appSecret = appSecret;
        }

        public void Send(string endpoint, string method, string json, Action<string> onSuccess, Action<string> onFailure)
        {
            if (HasTokenExpired(_accessToken))
            {
                RequestAuthorizationToken(_appKey, _appSecret, () => SendWithAuthorizationToken(endpoint, method, json, onSuccess, onFailure), onFailure);
            }
            else
            {
                SendWithAuthorizationToken(endpoint, method, json, onSuccess, onFailure);
            }
        }

        private void RequestAuthorizationToken(string appKey, string appSecret, Action onSuccess, Action<string> onFailure)
        {
            AukiDebug.LogTrace($"Making an authorization request to {ApiURL + "service/domains-access-token"}");

            var request = new UnityWebRequest(ApiURL + "service/domains-access-token", "POST");
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{appKey}:{appSecret}"))}");
            request.SendWebRequest().completed += _ =>
            {
                AukiDebug.LogTrace($"Request received code: {request.responseCode} data: {request.downloadHandler.text}");

                if (request.responseCode != 200)
                {
                    onFailure?.Invoke($"Received {request.responseCode} from service/domains-access-token.");
                    return;
                }

                try
                {
                    var tokenData = JsonUtility.FromJson<TokenData>(request.downloadHandler.text);
                    _accessToken = tokenData.access_token;
                    onSuccess?.Invoke();
                }
                catch (Exception e)
                {
                    onFailure?.Invoke($"Failed parsing login response: {e}.");
                }
            };
        }

        private void SendWithAuthorizationToken(string endpoint, string method, string json, Action<string> onSuccess, Action<string> onFailure)
        {
            AukiDebug.LogTrace($"Sending {json} to {DemoServiceURL + endpoint}");

            var request = new UnityWebRequest(DemoServiceURL + endpoint, method);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {_accessToken}");

            if (!string.IsNullOrEmpty(json))
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            }

            request.SendWebRequest().completed += _ =>
            {
                AukiDebug.LogTrace($"Request received code: {request.responseCode} data: {request.downloadHandler.text}");

                if (request.responseCode != 200)
                {
                    onFailure?.Invoke($"Received {request.responseCode} from {endpoint}.");
                    return;
                }

                onSuccess?.Invoke(request.downloadHandler.text);
            };
        }

        private static bool HasTokenExpired(string token)
        {
            return (GetJwtExpirationTime(token) - DateTimeOffset.Now).TotalSeconds <= TokenExpirationTolerance || string.IsNullOrEmpty(token);
        }

        private static DateTimeOffset GetJwtExpirationTime(string token)
        {
            var offset = new DateTimeOffset();
            if (string.IsNullOrEmpty(token)) return offset;

            var parts = token.Split('.');

            if (parts.Length <= 1) return offset;

            AukiDebug.LogTrace($"Parsing {parts[1]} with length {parts[1].Length}");
            var payload = parts[1];

            // Append '=' if length is not divisible by 4 (required for base64 decoding).
            var remainder = payload.Length % 4;
            if (remainder > 0)
            {
                var appended = 4 - remainder;
                for (var i = 0; i < appended; i++)
                {
                    payload += '=';
                }
            }

            var payloadDecoded = Convert.FromBase64String(payload);
            var payloadJsonString = Encoding.UTF8.GetString(payloadDecoded);
            var tokenPayloadData = JsonUtility.FromJson<TokenPayloadData>(payloadJsonString);
            offset = DateTimeOffset.FromUnixTimeSeconds(tokenPayloadData.exp);

            return offset;
        }
    }
}