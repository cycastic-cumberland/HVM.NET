namespace HVM.Experiments;

public static class Program
{
    public static void Main()
    {
        using var book = Book.Parse(Constants.Fib);

        var result = book.Evaluate(RuntimeTypes.C);
        Console.WriteLine(result.ToString());
    }
}