using System;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public struct SPose
{
    [JsonProperty]
    private float _px, _py, _pz;

    [JsonProperty]
    private float _rx, _ry, _rz, _rw;

    public SPose(Pose pose)
    {
        _px = pose.position.x;
        _py = pose.position.y;
        _pz = pose.position.z;

        _rx = pose.rotation.x;
        _ry = pose.rotation.y;
        _rz = pose.rotation.z;
        _rw = pose.rotation.w;
    }

    public Pose GetPose()
    {
        var p = new Pose
        {
            position = new Vector3(_px, _py, _pz),
            rotation = new Quaternion(_rx, _ry, _rz, _rw)
        };

        return p;
    }
}

public static class GameSerializer
{
    public static byte[] ToJsonByteArray(this object obj) => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj, new JsonSerializerSettings()
    {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    }));

    public static T FromJsonByteArray<T>(this byte[] byteArray) => JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(byteArray));
}