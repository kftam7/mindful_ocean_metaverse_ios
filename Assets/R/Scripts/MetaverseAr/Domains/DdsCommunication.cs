using System;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Dds.Communications
{
    public interface IDdsCommunication
    {
        UniTask<string> GetToken(string appKey, string appSecret);
        UniTask<string> Send(string endpoint, string method, string json, string authToken);
    }

    public class DdsCommunication : IDdsCommunication
    {
        public async UniTask<string> GetToken(string appKey, string appSecret)
        {
            using var request = new UnityWebRequest(DdsValues.DomainAccessTokenUrl, DdsValues.POST);
            var encodedCredentials = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes($"{appKey}:{appSecret}"));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Basic {encodedCredentials}");

            try
            {
                await request.SendWebRequest();

                Logger.Log($"Request received code: {request.responseCode} data: {request.downloadHandler.text}");

                var item = JObject.Parse(request.downloadHandler.text);
                var token = item.GetValue("access_token").ToString();
                return token;
            }
            catch (Exception e)
            {
                string errorMessage = request.result == UnityWebRequest.Result.ConnectionError || 
                                      request.result == UnityWebRequest.Result.ProtocolError 
                    ? request.error 
                    : "An unexpected error occurred.";
                    
                Logger.LogError($"Request received code: {request.responseCode} exception: {e.Message} error: {errorMessage}");
                return errorMessage;
            }
        }

        public async UniTask<string> Send(string endpoint, string method, string json, string authToken)
        {
            var url = $"{DdsValues.DdsUrl}/{endpoint}";
            Logger.Log($"Sending {method} url: {url}");
            using var request = new UnityWebRequest(url, method);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            if (!string.IsNullOrEmpty(json))
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)) as UploadHandler;

            if (!string.IsNullOrEmpty(authToken))
                request.SetRequestHeader("Authorization", $"Bearer {authToken}");

            try
            {
                await request.SendWebRequest();

                Logger.Log($"Request received code: {request.responseCode} data:\n{request.downloadHandler.text}");

                return request.downloadHandler.text;
            }
            catch (Exception e)
            {
                string errorMessage = request.result == UnityWebRequest.Result.ConnectionError || 
                                      request.result == UnityWebRequest.Result.ProtocolError 
                    ? request.error 
                    : "An unexpected error occurred.";
                    
                Logger.LogError($"Request received code: {request.responseCode} exception: {e.Message} error:\n{errorMessage}");
                return errorMessage;
            }
        }
    }
}