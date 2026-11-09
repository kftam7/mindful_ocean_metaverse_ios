using System;
using Matterless.Rest;

namespace Auki.Navigation
{
    public interface IRestService
    {
        bool IsCalling { get; }
            
        void SecureGetHack(string url, Action<string> onSuccess,
            Action<RestService.ErrorResponse> onError);
        
        void UnsecureGet(string url, Action<string> onSuccess,
            Action<RestService.ErrorResponse> onError);

        void UnsecurePostJson(string url, string payload, Action<string> onSuccess,
            Action<RestService.ErrorResponse> onError);
        
        void UnsecurePutJson(string url, string payload, Action<string> onSuccess,
            Action<RestService.ErrorResponse> onError);

        void UnsecureDelete(string url, string payload, Action<string> onSuccess, Action<RestService.ErrorResponse> onError);

        void OnErrorResponse(Action onRefreshAuth, Action<RestService.ErrorResponse> onError,
            RestController.RestCallError error);
    }
}