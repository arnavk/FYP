//
//  DesignChooserViewController.m
//  iPod+Kinect
//
//  Created by Arnav Kumar on 31/3/14.
//  Copyright (c) 2014 Arnav Kumar. All rights reserved.
//

#import "DesignChooserViewController.h"
#import "ConnectionManager.h"
#import "TOWebViewController.h"

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
    
    UIBarButtonItem *rightButton = [[UIBarButtonItem alloc] initWithTitle:@"Browse"
                                                                    style:UIBarButtonItemStyleDone
                                                                   target:self
                                                                   action:@selector(showWebView) ];
    self.navigationItem.rightBarButtonItem = rightButton;
}

- (void) didReceiveMemoryWarning
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

- (void) showWebView
{
    NSURL *url = [NSURL URLWithString:@"https://www.google.com.sg/webhp?hl=en&tbm=isch&tab=wi"];
    TOWebViewController *webViewController = [[TOWebViewController alloc] initWithURL:url];
    self.navigationController.navigationBar.barTintColor = [UIColor blackColor];
    [self presentViewController:[[UINavigationController alloc] initWithRootViewController:webViewController] animated:YES completion:nil];
    
}

@end
