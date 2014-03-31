//
//  CircularButton.m
//  iPod+Kinect
//
//  Created by Arnav Kumar on 31/3/14.
//  Copyright (c) 2014 Arnav Kumar. All rights reserved.
//

#import "CircularButton.h"
#import <QuartzCore/QuartzCore.h>

@implementation CircularButton

- (id)initWithFrame:(CGRect)frame
{
    self = [super initWithFrame:frame];
    if (self) {
        self.clipsToBounds = YES;
        
        self.layer.cornerRadius = frame.size.width/2;//half of the width
        self.layer.borderColor = [UIColor whiteColor].CGColor;
        self.layer.borderWidth = 3.0f;

    }
    return self;
}

- (void) setFrame:(CGRect)frame
{
    [super setFrame:frame];
    self.layer.cornerRadius = frame.size.width/2;
}

/*
// Only override drawRect: if you perform custom drawing.
// An empty implementation adversely affects performance during animation.
- (void)drawRect:(CGRect)rect
{
    // Drawing code
}
*/

@end
