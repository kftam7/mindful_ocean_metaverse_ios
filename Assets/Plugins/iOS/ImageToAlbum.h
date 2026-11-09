//
//  ImageToAlbum.h
//  Unity-iPhone
//
//  Created by Keith Chan on 17/5/2018.
//

@interface ImageToAlbum : NSObject

@end

extern "C"
{
    void _RegisterImageToAlbumCallback(const char* receiver, const char* callbackFunc);
    void _DeregisterImageToAlbumCallback();
    void _ImgToAlbum(const char* path);
}
