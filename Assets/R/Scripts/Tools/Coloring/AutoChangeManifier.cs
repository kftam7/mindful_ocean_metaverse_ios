using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PaintCraft.Tools;

public class AutoChangeManifier : MonoBehaviour
{
    public LineConfig LineConfig;
    public Brush Brush;

    protected LineConfig LastLineConfig;
    protected Brush LastBrush;

    protected bool isManifier = false;

    void Update()
    {
        if (Input.touchCount > 1)
        {
            if (!isManifier)
            {
                if (LineConfig.Brush != null)
                {
                    LastBrush = LineConfig.Brush;
                }

                LineConfig.Brush = Brush;

                isManifier = true;
            }
        }
        else
        {
            if (isManifier)
            {
                //Recover
                if (LastBrush != null)
                {
                    LineConfig.Brush = LastBrush;
                }

                isManifier = false;
            }
        }
    }
}