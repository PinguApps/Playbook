// snippet:native
// The managed boundary accepts only 0..10000, so this arithmetic cannot overflow.
int shore_convert(int raw)
{
    return (raw * 7 + 5) / 10;
}
// endsnippet:native
