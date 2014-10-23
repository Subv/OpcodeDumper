using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace OpcodeBruter.JamGroups
{
    public class ClientSocial : JamDispatch
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
            return ((opcode - 1) & 0x1C86) == 1156;
        }

        public ClientSocial(FileStream wow)
            : base(wow)
        {

        }

        public override uint CalculateHandler(uint opcode)
        {
            return 0x05E7E7B; // This one doesn't have a jump table
        }

        public override int CalculateOffset(uint opcode)
        {
            var v5 = (opcode - 1) & 1;
            return (int)(v5 | (((opcode - 1) & 0x78 | (((opcode - 1) & 0x300 | ((opcode - 1) >> 3) & 0x1C00) >> 1)) >> 2));
        }
    }
}
