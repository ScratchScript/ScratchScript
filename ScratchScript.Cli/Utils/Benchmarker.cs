using System.Diagnostics;

namespace ScratchScript.Cli.Utils;

public static class Benchmarker
{
    public static long Measure(Action action)
    {
        var stopwatch = Stopwatch.StartNew();
        action();
        return stopwatch.ElapsedMilliseconds;
    }

    public static (T, long) Measure<T>(Func<T> action)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = action();
        return (result, stopwatch.ElapsedMilliseconds);
    }
}