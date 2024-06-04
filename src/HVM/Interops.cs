using System.Runtime.InteropServices;

namespace HVM;

public static class Interops
{
    private const string DllName = "hvm_dotnet";
    
    [DllImport(DllName, EntryPoint = "book_parse")]
    [SuppressGCTransition]
    internal static extern unsafe void* BookParse([MarshalAs(UnmanagedType.LPUTF8Str)] string code, byte** errOut);
    
    [DllImport(DllName, EntryPoint = "free_cstring", CallingConvention = CallingConvention.Cdecl)]
    [SuppressGCTransition]
    internal static extern unsafe void* FreeCString(void* stringPtr);
    
    [DllImport(DllName, EntryPoint = "free_book", CallingConvention = CallingConvention.Cdecl)]
    [SuppressGCTransition]
    internal static extern unsafe void* FreeBook(void* bookPtr);
    
    [DllImport(DllName, EntryPoint = "book_evaluate", CallingConvention = CallingConvention.Cdecl)]
    [SuppressGCTransition]
    internal static extern unsafe EvaluationResultRaw* BookEvaluate(void* bookPtr, RuntimeTypes runtimeType, uint enableMemDump, byte** errOut);
    
    [DllImport(DllName, EntryPoint = "free_evaluation_result", CallingConvention = CallingConvention.Cdecl)]
    [SuppressGCTransition]
    internal static extern unsafe void FreeEvaluationResult(EvaluationResultRaw* resultPtr);
    
    [DllImport(DllName, EntryPoint = "book_serialize", CallingConvention = CallingConvention.Cdecl)]
    [SuppressGCTransition]
    internal static extern unsafe void* BookSerialize(void* bookPtr, byte** errOut);
    
    [DllImport(DllName, EntryPoint = "vec_get_length", CallingConvention = CallingConvention.Cdecl)]
    [SuppressGCTransition]
    internal static extern unsafe ulong VecGetLength(void* vecPtr);
    
    [DllImport(DllName, EntryPoint = "vec_copy", CallingConvention = CallingConvention.Cdecl)]
    [SuppressGCTransition]
    internal static extern unsafe ulong VecCopy(void* vecPtr, void* loc, ulong bufferSize);
    
    [DllImport(DllName, EntryPoint = "free_vec", CallingConvention = CallingConvention.Cdecl)]
    [SuppressGCTransition]
    internal static extern unsafe ulong FreeVec(void* vecPtr);
}