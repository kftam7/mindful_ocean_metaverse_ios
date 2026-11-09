using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class CFX_AutoDeactivate : MonoBehaviour
{
    public float effectTime = 1f;
    public bool OnlyDeactivate = true;
    public UnityEvent Callback;


    void OnEnable()
    {
        StartCoroutine("End");
    }

    IEnumerator End()
    {
        yield return new WaitForSeconds(effectTime);

        if (Callback != null) Callback.Invoke();

        if (OnlyDeactivate)
        {
#if UNITY_3_5
			this.gameObject.SetActiveRecursively(false);
#else
            this.gameObject.SetActive(false);
#endif
        }
        else
        {
            GameObject.Destroy(this.gameObject);
        }
    }
}