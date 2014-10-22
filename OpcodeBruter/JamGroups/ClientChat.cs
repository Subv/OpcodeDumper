using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace OpcodeBruter.JamGroups
{
    public class ClientChat : JamDispatch
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
            return ((opcode - 1) & 0x9AA) == 2056;

        }

        public ClientChat(FileStream wow)
            : base(wow)
        {

        }

        public override uint CalculateHandler(uint opcode)
        {
            return 0x0CB2F02; // This one doesn't have a jump table
        }

        public override int CalculateOffset(uint opcode)
        {
            var edx = opcode - 1;
            return (int)((edx & 1) | ((((((((((edx >> 1) & 0x7800) | (edx & 0x600)) >> 2) | (edx & 0x40)) >> 1) | (edx & 0x10)) >> 1) | (edx & 0x4)) >> 1));
        }
    }
}
