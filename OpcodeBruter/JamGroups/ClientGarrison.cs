using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace OpcodeBruter.JamGroups
{
    public class ClientGarrison : JamDispatch
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
            return (v1 & 0xA8E) == 8
                || (v1 & 0x17BA) == 4480
                || (v1 & 0xEBE) == 2182
                || (v1 & 0xEBA) == 2194
                || (v1 & 0xEAA) == 2186;

        }

        public ClientGarrison(FileStream wow)
            : base(wow)
        {

        }

        public override uint CalculateHandler(uint opcode)
        {
            return 0x0CB9842; // This one doesn't have a jump table
        }

        public override int CalculateOffset(uint opcode)
        {
            var v6 = opcode - 1;
            var v5 = v6;
            var v7 = v6;
            var v8 = 0;
            if ((v6 & 0xA8E) == 8)
            {
                v8 = (int)(v6 & 1 | ((v6 & 0x70 | ((v6 & 0x100 | ((v7 & 0x400 | (v5 >> 1) & 0x7800) >> 1)) >> 1)) >> 3));
            }
            else
            {
                if ((v5 & 0x17BA) == 4480)
                {
                    v8 = (int)((v6 & 1 | ((v6 & 4 | ((v6 & 0x40 | ((v7 & 0x800 | (v6 >> 1) & 0x7000) >> 4)) >> 3)) >> 1)) + 128);
                }
                else
                {
                    if ((v6 & 0xEBE) == 2182)
                    {
                        v8 = (int)((v6 & 1 | ((v6 & 0x40 | ((v7 & 0x100 | (v6 >> 3) & 0x1E00) >> 1)) >> 5)) + 144);
                    }
                    else
                    {
                        var v9 = v7 & 0x100;
                        if ((v6 & 0xEBA) == 2194)
                            v8 = (int)((v6 & 1 | ((v6 & 4 | ((v6 & 0x40 | ((v9 | (v6 >> 3) & 0x1E00) >> 1)) >> 3)) >> 1)) + 160);
                        else
                            v8 = (int)((v6 & 1 | ((v6 & 4 | ((v6 & 0x10 | ((v6 & 0x40 | ((v9 | (v6 >> 3) & 0x1E00) >> 1)) >> 1)) >> 1)) >> 1)) + 192);
                    }
                }
            }

            return v8;
        }
    }
}
