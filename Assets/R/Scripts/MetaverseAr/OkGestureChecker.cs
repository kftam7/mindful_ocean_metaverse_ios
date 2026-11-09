using System.Collections.Generic;
using UnityEngine;

public class OkGestureChecker : GestureCheckerBase
{
    public override HandGestureEnum GestureType => HandGestureEnum.Torus;

    public OkGestureChecker(float chargingCheckTime) : base(chargingCheckTime) { }

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

        if (Vector2.Angle(Vector2.up, segThumbTip) > 60) return false;
        if (Vector2.Angle(segIndexMcp, segThumbCmc) < 30) return false;
        if (Vector2.Angle(Vector2.up, segIndexMcp) > 30) return false;
        if (Vector2.Angle(Vector2.up, segMiddleMcp) > 30) return false;
        if (Vector2.Angle(Vector2.up, segRingMcp) > 30) return false;
        if (Vector2.Angle(Vector2.up, segLittleMcp) > 30) return false;
        if (Vector2.Angle(segMiddleMcp, segMiddlePip) > 45) return false;
        if (Vector2.Angle(segIndexMcp, segIndexPip) < 10) return false;
        if (Vector2.Angle(segIndexPip, segIndexDip) < 20) return false;
        if (indexTipData.screenPos.y > middleTipData.screenPos.y) return false;
        if (indexTipData.screenPos.y > ringTipData.screenPos.y) return false;
        if (indexTipData.screenPos.y > littleTipData.screenPos.y) return false;

        // if (Vector2.Angle(segIndexDip, segIndexTip) < 20) return false;

        //
        // float thumbTipLenght = (thumbTipData.screenPos - thumbIpData.screenPos).magnitude;
        // float distThumbTipIndexTip = (thumbTipData.screenPos - indexTipData.screenPos).magnitude;
        // if (distThumbTipIndexTip > thumbTipLenght * 1.5f) return false;

        return true;
    }
}