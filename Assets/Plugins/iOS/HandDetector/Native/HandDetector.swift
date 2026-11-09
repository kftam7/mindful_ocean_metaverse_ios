import Foundation
import Vision
import ARKit

@objc public class HandDetector: NSObject{
    
    @objc public var outputBuffer : CVPixelBuffer?
    private var callbackTarget: String = "HandDetector"
    var session:ARSession?
    var handPoseRequest:VNDetectHumanHandPoseRequest
    
    var nameConversions: [VNHumanHandPoseObservation.JointName: String] = [
        .wrist: "wrist",
        .thumbCMC: "thumbCMC",
        .thumbMP: "thumbMP",
        .thumbIP: "thumbIP",
        .thumbTip: "thumbTip",
        .indexMCP: "indexMCP",
        .indexPIP: "indexPIP",
        .indexDIP: "indexDIP",
        .indexTip: "indexTip",
        .middleMCP: "middleMCP",
        .middlePIP: "middlePIP",
        .middleDIP: "middleDIP",
        .middleTip: "middleTip",
        .ringMCP: "ringMCP",
        .ringPIP: "ringPIP",
        .ringDIP: "ringDIP",
        .ringTip: "ringTip",
        .littleMCP: "littleMCP",
        .littlePIP: "littlePIP",
        .littleDIP: "littleDIP",
        .littleTip: "littleTip"
    ]
    
    @objc public init(arSession: ARSession) {
        session = arSession
        handPoseRequest = VNDetectHumanHandPoseRequest()
        handPoseRequest.usesCPUOnly = false
        handPoseRequest.maximumHandCount = 1
    }
    
    @objc public func startDetection() {
        var msg:String = ""
        guard let buffer : CVPixelBuffer = (session?.currentFrame?.capturedImage)
        else {
            DispatchQueue.main.async {
                UnitySendMessage(self.callbackTarget, "OnHandDetectedFromNative", msg)
            }
            return
        }
        let imageRequestHandler = VNImageRequestHandler(cvPixelBuffer: buffer, orientation: .up, options: [:])
        do {
            try imageRequestHandler.perform([handPoseRequest])
            let observation: VNHumanHandPoseObservation? = handPoseRequest.results?.first
            
            if observation == nil {
                msg = ""
            }
            else
            {
                let displayTransform = session?.currentFrame?.displayTransform(for: UIInterfaceOrientation.landscapeRight, viewportSize: UIScreen.main.bounds.size)
                
                let points = try observation!.recognizedPoints(.all)
                for (key,value) in points
                {
                    guard let name = nameConversions[key] else {
                        continue
                    }
                    
                    var pos = CGPoint(x:value.location.x,y:1-value.location.y)
                    pos = pos.applying(displayTransform!)
                    msg += "\(name):\(pos.x)|\(1-pos.y)|\(value.confidence);"
                }
            }
            
            DispatchQueue.main.async {
                UnitySendMessage(self.callbackTarget, "OnHandDetectedFromNative", msg)
            }
        } catch {
            print(error)
        }
    }
}
