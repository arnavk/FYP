//
//  AKTextField.m
//  iPod+Kinect
//
//  Created by Arnav Kumar on 7/2/14.
//  Copyright (c) 2014 Arnav Kumar. All rights reserved.
//

#import "AKTextField.h"

@implementation AKTextField

- (id)initWithFrame:(CGRect)frame
{
    self = [super initWithFrame:frame];
    if (self) {
        // Initialization code
    }
    return self;
}

- (void)drawRect:(CGRect)rect
{
    
    CALayer *layer = self.layer;
    layer.backgroundColor = [[UIColor whiteColor] CGColor];
    layer.cornerRadius = 10.0;
    layer.masksToBounds = YES;
    layer.borderWidth = 1.0;
    layer.borderColor = [[UIColor colorWithRed:0 green:0 blue:0 alpha:1] CGColor];
    [layer setShadowColor: [[UIColor blackColor] CGColor]];
    [layer setShadowOpacity:1];
    [layer setShadowRadius:4];
    [layer setShadowOffset: CGSizeMake(0, 2.0)];
    [self setClipsToBounds:NO];
    [self setContentVerticalAlignment:UIControlContentVerticalAlignmentCenter];
    [self setContentHorizontalAlignment:UIControlContentHorizontalAlignmentLeft];
}
- (CGRect)textRectForBounds:(CGRect)bounds {
    return CGRectMake(bounds.origin.x + 20, bounds.origin.y + 8, bounds.size.width - 40, bounds.size.height - 16);
}
- (CGRect)editingRectForBounds:(CGRect)bounds {
    return [self textRectForBounds:bounds];
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
