//
//  DesignChooserViewController.m
//  iPod+Kinect
//
//  Created by Arnav Kumar on 31/3/14.
//  Copyright (c) 2014 Arnav Kumar. All rights reserved.
//

#import "DesignChooserViewController.h"
#import "ConnectionManager.h"

@interface DesignChooserViewController ()

@end

@implementation DesignChooserViewController

- (void)viewDidLoad
{
    [super viewDidLoad];
    
    self.title = @"Design Selection";
    
    UIBarButtonItem *temporaryBarButtonItem = [[UIBarButtonItem alloc] init];
    temporaryBarButtonItem.title = @"Choose";
    self.navigationItem.backBarButtonItem = temporaryBarButtonItem;
    
    [self addCardViewWithID:[NSNumber numberWithInt:1] Image:[UIImage imageNamed:@"d1.png"] ButtonTitle:@"Choose"];
    [self addCardViewWithID:[NSNumber numberWithInt:2] Image:[UIImage imageNamed:@"d2.png"] ButtonTitle:@"Choose"];
    [self addCardViewWithID:[NSNumber numberWithInt:3] Image:[UIImage imageNamed:@"d3.png"] ButtonTitle:@"Choose"];
    [self addCardViewWithID:[NSNumber numberWithInt:4] Image:[UIImage imageNamed:@"d4.png"] ButtonTitle:@"Choose"];
}

- (void)didReceiveMemoryWarning
{
    [super didReceiveMemoryWarning];
}

- (void) buttonPressedOnCardView:(TTCardView *)cardView
{
    NSLog(@"Pressed");
    [[ConnectionManager sharedManager] sendMessage:[NSString stringWithFormat:@"/design/%@", cardView.ID]];
}

- (void) viewDidDisappear:(BOOL)animated
{
    [[ConnectionManager sharedManager] sendMessage:[NSString stringWithFormat:@"/clear_design/"]];
}

@end
