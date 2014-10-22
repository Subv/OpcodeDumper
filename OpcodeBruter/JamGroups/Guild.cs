using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace OpcodeBruter.JamGroups
{
    public class Guild : JamDispatch
    {
        public override int JumpTableAddress
        {
            get { return 0x5F0B35 - 0x400000 - 0xC00; }
        }

        public override int JumpTableSize
        {
            get { return 100; }
        }

        public override int IndirectJumpTableAddress
        {
            get { return 0x5F0C35; }
        }

        public override int IndirectJumpTableSize
        {
            get { return 100; }
        }

        public Guild(FileStream wow) : base(wow)
        {

        }
        public override uint CalculateHandler(uint opcode)
        {
            return Offsets[CalculateOffset(opcode)];
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

            eax = (int)IndirectJumpTable[eax];
            
            return eax;
        }
    }
}
