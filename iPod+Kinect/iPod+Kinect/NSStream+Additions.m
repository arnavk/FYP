//
//  NSStream+Additions.m
//  iPodClient
//
//  Created by Arnav Kumar on 15/10/13.
//  Copyright (c) 2013 Arnav Kumar. All rights reserved.
//

#import "NSStream+Additions.h"

@implementation NSStream (Additions)
+ (void)getStreamsToHostNamed:(NSString *)hostName
                         port:(NSInteger)port
                  inputStream:(NSInputStream **)inputStreamPtr
                 outputStream:(NSOutputStream **)outputStreamPtr
{
    CFReadStreamRef     readStream;
    CFWriteStreamRef    writeStream;
    
    assert(hostName != nil);
    assert( (port > 0) && (port < 65536) );
    assert( (inputStreamPtr != NULL) || (outputStreamPtr != NULL) );
    
    readStream = NULL;
    writeStream = NULL;
    
    CFStreamCreatePairWithSocketToHost(
                                       NULL,
                                       (__bridge CFStringRef) hostName,
                                       port,
                                       ((inputStreamPtr  != nil) ? &readStream : NULL),
                                       ((outputStreamPtr != nil) ? &writeStream : NULL)
                                       );
    
    if (inputStreamPtr != NULL) {
        *inputStreamPtr  = CFBridgingRelease(readStream);
        //[NSMakeCollectable(readStream) autorelease];
    }
    if (outputStreamPtr != NULL) {
        *outputStreamPtr = CFBridgingRelease(writeStream);
        //        [NSMakeCollectable(writeStream) autorelease];
    }
}
@end
