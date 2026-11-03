// snippet:complete
using System.Runtime.InteropServices;
using System.Windows;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
#if NET11_0_OR_GREATER
        // Set once, before the first clipboard/data-object retrieval.
        AppContext.SetSwitch("Windows.Clipboard.ThrowExceptionsForGetAPIs", args.Contains("--errors"));
#endif
        // A real COM IDataObject failure, without replacing the user's clipboard.
        var data = new DataObject(new UnavailableTransfer());
        try
        {
            object? value = data.GetData(DataFormats.UnicodeText, autoConvert: false);
            Console.WriteLine(value is null ? "EMPTY" : "VALUE");
        }
        catch (ExternalException error)
        {
            Console.WriteLine($"ERROR {error.HResult:X8}");
        }
    }
}

[ComVisible(true), ClassInterface(ClassInterfaceType.None)]
internal sealed class UnavailableTransfer : System.Runtime.InteropServices.ComTypes.IDataObject
{
    public void GetData(ref System.Runtime.InteropServices.ComTypes.FORMATETC format, out System.Runtime.InteropServices.ComTypes.STGMEDIUM medium)
    { medium = default; throw new COMException("Transfer is unavailable", unchecked((int)0x80004005)); }
    public int QueryGetData(ref System.Runtime.InteropServices.ComTypes.FORMATETC format) => 0;
    public void GetDataHere(ref System.Runtime.InteropServices.ComTypes.FORMATETC format, ref System.Runtime.InteropServices.ComTypes.STGMEDIUM medium) => throw new NotSupportedException();
    public int GetCanonicalFormatEtc(ref System.Runtime.InteropServices.ComTypes.FORMATETC formatIn, out System.Runtime.InteropServices.ComTypes.FORMATETC formatOut) { formatOut = formatIn; return 1; }
    public void SetData(ref System.Runtime.InteropServices.ComTypes.FORMATETC formatIn, ref System.Runtime.InteropServices.ComTypes.STGMEDIUM medium, bool release) => throw new NotSupportedException();
    public System.Runtime.InteropServices.ComTypes.IEnumFORMATETC EnumFormatEtc(System.Runtime.InteropServices.ComTypes.DATADIR direction) => throw new NotSupportedException();
    public int DAdvise(ref System.Runtime.InteropServices.ComTypes.FORMATETC pFormatetc, System.Runtime.InteropServices.ComTypes.ADVF advf, System.Runtime.InteropServices.ComTypes.IAdviseSink adviseSink, out int connection) { connection = 0; return unchecked((int)0x80040003); }
    public void DUnadvise(int connection) => throw new NotSupportedException();
    public int EnumDAdvise(out System.Runtime.InteropServices.ComTypes.IEnumSTATDATA? enumAdvise) { enumAdvise = null; return unchecked((int)0x80040003); }
}
// endsnippet:complete
