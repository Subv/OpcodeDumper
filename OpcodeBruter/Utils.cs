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
        const uint PAGE_EXECUTE_READWRITE = 0x40;
        const uint MEM_COMMIT = 0x1000;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        public delegate byte ByteReturner(int _this, int a2, int a3, ushort opcode, int a5);
        public static ByteReturner AsDelegate(this byte[] body)
        {
            IntPtr buf = VirtualAlloc(IntPtr.Zero, (uint)body.Length, MEM_COMMIT, PAGE_EXECUTE_READWRITE);
            Marshal.Copy(body, 0, buf, body.Length);

            return (ByteReturner)Marshal.GetDelegateForFunctionPointer(buf, typeof(ByteReturner));
        }

        public static unsafe List<uint> FindPattern(this byte[] Haystack, byte[] Needle, byte wildcard)
        {
            List<uint> Indexes = new List<uint>();
            fixed (byte* H = Haystack) fixed (byte* N = Needle)
            {
                uint i = 0;
                for (byte* hNext = H, hEnd = H + Haystack.LongLength; hNext < hEnd; ++i, ++hNext)
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
