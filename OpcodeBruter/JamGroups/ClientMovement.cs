using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace OpcodeBruter.JamGroups
{
    public class ClientMovement : JamDispatch
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
            var v2 = v1 & 0xEA2;
            var v3 = v1 & 0x1CC6;
            var v4 = v1 & 0x15B8;
            return v2 == 130
                || v2 == 2690
                || v3 == 1152
                || v3 == 1216
                || (v1 & 0x1682) == 1154
                || v4 == 5248
                || v4 == 5264
                || (v1 & 0x15A0) == 5504;

        }

        public ClientMovement(FileStream wow)
            : base(wow)
        {

        }

        public override uint CalculateHandler(uint opcode)
        {
            return 0x0CB3484; // This one doesn't have a jump table
        }

        public override int CalculateOffset(uint opcode)
        {
            var v6 = opcode - 1;
            var v5 = v6 & 0xEA2;
            var v7 = v6;
            var v8 = 0;
            if (v5 == 130)
            {
                v8 = (int)(v6 & 1 | ((v6 & 0x1C | ((v6 & 0x40 | ((v7 & 0x100 | (v6 >> 3) & 0x1E00) >> 1)) >> 1)) >> 1));
            }
            else
            {
                if (v5 == 2690)
                {
                    v8 = (int)((v6 & 1 | ((v6 & 0x1C | ((v6 & 0x40 | ((v7 & 0x100 | (v6 >> 3) & 0x1E00) >> 1)) >> 1)) >> 1)) + 128);
                }
                else
                {
                    if ((v6 & 0x1CC6) == 1152)
                    {
                        v8 = (int)((v6 & 1 | ((v6 & 0x38 | ((v7 & 0x300 | (v6 >> 3) & 0x1C00) >> 2)) >> 2)) + 256);
                    }
                    else
                    {
                        if ((v6 & 0x1CC6) == 1216)
                        {
                            v8 = (int)((v6 & 1 | ((v6 & 0x38 | ((v7 & 0x300 | (v6 >> 3) & 0x1C00) >> 2)) >> 2)) + 320);
                        }
                        else
                        {
                            if ((v6 & 0x1682) == 1154)
                            {
                                v8 = (int)((v6 & 1 | ((v6 & 0x7C | ((v6 & 0x100 | ((v7 & 0x800 | (v6 >> 1) & 0x7000) >> 2)) >> 1)) >> 1)) + 384);
                            }
                            else
                            {
                                var v9 = v7 & 0x800;
                                if ((v6 & 0x15B8) == 5248)
                                {
                                    v8 = (int)((v6 & 7 | ((v6 & 0x40 | ((v6 & 0x200 | ((v9 | (v6 >> 1) & 0x7000) >> 1)) >> 2)) >> 3)) + 640);
                                }
                                else
                                {
                                    if ((v6 & 0x15B8) == 5264)
                                        v8 = (int)((v6 & 7 | ((v6 & 0x40 | ((v6 & 0x200 | ((v9 | (v6 >> 1) & 0x7000) >> 1)) >> 2)) >> 3)) + 704);
                                    else
                                        v8 = (int)((v6 & 0x1F | ((v6 & 0x40 | ((v6 & 0x200 | ((v9 | (v6 >> 1) & 0x7000) >> 1)) >> 2)) >> 1)) + 768);
                                }
                            }
                        }
                    }
                }
            }

            return v8;
        }
    }
}
