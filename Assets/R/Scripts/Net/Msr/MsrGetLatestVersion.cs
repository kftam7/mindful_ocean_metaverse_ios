using System.Net;

namespace Network
{
    public partial class MockServerResponder
    {
        private string RespondGetLatestVersion(HttpListenerRequest request)
        {
            return _successResponseDic[MockJsonFileNameEnum.GetLatestVersion.ToString()];
        }
    }
}