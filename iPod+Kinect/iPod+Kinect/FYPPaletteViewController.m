//
//  FYPPalletteViewController.m
//  iPod+Kinect
//
//  Created by Arnav Kumar on 5/2/14.
//  Copyright (c) 2014 Arnav Kumar. All rights reserved.
//

#import "FYPPaletteViewController.h"
#import "ESCColorPickerView.h"
#import "ESCColorPickerModel.h"
#import "ESCColorPickerPresenter.h"
#import "ConnectionManager.h"
#import "CircularButton.h"

@interface FYPPaletteViewController ()<ColorPickerDelegate>

@property ESCColorPickerPresenter *presenter;

@property (nonatomic, strong) NSString *color;

@end

@implementation FYPPaletteViewController


- (void) viewDidLoad
{
    [super viewDidLoad];
    
    
    self.view.backgroundColor = [UIColor colorWithWhite:0.1 alpha:1.0];
	
	CGRect contentRect = self.view.bounds;
	if ([[[UIDevice currentDevice] systemVersion] integerValue] >= 7) {
		CGRect statusBarFrame = [self.view convertRect:[[UIApplication sharedApplication] statusBarFrame] fromView:self.view.window];
		contentRect = UIEdgeInsetsInsetRect(contentRect, UIEdgeInsetsMake(CGRectGetHeight(statusBarFrame), 0.0, 0.0, 0.0));
	}
	ESCColorPickerView *colorPickerView = [[ESCColorPickerView alloc] initWithFrame:contentRect];
	colorPickerView.autoresizingMask = UIViewAutoresizingFlexibleHeight | UIViewAutoresizingFlexibleWidth;
	[self.view addSubview:colorPickerView];
	
	ESCColorPickerModel *colorPickerModel = [[ESCColorPickerModel alloc] init];
	
	self.presenter = [[ESCColorPickerPresenter alloc] initWithView:colorPickerView Model:colorPickerModel andDelegate:self];
    self.color = @"92DB36";
    
    
//    UIButton *button = [UIButton buttonWithType:UIButtonTypeCustom];
//
//    [button addTarget:self action:@selector(buttonReleased) forControlEvents:UIControlEventTouchUpInside];
//    [button addTarget:self action:@selector(buttonHeld) forControlEvents:UIControlEventTouchDown];
//    
//    [button setTitle:@"YOLO" forState:UIControlStateNormal];
//    button.frame = CGRectMake(140.0, 280.0, 40.0, 40.0);//width and height should be same value
//    button.clipsToBounds = YES;
//    
//    button.layer.cornerRadius = 20;//half of the width
//    button.layer.borderColor=[UIColor redColor].CGColor;
//    button.layer.borderWidth=2.0f;
    
    
    
    
    [[ConnectionManager sharedManager] sendMessage:[NSString stringWithFormat:@"/paint_start/"]];
}

- (void) colorChangedTo:(NSString *)rgbHex
{
    self.color = rgbHex;
    NSLog(@"%@", self.color);
}

- (void) buttonHeld
{
    [[ConnectionManager sharedManager] sendMessage:[NSString stringWithFormat:@"/start/%@", self.color]];
}

- (void) buttonReleased
{
    [[ConnectionManager sharedManager] sendMessage:@"/stop"];    
}

- (void) stateChangedTo:(BOOL)state
{
    if (state) {
        [[ConnectionManager sharedManager] sendMessage:[NSString stringWithFormat:@"/start/%@", self.color]];
    }
    else
        [[ConnectionManager sharedManager] sendMessage:@"/stop"];
}

- (BOOL)canBecomeFirstResponder
{
    return YES;
}

- (void)motionEnded:(UIEventSubtype)motion withEvent:(UIEvent *)event
{
    if (motion == UIEventSubtypeMotionShake)
    {
        NSLog(@"Shake");
        [[ConnectionManager sharedManager] sendMessage:@"/clean_canvas/"];
    } 
}


- (void) viewDidDisappear:(BOOL)animated
{
    [[ConnectionManager sharedManager] sendMessage:[NSString stringWithFormat:@"/paint_stop/"]];
}

@end
