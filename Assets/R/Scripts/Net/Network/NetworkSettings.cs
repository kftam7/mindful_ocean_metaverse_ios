using UnityEngine;

namespace Network
{
    public enum NetworkEnvironmentEnum
    {
        Saved = 0,
        Product = 1,
        Develop = 2,
        Mock = 3
    }

    public class NetworkSettings : Singleton<NetworkSettings>
    {
        #region Serialize Field

        [SerializeField] private NetworkEnvironmentEnum _environment = NetworkEnvironmentEnum.Mock;
        [SerializeField] private string _checkConnectionUrl = "www.bing.com";
        [SerializeField] private string _productLogicUrl = "";
        [SerializeField] private string _developLogicUrl = "";

        public string TestAppleUserId;
        public string TestAppleIdToken;
        public string TestAppleAuthCode;

        public int MaxRetryTimes = 3;
        public float Timeout = 5;

        #endregion

        #region Public Feilds

        public int SavedEnvironment
        {
            get => PlayerPrefs.GetInt("NETWORK_ENVIRONMENT", 2);
            set => PlayerPrefs.SetInt("NETWORK_ENVIRONMENT", value);
        }

        public NetworkEnvironmentEnum Environment
        {
            get
            {
                if (_environment == NetworkEnvironmentEnum.Saved)
                {
                    _environment = (NetworkEnvironmentEnum)SavedEnvironment;
                    if (_environment == NetworkEnvironmentEnum.Saved)
                    {
                        _environment = NetworkEnvironmentEnum.Develop;
                    }
                }

                return _environment;
            }
        }

        public bool IsMock => Environment == NetworkEnvironmentEnum.Mock;

        #endregion

        #region Public Methods

        public string GetCheckingConnectionUrl()
        {
            switch (Environment)
            {
                case NetworkEnvironmentEnum.Mock:
                    return MockServer.Instance.GetMockServerUrl();
                default:
                    return _checkConnectionUrl;
            }
        }

        public string GetLogicServerUrl()
        {
            switch (Environment)
            {
                case NetworkEnvironmentEnum.Develop:
                    return _developLogicUrl;
                case NetworkEnvironmentEnum.Product:
                    return _productLogicUrl;
                case NetworkEnvironmentEnum.Mock:
                    return MockServer.Instance.GetMockServerUrl();
                default:
                    return MockServer.Instance.GetMockServerUrl();
            }
        }

        #endregion
    }
}