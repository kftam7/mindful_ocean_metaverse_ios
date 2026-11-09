using System.Net;

namespace Network
{
    public partial class MockServerResponder
    {
        private string RespondGetEvents(HttpListenerRequest request)
        {
            return _successResponseDic[MockJsonFileNameEnum.GetEvents.ToString()];
        }
    }
}