#import <Foundation/Foundation.h>
#import "ESCColorPickerView.h"
#import "ESCColorPickerModel.h"

@protocol ColorPickerDelegate

- (void) colorChangedTo:(NSString *)rgbHex;
- (void) stateChangedTo:(BOOL) state;

@end

@interface ESCColorPickerPresenter : NSObject

- (instancetype)init UNAVAILABLE_ATTRIBUTE;
- (instancetype)initWithView:(ESCColorPickerView *)view Model:(ESCColorPickerModel *)model andDelegate:(id<ColorPickerDelegate>)delegate;

@end
