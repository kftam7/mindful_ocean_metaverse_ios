#import <UnityFramework/UnityFramework-Swift.h>
#import "UnityXRNativePtrs.h"
#import <ARKit/ARKit.h>

extern "C" {

HandDetector *detector;

void HandDetector_Create(void* session) {
    UnityXRNativeSession_1* unityXRSession = (UnityXRNativeSession_1*) session;
    ARSession* sess = (__bridge ARSession*)unityXRSession->sessionPtr;
    detector = [[HandDetector alloc] initWithArSession:sess];
}

void HandDetector_Release() {
    detector = nil;
}

void HandDetector_StartDetect() {
    [detector startDetection];
}
}
