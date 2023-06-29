namespace Play.Common;

using System;
using System.Threading;

public static class GeneratorOperationId
{
    private static readonly char[] Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUV".ToCharArray();
    private static long _lastId = DateTime.UtcNow.Ticks;
    private static readonly ThreadLocal<char[]> Buffer = new ThreadLocal<char[]>(() => new char[13]);
        
    public static string Generate() => InternalGenerateOperationId(Interlocked.Increment(ref _lastId));

    private static string InternalGenerateOperationId(long id)
    {
        // SEE: https://nimaara.com/2018/10/10/generating-ids-in-csharp.html

        var buffer = Buffer.Value;

        buffer[0] = Chars[(int)(id >> 60) & 31];
        buffer[1] = Chars[(int)(id >> 55) & 31];
        buffer[2] = Chars[(int)(id >> 50) & 31];
        buffer[3] = Chars[(int)(id >> 45) & 31];
        buffer[4] = Chars[(int)(id >> 40) & 31];
        buffer[5] = Chars[(int)(id >> 35) & 31];
        buffer[6] = Chars[(int)(id >> 30) & 31];
        buffer[7] = Chars[(int)(id >> 25) & 31];
        buffer[8] = Chars[(int)(id >> 20) & 31];
        buffer[9] = Chars[(int)(id >> 15) & 31];
        buffer[10] = Chars[(int)(id >> 10) & 31];
        buffer[11] = Chars[(int)(id >> 5) & 31];
        buffer[12] = Chars[(int)id & 31];

        return new string(buffer, 0, buffer.Length);
    }
}