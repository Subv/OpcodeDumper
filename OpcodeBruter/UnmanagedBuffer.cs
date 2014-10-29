using System;
using System.Runtime.InteropServices;
using System.IO;

namespace OpcodeBruter
{
    public class UnmanagedBuffer
    {
        public readonly IntPtr Ptr = IntPtr.Zero;
        public readonly int Length = 0;

        public UnmanagedBuffer(byte[] data)
        {
            Ptr = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, Ptr, data.Length);
            Length = data.Length;
        }
        
        public static UnmanagedBuffer CreateFromFile(FileStream file)
        {
            return new UnmanagedBuffer(File.ReadAllBytes(file.Name));
        }

        ~UnmanagedBuffer()
        {
            if (Ptr != IntPtr.Zero)
                Marshal.FreeHGlobal(Ptr);
        }
    }
}
