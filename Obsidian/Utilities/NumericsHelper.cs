using System.Runtime.CompilerServices;

namespace Obsidian.Utilities;

public static class NumericsHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LongToInts(long l, out int a, out int b)
    {
        a = (int)(l & uint.MaxValue);
        b = (int)(l >> 32);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long IntsToLong(int a, int b) => ((long)b << 32) | (uint)a;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Modulo(int x, int mod) => (x % mod + mod) % mod;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DegreesDiff(float a, float b) => WrapDegrees(a - b);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float WrapDegrees(float a)
    {
        var a1 = a % 360.0f;
        if (a1 >= 180.0f)
        {
            a1 -= 360.0f;
        }
        if (a1 < -180.0f)
        {
            a1 += 360.0f;
        }
        return a1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double DegreesDiff(double a, double b) => WrapDegrees(a - b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double WrapDegrees(double a)
    {
        var a1 = a % 360.0D;
        if (a1 >= 180.0D)
        {
            a1 -= 360.0D;
        }
        if (a1 < -180.0D)
        {
            a1 += 360.0D;
        }
        return a1;
    }
}
