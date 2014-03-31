#import "ESCColorPickerView.h"
#import "ESCGradientSlider.h"
#import "ESCHueWheel.h"
#import <QuartzCore/QuartzCore.h>
//#import "FlatUIKit.h"

#define PADDING 15.0

@interface ESCColorPickerView()<ESCObservableInternal>

@property (nonatomic) ESCHueWheel *hueWheel;
@property (nonatomic) ESCGradientSlider *saturationSlider;
@property (nonatomic) ESCGradientSlider *brightnessSlider;
@property (nonatomic) UILabel *colorLabel;
@property (nonatomic) UITapGestureRecognizer *tapGestureRecognizer;
//@property (nonatomic, strong) FUIButton *paintButton;
@property (nonatomic, strong) UISwitch *paintSwitch;

@end

@implementation ESCColorPickerView

- (instancetype)initWithFrame:(CGRect)frame {
    if (self = [super initWithFrame:frame]) {
		[self escRegisterObserverProtocol:@protocol(ESCColorPickerViewObserver)];
		
		self.backgroundColor = [UIColor colorWithWhite:0.1 alpha:1.0];
		
		self.hueWheel = [[ESCHueWheel alloc] init];
		[self.hueWheel escAddObserver:self forSelector:@selector(hueDidChange:)];
		[self addSubview:self.hueWheel];
		
		self.saturationSlider = [[ESCGradientSlider alloc] init];
		[self.saturationSlider escAddObserver:self forSelector:@selector(sliderValueDidChange:) forwardingToSelector:@selector(saturationDidChange:)];
		[self addSubview:self.saturationSlider];
		
		self.brightnessSlider = [[ESCGradientSlider alloc] init];
		[self.brightnessSlider escAddObserver:self forSelector:@selector(sliderValueDidChange:) forwardingToSelector:@selector(brightnessDidChange:)];
		[self addSubview:self.brightnessSlider];
		
		self.tapGestureRecognizer = [[UITapGestureRecognizer alloc] initWithTarget:self action:@selector(colorLabelTapped:)];
		
		self.colorLabel = [[UILabel alloc] init];
		self.colorLabel.backgroundColor = [UIColor clearColor];
		self.colorLabel.textAlignment = NSTextAlignmentCenter;
		self.colorLabel.userInteractionEnabled = YES;
		[self.colorLabel addGestureRecognizer:self.tapGestureRecognizer];
		[self addSubview:self.colorLabel];
        
        self.paintSwitch = [[UISwitch alloc] init];
//        self.paintSwitch.onColor = [UIColor turquoiseColor];
//        self.paintSwitch.offColor = [UIColor cloudsColor];
//        self.paintSwitch.onBackgroundColor = [UIColor midnightBlueColor];
////        self.paintSwitch.offBackgroundColor = [UIColor silverColor];
//        self.paintSwitch.offLabel.font = [UIFont boldFlatFontOfSize:14];
//        self.paintSwitch.onLabel.font = [UIFont boldFlatFontOfSize:14];
        [self addSubview:self.paintSwitch];
        self.paintSwitch.layer.cornerRadius = 16.0;
        [self.paintSwitch setTintColor:[UIColor whiteColor]];
        [self.paintSwitch setOnTintColor:[UIColor greenColor]];
        self.paintSwitch.backgroundColor = [UIColor redColor];
        [self.paintSwitch addTarget:self action:@selector(switchStateChanged) forControlEvents:UIControlEventAllTouchEvents];
        
    }
    return self;
}

- (void)colorLabelTapped:(UITapGestureRecognizer *)gestureRecognizer {
	if (gestureRecognizer.state == UIGestureRecognizerStateEnded) {
		[self.escNotifier colorDescriptionTapped];
	}
}

- (void)saturationDidChange:(CGFloat)saturation {
	[self.escNotifier saturationDidChange:saturation];
}

- (void)brightnessDidChange:(CGFloat)brightness {
	[self.escNotifier brightnessDidChange:brightness];
}

- (void)hueDidChange:(CGFloat)hue {
	[self.escNotifier hueDidChange:hue];
}

- (void)setHue:(CGFloat)hue saturation:(CGFloat)saturation brightness:(CGFloat)brightness {
	[self.saturationSlider setStartColor:[UIColor colorWithHue:hue saturation:0.0 brightness:brightness alpha:1.0]];
	[self.saturationSlider setEndColor:[UIColor colorWithHue:hue saturation:1.0 brightness:brightness alpha:1.0]];
	[self.saturationSlider setSliderValue:saturation];
	
	[self.brightnessSlider setStartColor:[UIColor colorWithHue:hue saturation:saturation brightness:0.0 alpha:1.0]];
	[self.brightnessSlider setEndColor:[UIColor colorWithHue:hue saturation:saturation brightness:1.0 alpha:1.0]];
	[self.brightnessSlider setSliderValue:brightness];
	
	[self.hueWheel setHue:hue saturation:saturation brightness:brightness];
}

- (void)setColorDescriptionKeys:(NSArray *)keys values:(NSArray *)values {
	NSMutableAttributedString *colorString = [[NSMutableAttributedString alloc] init];
	
	NSDictionary *keyAttributes = @{NSFontAttributeName : [UIFont fontWithName:@"HelveticaNeue-Light" size:25.0], NSForegroundColorAttributeName : [UIColor colorWithWhite:0.5 alpha:1.0]};
	NSDictionary *valueAttributes = @{NSFontAttributeName : [UIFont fontWithName:@"HelveticaNeue-Bold" size:25.0], NSForegroundColorAttributeName : [UIColor colorWithWhite:0.95 alpha:1.0]};
	
	NSInteger i = 0;
	for (NSString *key in keys) {
		NSAttributedString *keyString = [[NSAttributedString alloc] initWithString:key attributes:keyAttributes];
		[colorString appendAttributedString:keyString];
		NSAttributedString *valueString = [[NSAttributedString alloc] initWithString:[values[i] description] attributes:valueAttributes];
		[colorString appendAttributedString:valueString];
		[colorString appendAttributedString:[[NSAttributedString alloc] initWithString:@" "]];
		i++;
	}
	self.colorLabel.attributedText = colorString;
}

// Increase touchable area of sliders
- (UIView *)hitTest:(CGPoint)point withEvent:(UIEvent *)event {
	UIView *hitView = [super hitTest:point withEvent:event];
	if (CGRectContainsPoint(CGRectInset(self.saturationSlider.frame, -20.0, -5.0), point)) {
		hitView = self.saturationSlider;
	} else if (CGRectContainsPoint(CGRectInset(self.brightnessSlider.frame, -20.0, -5.0), point)) {
		hitView = self.brightnessSlider;
	}
	return hitView;
}

- (void)layoutSubviews {
	[super layoutSubviews];
	
	CGRect contentRect = CGRectInset(self.bounds, PADDING, PADDING);
	CGFloat sliderHeight = 40.0;
	
	self.brightnessSlider.frame = CGRectMake(CGRectGetMinX(contentRect), CGRectGetMaxY(contentRect) - sliderHeight, CGRectGetWidth(contentRect), sliderHeight);
	self.saturationSlider.frame = CGRectMake(CGRectGetMinX(contentRect), CGRectGetMinY(self.brightnessSlider.frame) - PADDING - sliderHeight, CGRectGetWidth(contentRect), sliderHeight);
	
	CGFloat hueWheelSide = CGRectGetWidth(contentRect) - 40.0;
	self.hueWheel.frame = CGRectMake(CGRectGetMidX(contentRect) - hueWheelSide / 2.0, CGRectGetMinY(self.saturationSlider.frame) - PADDING - hueWheelSide, hueWheelSide, hueWheelSide);
	
	self.colorLabel.frame = CGRectMake(CGRectGetMinX(contentRect), CGRectGetMinY(contentRect), CGRectGetWidth(contentRect), CGRectGetMinY(self.hueWheel.frame) - CGRectGetMinY(contentRect));
    
    CGRect switchRect = CGRectMake(self.bounds.size.width/2 - 25, self.hueWheel.frame.origin.x + hueWheelSide , 20, 50);
    self.paintSwitch.frame = switchRect;
}

- (void) switchStateChanged
{
    if([self.paintSwitch isOn])
        [self.escNotifier switchStateDidChange:YES];
    else
        [self.escNotifier switchStateDidChange:NO];
}

@end
