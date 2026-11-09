using System.Net;

namespace Network
{
    public partial class MockServerResponder
    {
        private string RespondGetArtworkFileList(HttpListenerRequest request)
        {
            return _successResponseDic[MockJsonFileNameEnum.GetArtworkFileList.ToString()];
        }
    }
}