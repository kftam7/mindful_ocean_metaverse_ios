using System.Collections.Generic;
using UnityEngine;

public class HeartGestureChecker : GestureCheckerBase
{
    public override HandGestureEnum GestureType => HandGestureEnum.Heart;

    public HeartGestureChecker(float chargingCheckTime) : base(chargingCheckTime) { }

    protected override bool CheckGesture(Dictionary<JointName, JointData> joints)
    {
        var wristData = joints[JointName.wrist];
        var thumbTipData = joints[JointName.thumbTip];
        var thumbIpData = joints[JointName.thumbIP];
        var thumbMpData = joints[JointName.thumbMP];
        var thumbCmcData = joints[JointName.thumbCMC];
        var indexTipData = joints[JointName.indexTip];
        var indexDipData = joints[JointName.indexDIP];
        var indexPipData = joints[JointName.indexPIP];
        var indexMcpData = joints[JointName.indexMCP];
        var middleTipData = joints[JointName.middleTip];
        var middleDipData = joints[JointName.middleDIP];
        var middlePipData = joints[JointName.middlePIP];
        var middleMcpData = joints[JointName.middleMCP];
        var ringTipData = joints[JointName.ringTip];
        var ringDipData = joints[JointName.ringDIP];
        var ringPipData = joints[JointName.ringPIP];
        var ringMcpData = joints[JointName.ringMCP];
        var littleTipData = joints[JointName.littleTip];
        var littleDipData = joints[JointName.littleDIP];
        var littlePipData = joints[JointName.littlePIP];
        var littleMcpData = joints[JointName.littleMCP];
        Vector2 segThumbTip = thumbTipData.screenPos - thumbIpData.screenPos;
        Vector2 segThumbIp = thumbIpData.screenPos - thumbMpData.screenPos;
        Vector2 segThumbMp = thumbMpData.screenPos - thumbCmcData.screenPos;
        Vector2 segThumbCmc = thumbCmcData.screenPos - wristData.screenPos;
        Vector2 segIndexTip = indexTipData.screenPos - indexDipData.screenPos;
        Vector2 segIndexDip = indexDipData.screenPos - indexPipData.screenPos;
        Vector2 segIndexPip = indexPipData.screenPos - indexMcpData.screenPos;
        Vector2 segIndexMcp = indexMcpData.screenPos - wristData.screenPos;
        Vector2 segMiddleTip = middleTipData.screenPos - middleDipData.screenPos;
        Vector2 segMiddleDip = middleDipData.screenPos - middlePipData.screenPos;
        Vector2 segMiddlePip = middlePipData.screenPos - middleMcpData.screenPos;
        Vector2 segMiddleMcp = middleMcpData.screenPos - wristData.screenPos;
        Vector2 segRingTip = ringTipData.screenPos - ringDipData.screenPos;
        Vector2 segRingDip = ringDipData.screenPos - ringPipData.screenPos;
        Vector2 segRingPip = ringPipData.screenPos - ringMcpData.screenPos;
        Vector2 segRingMcp = ringMcpData.screenPos - wristData.screenPos;
        Vector2 segLittleTip = littleTipData.screenPos - littleDipData.screenPos;
        Vector2 segLittleDip = littleDipData.screenPos - littlePipData.screenPos;
        Vector2 segLittlePip = littlePipData.screenPos - littleMcpData.screenPos;
        Vector2 segLittleMcp = littleMcpData.screenPos - wristData.screenPos;

        if (Vector2.Angle(Vector2.up, segThumbMp) > 30) return false;
        if (Vector2.Angle(segThumbMp, segThumbIp) > 30) return false;
        if (Vector2.Angle(segThumbIp, segThumbTip) > 30) return false;
        if (Vector2.Angle(segMiddlePip, segMiddleDip) < 110) return false;
        if (Vector2.Angle(segRingPip, segRingDip) < 110) return false;
        if (Vector2.Angle(segLittlePip, segLittleDip) < 110) return false;

        float thumbTipLength = (thumbTipData.screenPos - thumbIpData.screenPos).magnitude;
        float distThumbTipIndexTip = (thumbTipData.screenPos - indexTipData.screenPos).magnitude;
        float distThumbTipIndexPip = (thumbTipData.screenPos - indexPipData.screenPos).magnitude;
        if (distThumbTipIndexTip > thumbTipLength && distThumbTipIndexPip > thumbTipLength) return false;

        return true;
    }
}