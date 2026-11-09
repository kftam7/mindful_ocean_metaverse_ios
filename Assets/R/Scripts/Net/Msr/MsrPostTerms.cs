using System.Net;

namespace Network
{
    public partial class MockServerResponder
    {
        private string RespondPostTerms(HttpListenerRequest request)
        {
            return _successResponseDic[MockJsonFileNameEnum.PostTerms.ToString()];
        }
    }
}