//
//  TTCardView.h
//  Texter
//
//  Created by Arnav Kumar on 22/1/14.
//  Copyright (c) 2014 Arnav Kumar. All rights reserved.
//

#import <UIKit/UIKit.h>

@class TTCardView;
@protocol CardViewButtonDelegate <NSObject>

- (void) buttonPressedOnCardView:(TTCardView *) cardView;

@end

@interface TTCardView : UIView

- (id)initWithFrame:(CGRect)frame ID:(NSNumber *)ID Image:(UIImage *)image buttonTitle:(NSString *)title andDelegate:(id<CardViewButtonDelegate>) delegate;
- (NSNumber *)ID;

@end
