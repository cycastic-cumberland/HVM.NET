using System.Runtime.InteropServices;

namespace HVM;

[StructLayout(LayoutKind.Sequential)]
internal readonly ref struct EvaluationResultRaw
{
    public readonly ulong Iterations;
    public readonly double Duration;
    public readonly RawCString Result;
    public readonly RawCString MemDump;
    public readonly nuint Deallocator;
}

public readonly struct EvaluationResult
{
    public ulong Iterations { get; }
    public TimeSpan Duration { get; }
    public double IterationsPerSecond => Iterations / Duration.TotalSeconds;
    public string Result { get; }
    public string MemDump { get; }

    internal unsafe EvaluationResult(EvaluationResultRaw* raw)
    {
        Iterations = raw->Iterations;
        Duration = TimeSpan.FromSeconds(raw->Duration);
        Result = raw->Result.ToString();
        MemDump = raw->MemDump.ToString();
    }

    public override string ToString()
    {
        return string.Create(null, stackalloc char[256], $"{{ Iterations = {Iterations}, Duration = {Duration}, IPS = {IterationsPerSecond}, Result = {Result}, MemDump = {(string.IsNullOrEmpty(MemDump) ? "<empty>" : $"\n{MemDump}")} }}");
    }
}