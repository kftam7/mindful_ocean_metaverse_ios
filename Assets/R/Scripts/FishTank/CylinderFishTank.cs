using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CylinderFishTank : FishTank
{
    public Vector3 TankCenter = Vector3.zero;
    public Vector3 TankRotation = Vector3.zero;
    public float TankHeight = 1;
    public float TankRadus = 1;

    protected override Volume GetVolume()
    {
        Cylinder volume = AutoAdjustAfterContainerChanged
            ? new Cylinder(this.transform.position + Vector3.Scale(TankCenter, this.transform.lossyScale), TankRadus, TankHeight, TankRotation + this.transform.eulerAngles)
            : new Cylinder(TankCenter, TankRadus, TankHeight, TankRotation);

        return volume;
    }
}