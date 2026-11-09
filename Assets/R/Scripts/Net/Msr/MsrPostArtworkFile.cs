using System.Net;

namespace Network
{
    public partial class MockServerResponder
    {
        private string RespondPostArtworkFile(HttpListenerRequest request)
        {
            return _successResponseDic[MockJsonFileNameEnum.PostArtworkFile.ToString()];
        }
    }
}