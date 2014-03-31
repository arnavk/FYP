//
//  TTCardViewController.m
//  Texter
//
//  Created by Arnav Kumar on 30/1/14.
//  Copyright (c) 2014 Arnav Kumar. All rights reserved.
//

#import "TTCardViewController.h"
#import "REPagedScrollView.h"

@interface TTCardViewController ()

@property (nonatomic, strong) REPagedScrollView *scrollView;

@end

@implementation TTCardViewController

- (id)initWithNibName:(NSString *)nibNameOrNil bundle:(NSBundle *)nibBundleOrNil
{
    self = [super initWithNibName:nibNameOrNil bundle:nibBundleOrNil];
    if (self) {
    }
    return self;
}

- (void) viewWillAppear:(BOOL)animated
{
    [super viewWillAppear:animated];
}

- (void)viewDidLoad
{
    [super viewDidLoad];
    CGRect frame = CardScrollViewFrame;
    self.scrollView = [[REPagedScrollView alloc] initWithFrame:frame];
    [self.view addSubview:self.scrollView];
}

- (void)didReceiveMemoryWarning
{
    [super didReceiveMemoryWarning];
}

- (void) addCardViewWithID:(NSNumber *)ID Image:(UIImage *)image ButtonTitle:(NSString *)buttonTitle
{
    CGRect frame = CardScrollViewFrame;
    TTCardView *page = [[TTCardView alloc] initWithFrame:CGRectMake(20, 20, frame.size.width - 40, frame.size.height - 40 - 36 ) ID:ID Image:image buttonTitle:buttonTitle andDelegate:self];
    [self.scrollView addPage:page];
}

- (void) buttonPressedOnCardView:(TTCardView *)cardView
{
    NSLog(@"INHERITANCE ERROR");
}

@end
