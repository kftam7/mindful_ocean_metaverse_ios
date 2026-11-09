//
//  PrintImage.m
//  Unity-iPhone
//
//  Created by Keith Chan on 23/3/2018.
//

#import <Foundation/Foundation.h>
#import "StringTools.h"

static const char* singletonGameObjectName;
static const char* callback;

extern "C"
{
    void _RegisterCallback(const char* receiver, const char* callbackFunc){

        singletonGameObjectName = MakeStringCopy(receiver);
        callback = MakeStringCopy(callbackFunc);
    }
    
    void _SendMessageToUnity(const char* message){
        
        if(singletonGameObjectName && callback){
            
            UnitySendMessage(singletonGameObjectName, callback, message);
        }
    }

    void _PrintImage(const char* path){
        
        NSArray *paths = [[NSString stringWithUTF8String:path] componentsSeparatedByString:@","];
        NSMutableArray *images = [[NSMutableArray alloc] init];
        
        for (int i = 0; i < paths.count; i++) {
           
            [images addObject:[UIImage imageWithContentsOfFile: paths[i]]];
        }
        
        UIPrintInteractionController *printController = [UIPrintInteractionController sharedPrintController];

        printController.printingItems = images;

        void (^completionHandler)(UIPrintInteractionController *, BOOL, NSError *) = ^(UIPrintInteractionController *pic, BOOL completed, NSError *error){
           
            NSString* message = nil;
            
            if (!completed && error){
                
                message = [NSString stringWithFormat:@"FAILED! due to error in domain %@ with error code %ld", error.domain, error.code];
                
                UnitySendMessage(singletonGameObjectName, callback, [message UTF8String]);

            }else{
                if(completed){
                    
                    UnitySendMessage(singletonGameObjectName, callback, "success");
                }else{
                    
                    UnitySendMessage(singletonGameObjectName, callback, "cancel");
                }
            }
        };

        [printController presentAnimated:YES completionHandler: completionHandler];
    }
}
