//
//  NetworkViewController.m
//  iPodClient
//
//  Created by Arnav Kumar on 15/10/13.
//  Copyright (c) 2013 Arnav Kumar. All rights reserved.
//

#import "NetworkViewController.h"
#import "NSStream+Additions.h"
#import "FYPPaletteViewController.h"
#import "ApparelChooserViewController.h"
#import "ConnectionManager.h"

@interface NetworkViewController ()
//@property (nonatomic, strong) NSMutableData *data;
//@property (nonatomic, strong) NSInputStream *iStream;
//@property (nonatomic, strong) NSOutputStream *oStream;
@property (strong, nonatomic) IBOutlet UITextField *ipField;
@property (strong, nonatomic) IBOutlet UITextField *portField;
@property (strong, nonatomic) IBOutlet UIButton *paintButton;
@property (strong, nonatomic) IBOutlet UIButton *apparelChooserButton;


@end

@implementation NetworkViewController

- (id)initWithNibName:(NSString *)nibNameOrNil bundle:(NSBundle *)nibBundleOrNil
{
    self = [super initWithNibName:nibNameOrNil bundle:nibBundleOrNil];
    if (self) {

    }
    return self;
}

- (void)viewDidLoad
{
    [[UIApplication sharedApplication] setStatusBarStyle:UIStatusBarStyleLightContent];
    self.navigationController.navigationBar.barTintColor = [UIColor blackColor];
    [super viewDidLoad];
    self.view.backgroundColor = [UIColor colorWithWhite:0.1 alpha:1.0];
    NSDictionary *attributes = [NSDictionary dictionaryWithObjectsAndKeys:
                                [UIColor whiteColor], NSForegroundColorAttributeName, nil];
    
    self.navigationController.navigationBar.titleTextAttributes = attributes;
}

- (void)didReceiveMemoryWarning
{
    [super didReceiveMemoryWarning];
    
}

- (IBAction)paintButtonPressed:(UIButton *)sender {

    [self performSegueWithIdentifier:@"toColorPalette" sender:nil];
    
}
- (IBAction)apparelButtonPressed {
    [self performSegueWithIdentifier:@"toApparelChooser" sender:nil];
}

- (void) prepareForSegue:(UIStoryboardSegue *)segue sender:(id)sender
{
    if ([[segue identifier] isEqualToString:@"toColorPalette"])
    {
        
        [[ConnectionManager sharedManager] setServerIP:self.ipField.text];
        [[ConnectionManager sharedManager] setServerPort:self.portField.text];
        [[ConnectionManager sharedManager] connect];
    }
    
    if ([[segue identifier] isEqualToString:@"toApparelChooser"])
    {
        
        [[ConnectionManager sharedManager] setServerIP:self.ipField.text];
        [[ConnectionManager sharedManager] setServerPort:self.portField.text];
        [[ConnectionManager sharedManager] connect];        
    }
    
}
@end