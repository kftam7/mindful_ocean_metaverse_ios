using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskTest : MonoBehaviour
{
    void OnGUI()
    {
        if (GUI.Button(new Rect(0, 300, 100, 100), "1999"))
        {
            foreach (SetRenderQueue q in GetComponentsInChildren<SetRenderQueue>())
            {
                q.SetQueues(1999);
            }
        }

        if (GUI.Button(new Rect(0, 450, 100, 100), "3000"))
        {
            foreach (SetRenderQueue q in GetComponentsInChildren<SetRenderQueue>())
            {
                q.SetQueues(3000);
            }
        }
    }
}