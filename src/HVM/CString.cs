using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace HVM;

[StructLayout(LayoutKind.Sequential)]
internal readonly unsafe ref struct RawCString
{
    public readonly byte* Ptr;

    public RawCString(byte* ptr)
    {
        Ptr = ptr;
    }

    public void Dispose()
    {
        if (Ptr == null) return;
        Interops.FreeCString(Ptr);
    }

    private static int GetLength(byte* ptr)
    {
        var count = 0;
        while (ptr[count++] != 0)
        {
            
        }

        return count - 1;
    }

    private static string ConstructString(byte* ptr)
    {
        if (ptr == null) return string.Empty;
        var length = GetLength(ptr);
        return Encoding.ASCII.GetString(MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(ptr), length));
    }

    public override string ToString() => ConstructString(Ptr);
}

internal unsafe ref struct CString
{
    private RawCString _raw;
    private string? _stringCache;

    public bool HasValue => _raw.Ptr != null;

    public CString(void* ptr)
    {
        _raw = new((byte*)ptr);
        _stringCache = null;
    }

    public override string ToString()
    {
        _stringCache ??= _raw.ToString();
        return _stringCache;
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }

    public void Dispose()
    {
        _raw.Dispose();
        _raw = new();
    }
}