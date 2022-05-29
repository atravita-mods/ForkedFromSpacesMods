using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonAssets.Framework;
internal static class JAUtils
{
    /// <summary>
    /// Given a '/'-deliminated data string, get the name as a ReadOnlySpan[char].
    /// </summary>
    /// <param name="data">Data string from the game.</param>
    /// <returns>The name.</returns>
    /// <remarks>Name is always the first field. Favoring spans for now, but maybe building a lookup dictionary in the opposite direction would be more performant.</remarks>
    internal static ReadOnlySpan<char> GetNameFrom(string data)
    {
        int index = data.IndexOf('/');
        if (index > 0)
            return data.AsSpan()[..index];
        return new ReadOnlySpan<char>();
    }

    internal static bool AreSpansEqual(this ReadOnlySpan<char> thisSpan, ReadOnlySpan<char> thatSpan)
    {
        if (thisSpan.Length != thatSpan.Length)
            return false;
        for (int i = 0; i < thisSpan.Length; i++)
            if (thisSpan[i] != thatSpan[i])
                return false;
        return true;
    }

    internal static bool AreSpansEqual(this ReadOnlySpan<char> thisSpan, string thatstring)
        => thisSpan.AreSpansEqual(thatstring.AsSpan());
}
