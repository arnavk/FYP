#import "ESCColorPickerPresenter.h"

@interface ESCColorPickerPresenter()<ESCColorPickerModelObserver, ESCColorPickerViewObserver>

@property (nonatomic) ESCColorPickerModel *model;
@property (nonatomic) ESCColorPickerView *view;
@property (nonatomic, weak) id<ColorPickerDelegate> delegate;

@end

@implementation ESCColorPickerPresenter

- (instancetype)initWithView:(ESCColorPickerView *)view Model:(ESCColorPickerModel *)model andDelegate:(id<ColorPickerDelegate>)delegate {
    if (self = [super init]) {
		self.model = model;
		[self.model escAddObserver:self];
		
		self.view = view;
		[self.view escAddObserver:self];
		
		[self colorDidChange];
		[self.view setColorDescriptionKeys:self.model.colorDescriptionKeys values:self.model.colorDescriptionValues];
        
        self.delegate = delegate;
    }
    return self;
}

- (void)colorDidChange {
	[self.view setHue:self.model.hue
		   saturation:self.model.saturation
		   brightness:self.model.brightness];
    UIColor *hsbColor = [UIColor colorWithHue:self.model.hue saturation:self.model.saturation brightness:self.model.brightness alpha:1.0];
    CGFloat red, green, blue;
    [hsbColor getRed:&red green:&green blue:&blue alpha:NULL];
    NSString *color = [NSString stringWithFormat:@"%02lX%02lX%02lX", (unsigned long)(red * 255), (unsigned long)(green * 255), (unsigned long)(blue * 255)];
    [self.delegate colorChangedTo:color];
    
}

- (void)hueDidChange:(CGFloat)hue {
	self.model.hue = hue;
}

- (void)saturationDidChange:(CGFloat)saturation {
	self.model.saturation = saturation;
}

- (void)brightnessDidChange:(CGFloat)brightness {
	self.model.brightness = brightness;
}

- (void)colorDescriptionDidChangeKeys:(NSArray *)keys values:(NSArray *)values {
	[self.view setColorDescriptionKeys:keys values:values];
}

- (void)colorDescriptionTapped {
	self.model.descriptionFormat = (self.model.descriptionFormat + 1) % 3;
}

- (void)switchStateDidChange:(BOOL)state
{
    [self.delegate stateChangedTo:state];
}

@end
