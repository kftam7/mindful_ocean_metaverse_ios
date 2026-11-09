using System.Net;

namespace Network
{
    public partial class MockServerResponder
    {
        private string RespondGetAppNftsInfo(HttpListenerRequest request)
        {
            return _successResponseDic[MockJsonFileNameEnum.GetAppNftsInfo.ToString()];
        }
    }
}