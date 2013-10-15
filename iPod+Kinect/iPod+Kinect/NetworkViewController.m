//
//  NetworkViewController.m
//  iPodClient
//
//  Created by Arnav Kumar on 15/10/13.
//  Copyright (c) 2013 Arnav Kumar. All rights reserved.
//

#import "NetworkViewController.h"
#import "NSStream+Additions.h"

@interface NetworkViewController ()
@property (nonatomic, strong) NSMutableData *data;
@property (nonatomic, strong) NSInputStream *iStream;
@property (nonatomic, strong) NSOutputStream *oStream;
@property (strong, nonatomic) IBOutlet UITextField *ipField;
@property (strong, nonatomic) IBOutlet UITextField *portField;
@property (strong, nonatomic) IBOutlet UITextField *messageField;
@property (strong, nonatomic) IBOutlet UIButton *connectButton;
@property (strong, nonatomic) IBOutlet UIButton *sendMessageButton;


@end

@implementation NetworkViewController

CFReadStreamRef readStream = NULL;
CFWriteStreamRef writeStream = NULL;

- (id)initWithNibName:(NSString *)nibNameOrNil bundle:(NSBundle *)nibBundleOrNil
{
    self = [super initWithNibName:nibNameOrNil bundle:nibBundleOrNil];
    if (self) {
        // Custom initialization
    }
    return self;
}

- (void)viewDidLoad
{
    [super viewDidLoad];
    readStream = NULL;
    writeStream = NULL;
}

- (void)didReceiveMemoryWarning
{
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}
- (IBAction)connectButtonPressed:(UIButton *)sender {
    [self connectToServerUsingCFStream:self.ipField.text portNo:[self.portField.text intValue]];
}
- (IBAction)sendMessageButtonPressed:(UIButton *)sender {
    const uint8_t *str =
    (uint8_t *) [self.messageField.text cStringUsingEncoding:NSASCIIStringEncoding];
    [self writeToServer:str];
    self.messageField.text = @"";
}

-(void) connectToServerUsingCFStream:(NSString *) urlStr portNo: (uint) portNo {
    
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

-(void) writeToServer:(const uint8_t *) buf {
    [self.oStream write:buf maxLength:strlen((char*)buf)];
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
