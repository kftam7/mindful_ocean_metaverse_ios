using System.Net;

namespace Network
{
    public partial class MockServerResponder
    {
        private string RespondGetArtworkList(HttpListenerRequest request)
        {
            return _successResponseDic[MockJsonFileNameEnum.GetArtworkList.ToString()];
        }
    }
}