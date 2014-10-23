using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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
    }
}
