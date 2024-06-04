using System.Runtime.CompilerServices;

namespace HVM;

public unsafe class Book : IDisposable
{
    private void* _ptr;

    private Book(void* ptr)
    {
        _ptr = ptr;
    }

    [Obsolete("Use Parse() instead")]
    public Book()
    {
        throw new NotSupportedException("Use Parse() instead");
    }

    public static Book Parse(string code)
    {
        byte* errPtr = null;
        var bookPtr = Interops.BookParse(code, &errPtr);
        using var errString = new CString(errPtr);
        if (errString.HasValue)
        {
            throw new InteropException(errString.ToString());
        }

        return new Book(bookPtr);
    }

    public EvaluationResult Evaluate(RuntimeTypes runtimeType = RuntimeTypes.Rust, bool enableMemDump = false)
    {
        byte* errPtr = null;
        var resultPtr = Interops.BookEvaluate(_ptr, runtimeType, enableMemDump ? 1U : 0U, &errPtr);
        try
        {
            using var errString = new CString(errPtr);
            if (errString.HasValue)
            {
                throw new InteropException(errString.ToString());
            }

            return new EvaluationResult(resultPtr);
        }
        finally
        {
            if (resultPtr != null)
                Interops.FreeEvaluationResult(resultPtr);
        }
    }

    public bool Serialize(Span<byte> buffer, out ulong written)
    {
        byte* errPtr = null;
        var vecPtr = Interops.BookSerialize(_ptr, &errPtr);
        try
        {
            using var errString = new CString(errPtr);
            if (errString.HasValue)
            {
                throw new InteropException(errString.ToString());
            }

            var vecLength = Interops.VecGetLength(vecPtr);
            var targetBufferLength = (ulong)buffer.Length;
            if (vecLength > targetBufferLength)
            {
                written = 0;
                return false;
            }

            var offset = Unsafe.AsPointer(ref buffer[0]);
            Interops.VecCopy(vecPtr, offset, targetBufferLength);
            written = vecLength;
            return true;
        }
        finally
        {
            if (vecPtr != null)
                Interops.FreeVec(vecPtr);
        }
    }
    
    public byte[] Serialize()
    {
        byte* errPtr = null;
        var vecPtr = Interops.BookSerialize(_ptr, &errPtr);
        try
        {
            using var errString = new CString(errPtr);
            if (errString.HasValue)
            {
                throw new InteropException(errString.ToString());
            }

            var vecLength = Interops.VecGetLength(vecPtr);
            var buffer = new byte[vecLength];

            var offset = Unsafe.AsPointer(ref buffer[0]);
            Interops.VecCopy(vecPtr, offset, vecLength);
            return buffer;
        }
        finally
        {
            if (vecPtr != null)
                Interops.FreeVec(vecPtr);
        }
    }

    private void CleanUp()
    {
        if (_ptr == null) return;
        Interops.FreeBook(_ptr);
        _ptr = null;
    }
    
    public void Dispose()
    {
        CleanUp();
        GC.SuppressFinalize(this);
    }

    ~Book()
    {
        CleanUp();
    }
}