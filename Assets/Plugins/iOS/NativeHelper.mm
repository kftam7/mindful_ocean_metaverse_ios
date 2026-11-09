//
//  NativeHelper.m
//  Unity-iPhone
//
//  Created by Archer Fung on 13/8/2018.
//

#import "NativeHelper.h"
#import <AVFoundation/AVFoundation.h>
#import <StoreKit/StoreKit.h>

#if UNITY_VERSION >= 420
#import "UnityAppController.h"
#else
#import "AppController.h"
#endif

void UnitySendMessage(const char* obj, const char* method, const char* msg);
#if UNITY_VERSION >= 500
void UnityPause(int pause);
#else
void UnityPause(bool pause);
#endif

extern "C" {
    NSString* UnityAdsCreateNSString (const char* string) {
        return string ? [NSString stringWithUTF8String: string] : [NSString stringWithUTF8String: ""];
    }

    char* UnityAdsMakeStringCopy (const char* string) {
        if (string == NULL)
            return NULL;
        char* res = (char*)malloc(strlen(string) + 1);
        strcpy(res, string);
        return res;
    }
}

@implementation NativeHelper
static UIImpactFeedbackGenerator *_impactFeedbackGenerator = nil;
@end

extern "C"
{
    void OpenSetting() {
        NSURL * url = [NSURL URLWithString: UIApplicationOpenSettingsURLString];
        [UIApplication.sharedApplication openURL:url options:@{} completionHandler:nil];
    }

    char* GetCameraPermission()
    {
        NSString* result = @"";

        switch([AVCaptureDevice authorizationStatusForMediaType:AVMediaTypeVideo])
        {
            case AVAuthorizationStatusNotDetermined:
                result = @"Undetermined";
                break;
            case AVAuthorizationStatusDenied:
                result = @"Denied";
                break;
            case AVAuthorizationStatusAuthorized:
                result = @"Granted";
                break;
            default:
                result = @"Denied";
                break;
        }

        return UnityAdsMakeStringCopy([result UTF8String]);
    }

    void RequestCameraPermission(){

        [AVCaptureDevice requestAccessForMediaType:AVMediaTypeVideo completionHandler:^(BOOL granted) {
            dispatch_async(dispatch_get_main_queue(), ^{
                UnitySendMessage("~NativeHelper", "UpdateCameraPermission", GetCameraPermission());
            });
        }];
    }

    void OpenInAppReview(){
        [SKStoreReviewController requestReview];
    }

    void PrepareImpactFeedbackGenerator() {
      //UIViewController *objViewController = [UIApplication sharedApplication].keyWindow.rootViewController;
      if (_impactFeedbackGenerator == nil) {
        _impactFeedbackGenerator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight];
        [_impactFeedbackGenerator prepare];
      }
    }

    void GenerateImpactFeedback() {
      if (_impactFeedbackGenerator != nil) {
          [_impactFeedbackGenerator impactOccurred];
      }
    }
}
