using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPoint : MonoBehaviour
{
    protected static SetPoint mInstance;

    protected List<GameObject> mPointList = new List<GameObject>();

    protected GameObject GetPoint(bool newPoint)
    {
        if (newPoint || mPointList.Count == 0)
        {
            mPointList.Add(GameObject.Instantiate<GameObject>(Resources.Load("Testing/Point") as GameObject, Vector3.one, Quaternion.identity));
        }

        return mPointList[mPointList.Count - 1];
    }

    protected void DeleteAll()
    {
        for (int i = 0; i < mPointList.Count; i++)
        {
            Destroy(mPointList[i]);
        }

        mPointList = new List<GameObject>();
    }

    public static void SetPointAt(Vector3 position, bool newPoint)
    {
        SetPointAt(position, new Vector3(0.1f, 0.1f, 0.1f), newPoint);
    }

    public static void SetPointAt(Vector3 position, Vector3 size, bool newPoint)
    {
        if (mInstance == null)
        {
            GameObject go = new GameObject("PointTool");
            mInstance = go.AddComponent<SetPoint>();
        }

        GameObject p = mInstance.GetPoint(newPoint);
        p.transform.localScale = size;
        p.transform.position = position;
    }

    void OnGUI()
    {
        if (mPointList.Count > 0)
        {
            if (GUI.Button(new Rect(0, 200, 150, 150), "Delete"))
            {
                DeleteAll();
            }
        }
    }
}