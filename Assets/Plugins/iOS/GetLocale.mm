#import <Foundation/Foundation.h>
NSString * language;
const char *c ;
extern "C"
{
    const char* _GetLocale()
    {
        language = [[NSLocale preferredLanguages] objectAtIndex:0];
        c = [language UTF8String];
        return c;
    }
}