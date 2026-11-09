using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProGamer;

public class WaterLevelBoundary : SceneInstance<WaterLevelBoundary>
{
    public float WaterLevel = 0;
    public float UnderwaterCameraLevel = -1;
    public float WaterSurfaceBoundaryLevel = 1.99f;
}