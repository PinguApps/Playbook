// snippet:native-consumer
// Windows x64 IUnknown vtable: HRESULT accessors, UTF-16 and caller-owned outputs.
typedef int (__stdcall *get_int)(void*, int*);
typedef int (__stdcall *set_int)(void*, int);
typedef int (__stdcall *get_text)(void*, unsigned short**);
typedef int (__stdcall *set_text)(void*, const unsigned short*);
__declspec(dllexport) int exercise(void* object, void (__stdcall *release_text)(void*))
{
    void** slots = *(void***)object;
    int reading = 0;
    unsigned short* text = 0;
    const unsigned short replacement[] = {81,117,97,121,32,937,0};
    if (((get_int)slots[3])(object, &reading) != 0 || reading != 12) return 1;
    if (((set_int)slots[4])(object, 29) != 0) return 2;
    if (((get_text)slots[5])(object, &text) != 0 || !text) return 3;
    int matches = text[0] == 77 && text[4] == 32 && text[10] == 110 && text[11] == 0;
    release_text(text);
    if (!matches) return 4;
    if (((set_text)slots[6])(object, replacement) != 0) return 5;
    if (((get_text)slots[5])(object, &text) != 0 || !text) return 6;
    matches = text[0] == 81 && text[5] == 937 && text[6] == 0;
    release_text(text);
    return matches ? 0 : 7;
}
// endsnippet:native-consumer
