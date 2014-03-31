//
//  TTCardViewController.h
//  Texter
//
//  Created by Arnav Kumar on 30/1/14.
//  Copyright (c) 2014 Arnav Kumar. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "TTCardView.h"

@interface TTCardViewController : UIViewController <CardViewButtonDelegate>

@property (nonatomic, strong) NSNumber *ID;

- (void) addCardViewWithID:(NSNumber *)ID Image:(UIImage *)image ButtonTitle:(NSString *)buttonTitle;

@end
