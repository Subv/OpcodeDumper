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
        
        public UnmanagedBuffer Clone()
        {
            return new UnmanagedBuffer(ReadBytes(0, Length));
        }
        
        public byte ReadByte(int offset)
        {
            return Marshal.ReadByte(Ptr, offset - 0x400C00);
        }
        
        public byte[] ReadBytes(int offset, int count)
        {
            byte[] managedArray = new byte[count];
            Marshal.Copy(Ptr, managedArray, offset - 0x400C00, count);
            return managedArray;
        }
        
        public void ReplaceByte(int offset, byte value)
        {
            byte[] data = ReadBytes(0, Length);
            data[offset] = value;
            Marshal.Copy(data, 0, Ptr, data.Length);
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
