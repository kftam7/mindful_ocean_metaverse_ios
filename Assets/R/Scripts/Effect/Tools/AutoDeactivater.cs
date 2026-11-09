using UnityEngine;
using System.Collections;

public class AutoDeactivater : MonoBehaviour
{
    public delegate void DeactivateCallback();

    protected float waitTime = 1f;
    protected DeactivateCallback callback;

    public void Set(DeactivateCallback callback, float time)
    {
        this.callback = callback;
        this.waitTime = time;
    }

    protected bool isCounting = false;

    public void Play()
    {
        if (!isCounting)
        {
            isCounting = true;
            StartCoroutine(Count());
        }
    }

    IEnumerator Count()
    {
        yield return new WaitForSeconds(waitTime);

        if (callback != null)
        {
            callback();
            callback = null;
        }

        isCounting = false;
        this.gameObject.SetActive(false);
    }
}