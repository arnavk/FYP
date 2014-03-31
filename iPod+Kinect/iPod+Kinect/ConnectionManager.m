//
//  ConnectionManager.m
//  iPod+Kinect
//
//  Created by Arnav Kumar on 31/3/14.
//  Copyright (c) 2014 Arnav Kumar. All rights reserved.
//

#import "ConnectionManager.h"

@interface ConnectionManager () <NSStreamDelegate>

@property (nonatomic, strong) NSString *serverIP;
@property (nonatomic, strong) NSString *serverPort;

@property (nonatomic, strong) NSMutableData *data;
@property (nonatomic, strong) NSInputStream *iStream;
@property (nonatomic, strong) NSOutputStream *oStream;

@end

@implementation ConnectionManager

static ConnectionManager *_sharedInstance = nil;
CFReadStreamRef readStream = NULL;
CFWriteStreamRef writeStream = NULL;

+ (void) initialize
{
    static BOOL initialized = NO;
    if(!initialized)
    {
        initialized = YES;
        _sharedInstance = [[ConnectionManager alloc] init];
    }
}

+ (ConnectionManager *)sharedManager
{
    return _sharedInstance;
}

- (void) setServerIP:(NSString *)serverIP
{
    _serverIP = serverIP;
}

- (void) setServerPort:(NSString *)serverPort
{
    _serverPort = serverPort;
}

- (void) writeToServer:(const uint8_t *) buf {
    [self.oStream write:buf maxLength:strlen((char*)buf)];
}

- (void) sendMessage:(NSString *)message
{
    const uint8_t *str = (uint8_t *) [message cStringUsingEncoding:NSASCIIStringEncoding];
    [self writeToServer:str];
}

- (void) connect
{
    [self connectToServerUsingCFStream:self.serverIP portNo:[self.serverPort intValue]];
}

- (void) connectToServerUsingCFStream:(NSString *) urlStr portNo: (uint) portNo {
    
    CFStreamCreatePairWithSocketToHost(kCFAllocatorDefault,
                                       (__bridge CFStringRef) urlStr,
                                       portNo,
                                       &readStream,
                                       &writeStream);
    
    if (readStream && writeStream) {
        CFReadStreamSetProperty(readStream,
                                kCFStreamPropertyShouldCloseNativeSocket,
                                kCFBooleanTrue);
        CFWriteStreamSetProperty(writeStream,
                                 kCFStreamPropertyShouldCloseNativeSocket,
                                 kCFBooleanTrue);
        
        self.iStream = (__bridge NSInputStream *)readStream;
        [self.iStream setDelegate:self];
        [self.iStream scheduleInRunLoop:[NSRunLoop currentRunLoop]
                                forMode:NSDefaultRunLoopMode];
        [self.iStream open];
        
        self.oStream = (__bridge NSOutputStream *)writeStream;
        [self.oStream setDelegate:self];
        [self.oStream scheduleInRunLoop:[NSRunLoop currentRunLoop]
                                forMode:NSDefaultRunLoopMode];
        [self.oStream open];
    }
}

- (void)stream:(NSStream *)stream handleEvent:(NSStreamEvent)eventCode {
    
    switch(eventCode) {
        case NSStreamEventHasBytesAvailable:
        {
            if (self.data == nil) {
                self.data = [[NSMutableData alloc] init];
            }
            uint8_t buf[1024];
            unsigned long len = 0;
            len = [(NSInputStream *)stream read:buf maxLength:1024];
            if(len) {
                [self.data appendBytes:(const void *)buf length:len];
                int bytesRead = 0;
                bytesRead += len;
            } else {
                NSLog(@"No data.");
            }
            
            NSString *str = [[NSString alloc] initWithData:self.data
                                                  encoding:NSUTF8StringEncoding];
            NSLog(@"%@", str);
            UIAlertView *alert = [[UIAlertView alloc] initWithTitle:@"From server"
                                                            message:str
                                                           delegate:self
                                                  cancelButtonTitle:@"OK"
                                                  otherButtonTitles:nil];
            [alert show];
            self.data = nil;
            break;
        }
        case NSStreamEventOpenCompleted:
			NSLog(@"Stream opened");
			break;
        case NSStreamEventErrorOccurred:
			
			NSLog(@"Can not connect to the host!");
			break;
			
		case NSStreamEventEndEncountered:
            
            [self disconnect];
			
			break;
		default:
			NSLog(@"Unknown event");
    }
}


-(void) disconnect {
    [self.iStream close];
    [self.oStream close];
}

- (void) dealloc {
    [self disconnect];
    if (readStream) CFRelease(readStream);
    if (writeStream) CFRelease(writeStream);
}



@end
