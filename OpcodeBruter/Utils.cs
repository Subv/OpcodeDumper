using System;
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
    }
}
