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

        public byte ReadByte(uint offset)
        {
            return Marshal.ReadByte(Ptr, (int)offset);
        }

        public short ReadWord(uint offset)
        {
            return Marshal.ReadInt16(Ptr, (int)offset);
        }

        public unsafe byte[] ReadBytes(uint offset, int count)
        {
            byte[] managedArray = new byte[count];
            for (var i = 0; i < count; ++i)
                managedArray[i] = *(byte*)(Ptr + i);
            return managedArray;
        }

        public uint ReadDword(uint offset)
        {
            return (uint)Marshal.ReadInt32(Ptr, (int)offset);
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
