using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformDependency : MonoBehaviour
{
    [SerializeField] bool enableForPlatformAndroid = true;
    [SerializeField] bool enableForPlatformIOS = true;

    // Use this for initialization
    void Awake()
    {
#if UNITY_ANDROID
		this.gameObject.SetActive(enableForPlatformAndroid);
#else
        this.gameObject.SetActive(enableForPlatformIOS);
#endif
    }
}