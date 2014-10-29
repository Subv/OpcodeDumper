using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using Bea;
using System.Diagnostics;
using Microsoft

namespace OpcodeBruter.JamGroups
{
    public class ClientSpell : JamDispatch
    {
        public override int JumpTableAddress
        {
            get { return 0; }
        }

        public override int JumpTableSize
        {
            get { return 0; }
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
            var v1 = opcode - 1;
            return (v1 & 0xEA2) == 642 || (v1 & 0x1682) == 1666 || (v1 & 0x15A8) == 5256;
        }

        public ClientSpell(FileStream wow)
            : base(wow)
        {

        }

        public override uint CalculateHandler(uint opcode)
        {
            return 0x00CAB966; // This one doesn't have a jump table
        }

        public override int CalculateOffset(uint opcode)
        {
            var v8 = 0;
            var v6 = opcode - 1;
            var v5 = v6;
            var v7 = v6;
            if (((opcode - 1) & 0xEA2) == 642)
            {
                v8 = (int)(v6 & 1 | ((v6 & 0x1C | ((v6 & 0x40 | ((v7 & 0x100 | (v5 >> 3) & 0x1E00) >> 1)) >> 1)) >> 1));
            }
            else
            {
                var v9 = v7 & 0x800;
                if ((v5 & 0x1682) == 1666)
                    v8 = (int)((v6 & 1 | ((v6 & 0x7C | ((v6 & 0x100 | ((v9 | (v6 >> 1) & 0x7000) >> 2)) >> 1)) >> 1)) + 128);
                else
                    v8 = (int)((v6 & 7 | ((v6 & 0x10 | ((v6 & 0x40 | ((v6 & 0x200 | ((v9 | (v6 >> 1) & 0x7000) >> 1)) >> 2)) >> 1)) >> 1)) + 384);
            }
            return v8;
        }

        public override uint[] CalculateJamFunctionOffsets(uint opcode)
        {
            // We are passed the opcode here.
            // TODO: This is disassembling the function, aka doing what IDA already does.
            // We need to find a way to execute it (see Utils.AsDelegate(...) *cough*)
            // And INTERCEPT whenever a CALL opcode is emitted. Right now, invoking
            // the delegate causes an SEHException, which is fine and all, but
            // doesn't let us find out the offset of the call.
            // Basically we need MDbgEngine.
            var functionLength = 0x00CAC5E8 - 0x00CAB966;
            var disasm = new Disasm();
            disasm.EIP = new IntPtr(0x00CAB966 + wowDisasm.Ptr.ToInt64() - 0x400C00);
            var bytesTally = 0;

            while (bytesTally < functionLength)
            {
                int result = BeaEngine.Disasm(disasm);
                if (result == (int)BeaConstants.SpecialInfo.UNKNOWN_OPCODE)
                    return new uint[2];

                //if (Program.Debug)
                Console.WriteLine("0x{0:X8} {1}", (disasm.EIP.ToInt64() - wowDisasm.Ptr.ToInt64() + 0x400C00), disasm.CompleteInstr);

                bytesTally += result;
                disasm.EIP = new IntPtr(disasm.EIP.ToInt64() + result);
            }
            return new uint[2];
        }
    }
}
