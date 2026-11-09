using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankPreviewHandler : MonoBehaviour
{
    [SerializeField] protected GameObject preview;
    [SerializeField] protected GameObject blockingPreview;

    protected bool isBlocking = false;

    void OnTriggerEnter(Collider other)
    {
        SetBlocking(true, other);
    }

    void OnTriggerExit(Collider other)
    {
        SetBlocking(false, other);
    }

    void OnTriggerStay(Collider other)
    {
        SetBlocking(true, other);
    }

    public void SetBlocking(bool blocked, Collider by)
    {
        if (blocked != isBlocking && by.gameObject.tag == "Tank")
        {
            isBlocking = blocked;

            preview.SetActive(!blocked);
            blockingPreview.SetActive(blocked);
        }
    }
}