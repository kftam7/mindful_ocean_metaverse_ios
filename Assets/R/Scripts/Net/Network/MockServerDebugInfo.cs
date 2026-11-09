namespace Network
{
    public class MockServerDebugInfo
    {
        public MockServerFailTypeEnum FailType { get; private set; } = MockServerFailTypeEnum.None;

        public float Delay { get; private set; }

        public int SucceedAfterRetries { get; private set; }

        public static MockServerDebugInfo Create()
        {
            return new MockServerDebugInfo();
        }

        public MockServerDebugInfo SetFailType(MockServerFailTypeEnum type)
        {
            FailType = type;
            return this;
        }

        public MockServerDebugInfo SetRetryTimes(int times)
        {
            SucceedAfterRetries = times;
            return this;
        }

        public MockServerDebugInfo SetDelay(float sec)
        {
            Delay = sec;
            return this;
        }
    }
}