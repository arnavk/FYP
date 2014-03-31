//
//  TTCardView.m
//  Texter
//
//  Created by Arnav Kumar on 22/1/14.
//  Copyright (c) 2014 Arnav Kumar. All rights reserved.
//

#import "TTCardView.h"
#import "FUIButton.h"
#import "UIColor+FlatUI.h"

@interface TTCardView()

@property (nonatomic, strong) FUIButton *cardButton;
@property (nonatomic, strong) NSNumber *ID;
@property (nonatomic, weak) id<CardViewButtonDelegate> delegate;
@property (nonatomic, strong) UIImageView *imageView;

@end

@implementation TTCardView

- (id)initWithFrame:(CGRect)frame
{
    self = [super initWithFrame:frame];
    if (self) {
        self.backgroundColor = [UIColor clearColor];
        // Initialization code
    }
    return self;
}

- (id)initWithFrame:(CGRect)frame ID:(NSNumber *)ID Image:(UIImage *)image buttonTitle:(NSString *)title andDelegate:(id<CardViewButtonDelegate>) delegate
{
    
    self = [super initWithFrame:frame];
    if (self) {
        self.backgroundColor = [UIColor clearColor];
        // Initialization code
        
        CGRect buttonFrame = CGRectMake(30, frame.size.height - 50, frame.size.width - 60, 40);
        
        CGRect imageFrame = CGRectMake(30, 30, frame.size.width - 60, frame.size.height - 100);
        
        self.ID = ID;
        self.delegate = delegate;
        
        self.cardButton = [[FUIButton alloc] initWithFrame:buttonFrame];
        [self.cardButton setTitle:title forState:UIControlStateNormal];
        self.cardButton.buttonColor = [UIColor turquoiseColor];
        self.cardButton.shadowColor = [UIColor greenSeaColor];
        self.cardButton.shadowHeight = 1.0f;
        self.cardButton.cornerRadius = 6.0f;
        self.cardButton.titleLabel.font = [UIFont fontWithName:@"VERDANA" size:16];
        [self.cardButton setTitleColor:[UIColor cloudsColor] forState:UIControlStateNormal];
        [self.cardButton addTarget:self action:@selector(performAction) forControlEvents:UIControlEventTouchUpInside];
        [self.cardButton setTitleColor:[UIColor cloudsColor] forState:UIControlStateHighlighted];
        [self addSubview:self.cardButton];
        
        
        self.imageView = [[UIImageView alloc] initWithFrame:imageFrame];
        self.imageView.image = image;
        
        
        [self addSubview:self.imageView];
        
        
        [self setNeedsDisplay];
        
    }
    return self;
}

- (void) performAction
{
    [self.delegate buttonPressedOnCardView:self];
}


- (void)drawRect:(CGRect)rect
{
    self.layer.masksToBounds = NO;
    self.layer.shadowColor = [[UIColor whiteColor] CGColor];
    self.layer.shadowOffset = CGSizeMake(0,2);
    self.layer.shadowRadius = 2;
    self.layer.shadowOpacity = 0.2;
    
    UIBezierPath *path = [UIBezierPath bezierPathWithRoundedRect:rect byRoundingCorners:UIRectCornerAllCorners cornerRadii:CGSizeMake(20, 20)];
    [[UIColor cloudsColor] setFill];
    
    [path fill];
}

- (NSNumber *)ID
{
    return _ID;
}

@end
