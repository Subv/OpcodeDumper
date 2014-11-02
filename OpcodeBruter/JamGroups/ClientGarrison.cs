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
        
        public override uint[] CalculateJamFunctionOffsets(uint opcode)
        {
            #region Imported from Hex-Rays
            uint v5; // edx@1
            uint v6; // eax@1
            uint v7; // di@1
            uint v8; // eax@2
            uint v9; // edi@7
            uint v10; // eax@16
            uint v11; // eax@17
            uint v12; // eax@18
            uint v13; // eax@19
            uint v14; // eax@20
            uint v16; // eax@30
            uint v17; // eax@31
            uint v18; // eax@32
            uint v19; // eax@33
            uint v20; // eax@45
            uint v21; // eax@46
            uint v22; // eax@47
            uint v23; // eax@48
            uint v24; // eax@49
            uint v25; // eax@74
            uint v26; // eax@75
            uint v27; // eax@76
            uint v28; // eax@77
            uint v29; // eax@78
            uint v30; // eax@87
            uint v31; // eax@88
            uint v32; // eax@89
            uint v33; // eax@90
            uint v34; // eax@100
            uint v35; // eax@101
            uint v36; // eax@102
            uint v37; // eax@103
            uint v38; // eax@111
            uint v39; // eax@112
            uint v40; // eax@113
            uint v41; // eax@114

            v5 = opcode - 1;
            v6 = opcode - 1;
            v7 = opcode - 1;
            if ( ((opcode - 1) & 0xA8E) == 8 )
            {
                v8 = v5 & 1 | ((v5 & 0x70 | ((v5 & 0x100 | ((v7 & 0x400 | (v6 >> 1) & 0x7800) >> 1)) >> 1)) >> 3);
            }
            else
            {
                if ( (v6 & 0x17BA) == 4480 )
                {
                    v8 = (v5 & 1 | ((v5 & 4 | ((v5 & 0x40 | ((v7 & 0x800 | (v5 >> 1) & 0x7000) >> 4)) >> 3)) >> 1)) + 128;
                }
                else
                {
                    if ( (v5 & 0xEBE) == 2182 )
                    {
                        v8 = (v5 & 1 | ((v5 & 0x40 | ((v7 & 0x100 | (v5 >> 3) & 0x1E00) >> 1)) >> 5)) + 144;
                    }
                    else
                    {
                        v9 = v7 & 0x100;
                        if ( (v5 & 0xEBA) == 2194 )
                            v8 = (v5 & 1 | ((v5 & 4 | ((v5 & 0x40 | ((v9 | (v5 >> 3) & 0x1E00) >> 1)) >> 3)) >> 1)) + 160;
                        else
                            v8 = (v5 & 1 | ((v5 & 4 | ((v5 & 0x10 | ((v5 & 0x40 | ((v9 | (v5 >> 3) & 0x1E00) >> 1)) >> 1)) >> 1)) >> 1))
                                + 192;
                    }
                }
            }
            if ( v8 > 106 )
            {
                if ( v8 > 158 )
                {
                    if ( v8 <= 181 )
                    {
                        if ( v8 == 181 )
                        {
                            return new uint[] { 0xCC0046, 0xCBFA40 };
                        }
                        v34 = v8 - 159;
                        if ( 0 == v34 )
                        {
                            return new uint[] { 0xCC06BB, 0xCBFB2E };
                        }
                        v35 = v34 - 1;
                        if ( 0 == v35 )
                        {
                            return new uint[] { 0x5F482C, 0xCBF6BE };
                        }
                        v36 = v35 - 1;
                        if ( 0 == v36 )
                        {
                            return new uint[] { 0xCBFC4C, 0xCBF7B7 };
                        }
                        v37 = v36 - 6;
                        if ( 0 == v37 )
                        {
                            return new uint[] { 0x60582F, 0xCBF718 };
                        }
                        if ( v37 == 12 )
                        {
                            return new uint[] { 0xCC0E68, 0xCBF813 };
                        }
                        return new uint[2];
                    }
                    v38 = v8 - 182;
                    if ( 0 == v38 )
                    {
                        return new uint[] { 0x5FDCF5, 0xCBFAB0 };
                    }
                    v39 = v38 - 1;
                    if ( 0 == v39 )
                    {
                        return new uint[] { 0x600BEA, 0xCBF6CC };
                    }
                    v40 = v39 - 4;
                    if ( 0 == v40 )
                    {
                        return new uint[] { 0xCC0610, 0xCBFC3E };
                    }
                    v41 = v40 - 8;
                    if ( 0 == v41 )
                    {
                        return new uint[] { 0x606EBE, 0xCBFABE };
                    }
                    if ( v41 != 2 )
                        return new uint[2];
                }
                else
                {
                    if ( v8 == 158 )
                    {
                        return new uint[] { 0xCC05D4, 0xCBFACC };
                    }
                    if ( v8 <= 137 )
                    {
                        if ( v8 != 137 )
                        {
                            v25 = v8 - 109;
                            if ( 0 == v25 )
                            {
                                return new uint[] { 0xCBFC85, 0xCBFA86 };
                            }
                            v26 = v25 - 2;
                            if ( 0 == v26 )
                            {
                                return new uint[] { 0xCC0B87, 0xCBF907 };
                            }
                            v27 = v26 - 5;
                            if ( 0 == v27 )
                            {
                                return new uint[] { 0xCC0649, 0xCBF953 };
                            }
                            v28 = v27 - 3;
                            if ( 0 == v28 )
                            {
                                return new uint[] { 0x5F482C, 0xCBFB90 };
                            }
                            v29 = v28 - 4;
                            if ( 0 == v29 )
                            {
                                return new uint[] { 0xCC10CC, 0xCBFA32 };
                            }
                            if ( v29 == 7 )
                            {
                                return new uint[] { 0x5F482C, 0xCBFBBA };
                            }
                            return new uint[2];
                        }
                        return new uint[] { 0xCAEF72, 0xCBFAA2 };
                    }
                    v30 = v8 - 147;
                    if ( 0 == v30 )
                    {
                        return new uint[] { 0x60582F, 0xCBF7A9 };
                    }
                    v31 = v30 - 1;
                    if ( 0 == v31 )
                    {
                        return new uint[] { 0xCBFEBF, 0xCBFB20 };
                    }
                    v32 = v31 - 1;
                    if ( v32 != 0 )
                    {
                        v33 = v32 - 1;
                        if ( 0 == v33 )
                        {
                            return new uint[] { 0xCC007F, 0xCBF6A2 };
                        }
                        if ( v33 == 7 )
                        {
                            return new uint[] { 0xCBFCF7, 0xCBFA5C };
                        }
                        return new uint[2];
                    }
                }
                return new uint[] { 0xCC0530, 0xCBFBAC };
            }
            if ( v8 == 106 )
            {
                return new uint[] { 0xCC1109, 0xCBF9D0 };
            }
            if ( v8 > 50 )
            {
                if ( v8 <= 80 )
                {
                    if ( v8 == 80 )
                    {
                        return new uint[] { 0x6090F0, 0xCBFADA };
                    }
                    v20 = v8 - 51;
                    if ( 0 == v20 )
                    {
                        return new uint[] { 0xCC0C8D, 0xCBFB58 };
                    }
                    v21 = v20 - 3;
                    if ( v21 != 0 )
                    {
                        v22 = v21 - 2;
                        if ( 0 == v22 )
                        {
                            return new uint[] { 0x6090F0, 0xCBFB04 };
                        }
                        v23 = v22 - 3;
                        if ( 0 == v23 )
                        {
                            return new uint[] { 0xCBFEEE, 0xCBF6DA };
                        }
                        v24 = v23 - 12;
                        if ( 0 == v24 )
                        {
                            return new uint[] { 0xCBFD30, 0xCBFA78 };
                        }
                        if ( v24 == 5 )
                        {
                            return new uint[] { 0x5FDCF5, 0xCBFB74 };
                        }
                        return new uint[2];
                    }
                    return new uint[] { 0xCC0682, 0xCBF6B0 };
                }
                if ( v8 == 81 )
                {
                    return new uint[] { 0x600BEA, 0xCBFB9E };
                }
                if ( v8 == 87 )
                {
                    return new uint[] { 0x5FEA23, 0xCBFA94 };
                }
                if ( v8 != 90 )
                {
                    if ( v8 == 94 )
                    {
                        return new uint[] { 0xCBFE90, 0xCBFAF6 };
                    }
                    if ( v8 == 104 )
                    {
                        return new uint[] { 0x5FDCF5, 0xCBFA24 };
                    }
                    return new uint[2];
                }
                return new uint[] { 0xCC05D4, 0xCBF8DD };
            }
            if ( v8 == 50 )
            {
                return new uint[] { 0xCC0682, 0xCBFB3C };
            }
            if ( v8 > 36 )
            {
                v16 = v8 - 37;
                if ( 0 == v16 )
                {
                    return new uint[] { 0x5FEA23, 0xCBF9B4 };
                }
                v17 = v16 - 5;
                if ( 0 == v17 )
                {
                    return new uint[] { 0xCC0CF2, 0xCBFA6A };
                }
                v18 = v17 - 2;
                if ( 0 == v18 )
                {
                    return new uint[] { 0xCC06FE, 0xCBFB4A };
                }
                v19 = v18 - 3;
                if ( 0 == v19 )
                {
                    return new uint[] { 0xCC0D2B, 0xCBF9C2 };
                }
                if ( v19 == 2 )
                {
                    return new uint[] { 0x5FDCF5, 0xCBF8EB };
                }
                return new uint[2];
            }
            if ( v8 == 36 )
            {
                return new uint[] { 0xCBFCBE, 0xCBFA4E };
            }
            v10 = v8 - 3;
            if ( 0 == v10 )
            {
                return new uint[] { 0xCBFDF3, 0xCBFB82 };
            }
            v11 = v10 - 11;
            if ( 0 == v11 )
            {
                return new uint[] { 0x5F482C, 0xCBFB12 };
            }
            v12 = v11 - 6;
            if ( 0 == v12 )
            {
                return new uint[] { 0xCC0BD0, 0xCBFBF8 };
            }
            v13 = v12 - 1;
            if ( 0 == v13 )
            {
                return new uint[] { 0x5FDCF5, 0xCBF945 };
            }
            v14 = v13 - 1;
            if ( 0 == v14 )
            {
                return new uint[] { 0xCBFF27, 0xCBFB66 };
            }
            if ( v14 != 12 )
                return new uint[2];
            return new uint[] { 0x5F482C, 0xCBF8F9 };
            #endregion
        }
    }
}
