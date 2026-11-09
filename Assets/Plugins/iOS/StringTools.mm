//
//  StringTools.m
//  Unity-iPhone
//
//  Created by Keith Chan on 17/5/2018.
//

#import "StringTools.h"

extern "C"{

    char* MakeStringCopy(const char* str)
    {
        if (str == NULL) {
            return NULL;
        }
        
        char* res = (char*)malloc(strlen(str) + 1);
        
        strcpy(res, str);
        return res;
    }
}
