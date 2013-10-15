//
//  NSStream+Additions.h
//  iPodClient
//
//  Created by Arnav Kumar on 15/10/13.
//  Copyright (c) 2013 Arnav Kumar. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface NSStream (Additions)

+ (void)getStreamsToHostNamed:(NSString *)hostName
                         port:(NSInteger)port
                  inputStream:(NSInputStream **)inputStreamPtr
                 outputStream:(NSOutputStream **)outputStreamPtr;

@end
