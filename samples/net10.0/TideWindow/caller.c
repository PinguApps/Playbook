// snippet:caller
#include <dlfcn.h>
#include <stdio.h>

int main(int argc, char **argv) {
    if (argc != 2) return 2;
    void *library = dlopen(argv[1], RTLD_NOW | RTLD_LOCAL);
    if (!library) { fprintf(stderr, "%s\n", dlerror()); return 3; }
    int (*minutes)(int, int) = (int (*)(int, int))dlsym(library, "tide_window_minutes");
    if (!minutes || minutes(340, 300) != 80 || minutes(290, 300) != 0) return 4;
    puts("PASS: tide window 80; closed window 0");
    /* Native AOT shared-library unloading is unsupported. Keep loaded until exit. */
    return 0;
}
// endsnippet:caller
