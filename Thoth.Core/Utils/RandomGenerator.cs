using System;
using System.Threading;

namespace Thoth.Core.Utils;

internal static class RandomGenerator
{
    private static readonly Random Global = new();

    private static readonly ThreadLocal<Random> Rnd = new((Func<Random>)(() =>
    {
        int Seed;
        lock (Global)
        {
            Seed = Global.Next();
        }

        return new Random(Seed);
    }));

    public static int Next(int minValue = 0, int maxValue = int.MaxValue)
    {
        return Rnd.Value!.Next(minValue, maxValue);
    }

    public static double NextDouble()
    {
        return Rnd.Value!.NextDouble();
    }
}