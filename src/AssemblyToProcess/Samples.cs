namespace AssemblyToProcess;

public static class Samples
{
    public const int Answer = 42;

    static readonly int[] primes = [2, 3, 5, 7, 11, 13, 17, 19];

    public static int Prime(int index) =>
        primes[index];

    public static string Describe(int value)
    {
        switch (value)
        {
            case 0:
                return "zero";
            case 1:
                return "one";
            case 2:
                return "two";
            default:
                return "many";
        }
    }

    public static int Parse(string text)
    {
        try
        {
            return int.Parse(text);
        }
        catch (FormatException)
        {
            return -1;
        }
        finally
        {
            Console.WriteLine("parsed");
        }
    }

    public static T First<T>(IEnumerable<T> items)
        where T : class
    {
        foreach (var item in items)
        {
            return item;
        }

        throw new InvalidOperationException();
    }

    public static Func<int, int> Adder(int amount) =>
        _ => _ + amount;

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Use Describe", DiagnosticId = "SAMPLE1")]
    public static string Legacy(int value = Answer) =>
        Describe(value);
}

public struct Point(int x, int y)
{
    public int X { get; } = x;
    public int Y { get; } = y;

    public class Nested;
}
