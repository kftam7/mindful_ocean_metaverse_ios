using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegionMarker : SceneInstance<RegionMarker>
{
    protected LineRenderer marker;

    public LineRenderer Marker
    {
        get
        {
            if (marker == null)
            {
                GameObject go = new GameObject("RegionMarker");
                go.transform.SetParent(this.transform);

                marker = go.AddComponent<LineRenderer>();
                marker.widthMultiplier = 0.2f;
                marker.loop = true;
            }

            return marker;
        }
    }

    public void Draw(Vector3[] position)
    {
        Marker.positionCount = position.Length;
        Marker.SetPositions(position);
    }
}