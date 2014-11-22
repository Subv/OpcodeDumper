using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace OpcodeBruter
{
    /// <summary>
    /// Description of Utils.
    /// </summary>
    public static class Utils
    {
        public static unsafe List<uint> FindPattern(this byte[] Haystack, byte[] Needle, byte wildcard, uint begin = 0, uint end = 0)
        {
            List<uint> Indexes = new List<uint>();
            if (begin > end)
                return Indexes;

            fixed (byte* H = Haystack) fixed (byte* N = Needle)
            {
                uint i = 0;
                byte* hNext = H + begin;
                byte* hEnd  = H + (end == 0 ? Haystack.LongLength : end);
                for (; hNext < hEnd; ++i, ++hNext)
                {
                    bool Found = true;
                    for (byte* hInc = hNext, nInc = N, nEnd = N + Needle.LongLength; Found && nInc < nEnd; Found = *nInc == wildcard || *nInc == *hInc, ++nInc, ++hInc);
                    /*if (Found) { Indexes.Add(i); hNext += Needle.LongLength; }
                    else hNext++;*/
                    if (Found) Indexes.Add(i);
                }
                return Indexes;
            }
        }
    }
}
