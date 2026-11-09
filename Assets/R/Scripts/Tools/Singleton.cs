/*
 * Singleton.cs
 * 
 * - Unity Implementation of Singleton template
 * 
 */

using UnityEngine;
using ProGamer;
using System;

/// <summary>
/// Be aware this will not prevent a non singleton constructor
///   such as `T myT = new T();`
/// To prevent that, add `protected T () {}` to your singleton class.
///
/// As a note, this is made as MonoBehaviour because we need Coroutines.
/// </summary>
///
namespace ProGamer
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        private static object _lock = new object();

        private static bool created = false;

//Error code: Never used
#pragma warning disable 219
        protected virtual void Awake()
        {
            T t = Singleton<T>.Instance;
        }
#pragma warning restore 219


        public static T Instance
        {
            get
            {
                if (applicationIsQuitting)
                {
                    Debug.Log("[Singleton] Instance '" + typeof(T) +
                              "' already destroyed on application quit." +
                              " Won't create again.");
                    return _instance;
                }

                lock (_lock)
                {
                    if (_instance == null && !created)
                    {
                        _instance = (T)FindObjectOfType(typeof(T));

                        if (FindObjectsOfType(typeof(T)).Length > 1)
                        {
                            Debug.LogError("[Singleton] Something went really wrong " +
                                           " - there should never be more than 1 singleton!" +
                                           " Reopenning the scene might fix it.");
                            return _instance;
                        }

                        if (_instance == null)
                        {
                            created = true;
                            GameObject singleton = new GameObject();
                            Type autoClassType = AutoClassManager.GetAutoClass(typeof(T));

                            _instance = singleton.AddComponent(autoClassType) as T;

                            //use base class type as object name for UnitySendMessage callback
                            _instance.gameObject.name = "~" + typeof(T).ToString();

                            Debug.Log("[Singleton] An instance of " + autoClassType +
                                      " is needed in the scene, so '" + singleton +
                                      "' was created with DontDestroyOnLoad.");
                        }
                        else
                        {
                            //use base class type as object name for UnitySendMessage callback
                            _instance.gameObject.name = "~" + typeof(T).ToString();

                            Debug.Log("[Singleton] Using instance already created: " +
                                      _instance.gameObject.name);
                        }


                        DontDestroyOnLoad(_instance.transform.root.gameObject);
                    }

                    return _instance;
                }
            }
        }


        private static bool applicationIsQuitting = false;

        /// <summary>
        /// When Unity quits, it destroys objects in a random order.
        /// In principle, a Singleton is only destroyed when application quits.
        /// If any script calls Instance after it have been destroyed, 
        ///   it will create a buggy ghost object that will stay on the Editor scene
        ///   even after stopping playing the Application. Really bad!
        /// So, this was made to be sure we're not creating that buggy ghost object.
        /// </summary>
        public virtual void OnDestroy()
        {
            applicationIsQuitting = true;
        }
    }
}