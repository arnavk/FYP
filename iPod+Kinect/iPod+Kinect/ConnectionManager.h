//
//  ConnectionManager.h
//  iPod+Kinect
//
//  Created by Arnav Kumar on 31/3/14.
//  Copyright (c) 2014 Arnav Kumar. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface ConnectionManager : NSObject

+ (ConnectionManager *) sharedManager;

- (void) setServerIP:(NSString *)serverIP;
- (void) setServerPort:(NSString *)serverPort;
- (void) sendMessage:(NSString *)message;
- (void) disconnect;
- (void) connect;

@end
