#import <UIKit/UIKit.h>
#import "ESCObservable/ESCObservable.h"

@protocol ESCColorPickerViewObserver

- (void)hueDidChange:(CGFloat)hue;
- (void)saturationDidChange:(CGFloat)saturation;
- (void)brightnessDidChange:(CGFloat)brightness;
- (void)colorDescriptionTapped;
- (void)switchStateDidChange:(BOOL)state;

@end

@interface ESCColorPickerView : UIView<ESCObservable>

- (void)setHue:(CGFloat)hue saturation:(CGFloat)saturation brightness:(CGFloat)brightness;
- (void)setColorDescriptionKeys:(NSArray *)keys values:(NSArray *)values;

@end
