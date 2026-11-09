using System.Net;

namespace Network
{
    public partial class MockServerResponder
    {
        private string RespondLogin(HttpListenerRequest request)
        {
            return _successResponseDic[MockJsonFileNameEnum.Login.ToString()];
        }
    }
}