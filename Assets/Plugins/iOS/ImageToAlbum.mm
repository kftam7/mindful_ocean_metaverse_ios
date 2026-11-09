#import <Foundation/Foundation.h>
#import "ImageToAlbum.h"
#import "StringTools.h"

@implementation ImageToAlbum

static const char* imageToAlbumReceiver;
static const char* imageToAlbumCallback;
static ImageToAlbum *instance = nil;

-(void)saveToAlbum: (UIImage*) image{
    
    UIImageWriteToSavedPhotosAlbum(image, self, @selector(savedResult:didFinishSavingWithError:contextInfo:), nil);
}

- (void)savedResult:(UIImage *)image didFinishSavingWithError:(NSError *)error contextInfo:(void *)contextInfo{
    if(imageToAlbumReceiver && imageToAlbumCallback){
        if(error){
            UnitySendMessage(imageToAlbumReceiver, imageToAlbumCallback, "fail");
        }else{
            UnitySendMessage(imageToAlbumReceiver, imageToAlbumCallback, "success");
        }
    }
}

extern "C"{
    
    ImageToAlbum* _ImageToAlbumInstance()
    {
        if (instance == nil){
            
            instance = [[ImageToAlbum alloc] init];
        }
        return instance;
    }
    
    void _RegisterImageToAlbumCallback(const char* receiver, const char* callbackFunc){
        
        imageToAlbumReceiver = MakeStringCopy(receiver);
        imageToAlbumCallback = MakeStringCopy(callbackFunc);
    }
    
    void _DeregisterImageToAlbumCallback(){
        
        imageToAlbumReceiver = nil;
        imageToAlbumCallback = nil;
    }
    
    void _ImgToAlbum(const char* path) {

        UIImage *image = [UIImage imageWithContentsOfFile:[NSString stringWithUTF8String:path]];
        [_ImageToAlbumInstance() saveToAlbum:image];
    }
}
@end
