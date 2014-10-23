using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace OpcodeBruter.JamGroups
{
    public class ClientQuest : JamDispatch
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
            return ((opcode - 1) & 0x1482) == 128;
        }

        public ClientQuest(FileStream wow)
            : base(wow)
        {

        }

        public override uint CalculateHandler(uint opcode)
        {
            return 0x05EF996; // This one doesn't have a jump table
        }

        public override int CalculateOffset(uint opcode)
        {
            var edx = opcode;
            var eax = edx;
            eax >>= 1;
            eax &= 0x7000;
            var esi = edx & 0x800;
            eax |= esi;
            eax >>= 1;

            esi = edx & 0x300;
            eax |= esi;
            esi = edx;
            eax >>= 1;

            esi &= 0x7C;
            eax |= esi;

            edx &= 1;
            eax >>= 1;
            eax |= edx;

            return (int)eax;
        }
    }
}
