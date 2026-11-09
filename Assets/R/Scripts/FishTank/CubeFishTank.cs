using UnityEngine;

public class CubeFishTank : FishTank
{
    public Vector3 TankCenter = Vector3.zero;
    public Vector3 TankSize = Vector3.one;
    public Vector3 TankRotation = Vector3.zero;

    protected override Volume GetVolume()
    {
        var lossyScale = transform.lossyScale;
        Cube volume = AutoAdjustAfterContainerChanged
            ? new Cube(transform.position + Vector3.Scale(TankCenter, lossyScale),
                Vector3.Scale(TankSize, lossyScale),
                TankRotation + transform.eulerAngles)
            : new Cube(TankCenter, TankSize, TankRotation);

        return volume;
    }
}