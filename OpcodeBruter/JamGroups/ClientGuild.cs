using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Bea;

namespace OpcodeBruter.JamGroups
{
    public class ClientGuild : JamDispatch
    {
        public override int JumpTableAddress
        {
            get { return 0x5F0B35 - 0x400000 - 0xC00; }
        }

        public override int JumpTableSize
        {
            get { return 64; }
        }

        public override int IndirectJumpTableAddress
        {
            get { return 0x5F0C35 - 0x400000 - 0xC00; }
        }

        public override int IndirectJumpTableSize
        {
            get { return 100; }
        }

        public static bool Check(uint opcode)
        {
            return ((opcode - 1) & 0xA8E) == 520;
        }

        public ClientGuild(FileStream wow) : base(wow)
        {

        }
        public override uint CalculateHandler(uint opcode)
        {
            var index = CalculateOffset(opcode);
            return Offsets[index];
        }

        public override int CalculateOffset(uint opcode)
        {
            int edx = (int)(opcode - 1);
            int eax = edx;
            eax >>= 1;
            eax &= 0x7800;
            var esi = edx;
            esi &= 0x400;
            eax |= esi;
            eax >>= 1;

            esi = edx;
            esi &= 0x100;
            eax |= esi;
            eax >>= 1;

            esi = edx;
            esi &= 0x70;
            eax |= esi;
            eax >>= 3;

            edx &= 1;
            eax |= edx;

            if (eax >= IndirectJumpTableSize)
                return 3; // Default case

            eax = (int)IndirectJumpTable[eax];
            
            return eax;
        }

        public override uint[] CalculateJamFunctionOffsets(uint address)
        {
            var callStrings = new List<uint>();

            var disasm = new Disasm();
            disasm.EIP = new IntPtr(address + wowDisasm.Ptr.ToInt64() - 0x400C00);
            var callCount = 0;
            var disasmCount = 0;

            // Keep disassembling until we either catch two CALL opcodes,
            // or disassemble more than 32 bytes (size of the case in jump table)
            while (callCount < 2 && disasmCount < 32)
            {
                ++disasmCount;
                int result = BeaEngine.Disasm(disasm);
                if (result == (int)BeaConstants.SpecialInfo.UNKNOWN_OPCODE)
                    return new uint[2];

                if (Program.Debug)
                    Console.WriteLine("0x{0:X8} {1}", (disasm.EIP.ToInt64() - wowDisasm.Ptr.ToInt64() + 0x400C00), disasm.CompleteInstr);

                if (disasm.CompleteInstr.IndexOf("call") != -1)
                {
                    callStrings.Add((uint)(disasm.Instruction.AddrValue - (ulong)wowDisasm.Ptr.ToInt64() + 0x400C00));
                    ++callCount;
                }
                disasm.EIP = new IntPtr(disasm.EIP.ToInt64() + result);
            }

            // JAM Parser is the first one.
            if (callStrings.Count == 0)
                return new uint[2];
            return callStrings.ToArray();
        }
    }
}
