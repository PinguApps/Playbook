// snippet:loader
// Windows x64, no CRT or SDK headers. The runner supplies the working directory.
__declspec(dllimport) void* __stdcall LoadLibraryA(const char*);
__declspec(dllimport) void* __stdcall GetProcAddress(void*, const char*);
__declspec(dllimport) int __stdcall FreeLibrary(void*);
__declspec(dllimport) void __stdcall ExitProcess(unsigned int);
typedef int (*depth_fn)(int);
void mainCRTStartup(void)
{
    void* library = LoadLibraryA("TernExports.dll");
    if (!library) ExitProcess(10);
    depth_fn depth = (depth_fn)GetProcAddress(library, "tern_depth");
    if (!GetProcAddress(library, "tern_root")) ExitProcess(11);
    if (GetProcAddress(library, "Unnamed")) ExitProcess(12);
#if EXPECT_DEPTH
    if (!depth || depth(35) != 42) ExitProcess(13);
#else
    if (depth) ExitProcess(14);
#endif
    FreeLibrary(library);
    ExitProcess(0);
}
// endsnippet:loader
