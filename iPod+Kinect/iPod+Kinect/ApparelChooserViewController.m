//
//  ApparelChooserViewController.m
//  iPod+Kinect
//
//  Created by Arnav Kumar on 15/3/14.
//  Copyright (c) 2014 Arnav Kumar. All rights reserved.
//

#import "ApparelChooserViewController.h"
#import "ConnectionManager.h"

@interface ApparelChooserViewController ()

//@property (nonatomic, strong) NSString *serverIP;
//@property (nonatomic, strong) NSString *serverPort;
//
//@property (nonatomic, strong) NSMutableData *data;
//@property (nonatomic, strong) NSInputStream *iStream;
//@property (nonatomic, strong) NSOutputStream *oStream;

@end

@implementation ApparelChooserViewController

//CFReadStreamRef readStream2 = NULL;
//CFWriteStreamRef writeStream2 = NULL;
//
//- (void) setServerIP:(NSString *)serverIP
//{
//    _serverIP = serverIP;
//}
//
//- (void) setServerPort:(NSString *)serverPort
//{
//    _serverPort = serverPort;
//}

- (void)viewDidLoad
{
    [super viewDidLoad];
	// Do any additional setup after loading the view.
    
//    [self connectToServerUsingCFStream:self.serverIP portNo:[self.serverPort intValue]];
    
    self.title = @"What do you want to try?";
    
    UIBarButtonItem *temporaryBarButtonItem = [[UIBarButtonItem alloc] init];
    temporaryBarButtonItem.title = @"Choose";
    self.navigationItem.backBarButtonItem = temporaryBarButtonItem;
    
    [self addCardViewWithID:[NSNumber numberWithInt:1] Image:[UIImage imageNamed:@"1.png"] ButtonTitle:@"Choose"];
    [self addCardViewWithID:[NSNumber numberWithInt:2] Image:nil ButtonTitle:@"Choose"];
    [self addCardViewWithID:[NSNumber numberWithInt:3] Image:nil ButtonTitle:@"Choose"];
    [[ConnectionManager sharedManager] sendMessage:[NSString stringWithFormat:@"/apparel_start/"]];
}

- (void)didReceiveMemoryWarning
{
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}

- (void) buttonPressedOnCardView:(TTCardView *)cardView
{
    NSLog(@"Pressed");
    [[ConnectionManager sharedManager] sendMessage:[NSString stringWithFormat:@"/cloth/%@", cardView.ID]];
}
//
//- (void) sendMessage:(NSString *)message
//{
//    const uint8_t *str = (uint8_t *) [message cStringUsingEncoding:NSASCIIStringEncoding];
//    [self writeToServer:str];
//}

//- (void) writeToServer:(const uint8_t *) buf {
//    [self.oStream write:buf maxLength:strlen((char*)buf)];
//}
//
//-(void) connectToServerUsingCFStream:(NSString *) urlStr portNo: (uint) portNo {
//    
//    CFStreamCreatePairWithSocketToHost(kCFAllocatorDefault,
//                                       (__bridge CFStringRef) urlStr,
//                                       portNo,
//                                       &readStream2,
//                                       &writeStream2);
//    
//    if (readStream2 && writeStream2) {
//        CFReadStreamSetProperty(readStream2,
//                                kCFStreamPropertyShouldCloseNativeSocket,
//                                kCFBooleanTrue);
//        CFWriteStreamSetProperty(writeStream2,
//                                 kCFStreamPropertyShouldCloseNativeSocket,
//                                 kCFBooleanTrue);
//        
//        self.iStream = (__bridge NSInputStream *)readStream2;
//        [self.iStream setDelegate:self];
//        [self.iStream scheduleInRunLoop:[NSRunLoop currentRunLoop]
//                                forMode:NSDefaultRunLoopMode];
//        [self.iStream open];
//        
//        self.oStream = (__bridge NSOutputStream *)writeStream2;
//        [self.oStream setDelegate:self];
//        [self.oStream scheduleInRunLoop:[NSRunLoop currentRunLoop]
//                                forMode:NSDefaultRunLoopMode];
//        [self.oStream open];
//    }
//}
//
//- (void)stream:(NSStream *)stream handleEvent:(NSStreamEvent)eventCode {
//    
//    switch(eventCode) {
//        case NSStreamEventHasBytesAvailable:
//        {
//            if (self.data == nil) {
//                self.data = [[NSMutableData alloc] init];
//            }
//            uint8_t buf[1024];
//            unsigned long len = 0;
//            len = [(NSInputStream *)stream read:buf maxLength:1024];
//            if(len) {
//                [self.data appendBytes:(const void *)buf length:len];
//                int bytesRead = 0;
//                bytesRead += len;
//            } else {
//                NSLog(@"No data.");
//            }
//            
//            NSString *str = [[NSString alloc] initWithData:self.data
//                                                  encoding:NSUTF8StringEncoding];
//            NSLog(@"%@", str);
//            UIAlertView *alert = [[UIAlertView alloc] initWithTitle:@"From server"
//                                                            message:str
//                                                           delegate:self
//                                                  cancelButtonTitle:@"OK"
//                                                  otherButtonTitles:nil];
//            [alert show];
//            self.data = nil;
//            break;
//        }
//        case NSStreamEventOpenCompleted:
//			NSLog(@"Stream opened");
//			break;
//        case NSStreamEventErrorOccurred:
//			
//			NSLog(@"Can not connect to the host!");
//			break;
//			
//		case NSStreamEventEndEncountered:
//            
//            [self disconnect];
//			
//			break;
//		default:
//			NSLog(@"Unknown event");
//    }
//}
//
//-(void) disconnect {
//    [self.iStream close];
//    [self.oStream close];
//}
//
//- (void) dealloc {
//    [self disconnect];
//    if (readStream2) CFRelease(readStream2);
//    if (writeStream2) CFRelease(writeStream2);
//}

- (void) viewWillDisappear:(BOOL)animated
{
    [[ConnectionManager sharedManager] sendMessage:[NSString stringWithFormat:@"/apparel_stop/"]];
//    [self disconnect];
}

@end
