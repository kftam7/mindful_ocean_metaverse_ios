using UnityEngine;

namespace Network
{
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static GameObject _gameObject;
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance != null) return _instance;

                _instance = FindObjectOfType(typeof(T)) as T;

                if (_instance != null)
                {
                    _gameObject = _instance.gameObject;
                    return _instance;
                }

                Object prefab = Resources.Load($"Singletons/{typeof(T).Name}");
                if (prefab != null)
                {
                    _gameObject = (GameObject)Instantiate(prefab);
                    _gameObject.name = $"_{prefab.name}(Clone)";
                }

                if (_gameObject == null)
                {
                    _gameObject = new GameObject($"_{typeof(T).Name}");
                    _instance = _gameObject.AddComponent<T>();
                }
                else
                {
                    _instance = _gameObject.GetComponent<T>();
                }

                return _instance;
            }
        }

        public void DontDestroyOnLoad()
        {
            DontDestroyOnLoad(_gameObject);
        }
    }
}