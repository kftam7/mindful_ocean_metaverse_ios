using System.Net;

namespace Network
{
    public partial class MockServerResponder
    {
        private string RespondGetUserInfo(HttpListenerRequest request)
        {
            return _successResponseDic[MockJsonFileNameEnum.GetUserInfo.ToString()];
        }
    }
}