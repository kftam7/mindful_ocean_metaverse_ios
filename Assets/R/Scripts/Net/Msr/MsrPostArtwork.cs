using System.Net;

namespace Network
{
    public partial class MockServerResponder
    {
        private string RespondPostArtwork(HttpListenerRequest request)
        {
            return _successResponseDic[MockJsonFileNameEnum.PostArtwork.ToString()];
        }
    }
}