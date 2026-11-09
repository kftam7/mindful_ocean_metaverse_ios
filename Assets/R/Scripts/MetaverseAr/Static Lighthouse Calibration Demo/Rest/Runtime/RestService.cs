using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Matterless.Rest;
using UnityEngine;


namespace Auki.Navigation
{
    public class RestService : IRestService
    {
        private readonly RestController m_UnsecureRestController;

        public bool IsCalling => m_UnsecureRestController.isCalling;

        public RestService()
        {
            // unsecure
            m_UnsecureRestController = CreateController("_rest_manger_helper_unsecure_");
            m_UnsecureRestController.Start();
        }

        private RestController CreateController(string name)
        {
            var go = new GameObject(name);
            GameObject.DontDestroyOnLoad(go);
            var mono = go.AddComponent<RestMono>();
            return new RestController(mono);
        }
        
        public void SecureGetHack(string url, Action<string> onSuccess,
            Action<ErrorResponse> onError = null)
        {
            WebRequestBuilder builder = new WebRequestBuilder().Url(url).Verb(HttpVerb.GET);
            builder.Header("Authorization",
                $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"6d69fe14-92d7-4d55-887a-c1325325388c:a9f76ef1-a104-4ef1-8c35-8b793469f9c0a232555b-8789-4142-bfd5-545662f5c25a"))}");

            m_UnsecureRestController.Send(builder,
                (handler) => onSuccess(handler.text),
                (error) => OnErrorResponse(null, onError, error));
        }

        public void UnsecureGet(string url, Action<string> onSuccess,
            Action<ErrorResponse> onError = null)
        {
            WebRequestBuilder builder = new WebRequestBuilder().Url(url).Verb(HttpVerb.GET);

            m_UnsecureRestController.Send(builder,
                (handler) => onSuccess(handler.text),
                (error) => OnErrorResponse(null, onError, error));
        }

        public void UnsecurePostJson(string url, string payload, Action<string> onSuccess,
            Action<ErrorResponse> onError = null)
        {
            WebRequestBuilder builder = new WebRequestBuilder().Url(url).Verb(HttpVerb.POST)
                .AddJsonPayload(payload);

            m_UnsecureRestController.Send(builder,
                (handler) => onSuccess(handler.text),
                (error) => OnErrorResponse(null, onError, error));
        }

        public void UnsecurePutJson(string url, string payload, Action<string> onSuccess, Action<ErrorResponse> onError = null)
        {
            WebRequestBuilder builder = new WebRequestBuilder().Url(url).Verb(HttpVerb.PUT)
                .AddJsonPayload(payload);

            m_UnsecureRestController.Send(builder,
                (handler) => onSuccess(handler.text),
                (error) => OnErrorResponse(null, onError, error));
        }

        public void UnsecureDelete(string url, string payload, Action<string> onSuccess, Action<ErrorResponse> onError = null)
        {
            WebRequestBuilder builder = new WebRequestBuilder().Url(url).Verb(HttpVerb.DELETE).AddJsonPayload(payload);
            m_UnsecureRestController.Send(builder, (handler) => onSuccess(handler.text),
                (error) => OnErrorResponse(null, onError, error));
        }

        public void OnErrorResponse(Action onRefreshAuth, Action<ErrorResponse> onError,
            RestController.RestCallError error)
        {
            //Debug.Log(error.code +"=="+HTTP_RESPONSE_CODE.UNAUTHORIZED);
            //Debug.LogWarning(m_CurrentAuthResponse == null);

            ErrorResponse response = null;
            try
            {
                response = JsonUtility.FromJson<ErrorResponse>(error.raw);
                
                if (response != null)
                {
                    Debug.LogError($"Error response code: {response.code}");
                    Debug.LogError($"Error response msg: {response.message}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error response: Code: {error.code} Error: {error.raw}");
                response = new ErrorResponse
                {
                    message = error.raw,
                    code = error.code
                };
            }
            finally
            {
                onError?.Invoke(response);
            }
        }

        public class ErrorResponse
        {
            public string message;
            public long code;
            public string detailes;
        }
    }
}