
using System;

public static class Utils
{
    public static readonly Random rng = new Random();

    public static float RandomFloat(float minimum, float maximum)
        => (float)rng.NextDouble() * (maximum - minimum) + minimum;

}
