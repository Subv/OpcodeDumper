using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace OpcodeBruter.JamGroups
{
    public class ClientLFG : JamDispatch
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
            return ((opcode - 1) & 0x8DA) == 0;

        }

        public ClientLFG(FileStream wow)
            : base(wow)
        {

        }

        public override uint CalculateHandler(uint opcode)
        {
            return 0x00CBA49A; // This one doesn't have a jump table
        }

        public override int CalculateOffset(uint opcode)
        {
            return (int)((opcode - 1) & 1 | (((opcode - 1) & 4 | (((opcode - 1) & 0x20 | (((opcode - 1) & 0x700 | ((opcode - 1) >> 1) & 0x7800) >> 2)) >> 2)) >> 1));
        }
    }
}
