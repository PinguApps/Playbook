// snippet:native-helper
// This Windows x64 helper deliberately needs no SDK headers or C runtime.
typedef unsigned __int64 size_t;
#define offsetof(type, field) ((size_t)&(((type *)0)->field))
int _fltused = 0;

typedef struct { unsigned char kind; double depth; int count; } sounding;
typedef union { double depth; long long code; } reading;
typedef struct { unsigned char prefix; sounding value; } sounding_alignment;
typedef struct { unsigned char prefix; reading value; } reading_alignment;

__declspec(dllexport) size_t metric(int index)
{
    switch (index) {
    case 0: return sizeof(sounding);
    case 1: return __alignof(sounding);
    case 2: return offsetof(sounding, kind);
    case 3: return offsetof(sounding, depth);
    case 4: return offsetof(sounding, count);
    case 5: return sizeof(reading);
    case 6: return __alignof(reading);
    case 7: return offsetof(reading, depth);
    case 8: return offsetof(reading, code);
    case 9: return offsetof(sounding_alignment, value);
    case 10: return offsetof(reading_alignment, value);
    default: return (size_t)-1;
    }
}

__declspec(dllexport) void revise(sounding *value)
{
    value->kind += 1;
    value->depth += 0.5;
    value->count += 2;
}
__declspec(dllexport) void revise_depth(reading *value) { value->depth += 1.25; }
__declspec(dllexport) void revise_code(reading *value) { value->code += 7; }
// endsnippet:native-helper
