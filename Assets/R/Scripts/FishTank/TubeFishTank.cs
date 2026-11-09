using UnityEngine;

public class TubeFishTank : FishTank
{
    public Vector3 TankCenter = Vector3.zero;
    public Vector3 TankRotation = Vector3.zero;
    public float TankHeight = 1;
    public float TankRadus = 1;
    public float TankInnerRadus = 0.5f;

    protected override Volume GetVolume()
    {
        var xform = transform;
        Tube volume = AutoAdjustAfterContainerChanged
            ? new Tube(xform.position + Vector3.Scale(TankCenter, xform.lossyScale),
                TankRadus, TankInnerRadus, TankHeight,
                TankRotation + transform.eulerAngles)
            : new Tube(TankCenter, TankRadus, TankInnerRadus, TankHeight, TankRotation);

        return volume;
    }
}