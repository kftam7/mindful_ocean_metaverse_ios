using System.Net;

namespace Network
{
    public partial class MockServerResponder
    {
        private string RespondGetArtworkFile(HttpListenerRequest request)
        {
            return _successResponseDic[MockJsonFileNameEnum.GetArtworkFile.ToString()];
        }
    }
}