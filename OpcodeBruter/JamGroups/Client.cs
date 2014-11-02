using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Bea;

namespace OpcodeBruter.JamGroups
{
    public class Client : JamDispatch
    {
        public override int JumpTableAddress
        {
            get { return 0x5DCFD0 - 0x400C00; }
        }

        public override int JumpTableSize
        {
            get { return 900; }
        }

        public override int IndirectJumpTableAddress
        {
            get { return 0; }
        }

        public override int IndirectJumpTableSize
        {
            get { return 0; }
        }

        public static bool Check(uint opcode)
        {
            return ((opcode - 1) & 0x88A) == 2 || ((opcode - 1) & 0x4A2) == 162;
        }

        public Client(FileStream wow) : base(wow)
        {
            
        }

        public override uint CalculateHandler(uint opcode)
        {
            var index = CalculateOffset(opcode);
            if (index >= JumpTableSize)
                return Offsets[898]; // Default case
            return Offsets[index];
        }

        public override int CalculateOffset(uint opcode)
        {
            var v5 = opcode - 1;
            if ( ((opcode - 1) & 0x88A) == 2 )
                return (int)(v5 & 1 | ((v5 & 4 | ((v5 & 0x70 | ((v5 & 0x700 | (v5 >> 1) & 0x7800) >> 1)) >> 1)) >> 1));
            else
                return (int)((v5 & 1 | ((v5 & 0x1C | ((v5 & 0x40 | ((v5 & 0x300 | (v5 >> 1) & 0x7C00) >> 1)) >> 1)) >> 1)) + 0x200);
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
                int result = BeaEngine.Disasm(disasm);
                if (result == (int)BeaConstants.SpecialInfo.UNKNOWN_OPCODE)
                    return new uint[2];
                
                disasmCount += result;

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
