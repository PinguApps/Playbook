// snippet:complete
#import "SurveyPermit.h"
@implementation SurveyPermit
- (nullable instancetype)initWithStationCode:(NSString *)stationCode
{
    if (![stationCode hasPrefix:@"SUR-"]) return nil;
    return [super init];
}
@end
// endsnippet:complete
