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
        
        public override uint[] CalculateJamFunctionOffsets(uint opcode)
        {
            #region Imported from Hex-Rays
            uint v5; // edx@1
            uint v6; // eax@1
            uint v7; // bx@1
            uint v8; // eax@2
            uint v9; // ebx@11
            uint v10; // eax@24
            uint v11; // eax@25
            uint v12; // eax@26
            uint v13; // eax@27
            uint v15; // eax@47
            uint v16; // eax@48
            uint v17; // eax@49
            uint v18; // eax@50
            uint v19; // eax@58
            uint v20; // eax@59
            uint v21; // eax@60
            uint v22; // eax@71
            uint v23; // eax@72
            uint v24; // eax@73
            uint v25; // eax@74
            uint v26; // eax@82
            uint v27; // eax@83
            uint v28; // eax@84
            uint v29; // eax@93
            uint v30; // eax@94
            uint v31; // eax@95
            uint v32; // eax@96
            uint v33; // eax@104
            uint v34; // eax@105
            uint v35; // eax@106
            uint v36; // eax@119
            uint v37; // eax@120
            uint v38; // eax@121
            uint v39; // eax@122
            uint v40; // eax@130
            uint v41; // eax@131
            uint v42; // eax@132
            uint v43; // eax@141
            uint v44; // eax@142
            uint v45; // eax@143
            uint v46; // eax@144
            uint v47; // eax@152
            uint v48; // eax@153
            uint v49; // eax@154
            uint v50; // eax@165
            uint v51; // eax@166
            uint v52; // eax@167
            uint v53; // eax@168
            uint v54; // eax@178
            uint v55; // eax@179
            uint v56; // eax@180
            uint v57; // eax@189
            uint v58; // eax@190
            uint v59; // eax@191
            uint v60; // eax@192
            uint v61; // eax@200
            uint v62; // eax@201
            uint v63; // eax@202

            v5 = opcode - 1;
            v6 = (opcode - 1) & 0xEA2;
            v7 = opcode - 1;
            if ( v6 == 130 )
            {
                v8 = v5 & 1 | ((v5 & 0x1C | ((v5 & 0x40 | ((v7 & 0x100 | (v5 >> 3) & 0x1E00) >> 1)) >> 1)) >> 1);
            }
            else
            {
                if ( v6 == 2690 )
                {
                    v8 = (v5 & 1 | ((v5 & 0x1C | ((v5 & 0x40 | ((v7 & 0x100 | (v5 >> 3) & 0x1E00) >> 1)) >> 1)) >> 1)) + 128;
                }
                else
                {
                    if ( (v5 & 0x1CC6) == 1152 )
                    {
                        v8 = (v5 & 1 | ((v5 & 0x38 | ((v7 & 0x300 | (v5 >> 3) & 0x1C00) >> 2)) >> 2)) + 256;
                    }
                    else
                    {
                        if ( (v5 & 0x1CC6) == 1216 )
                        {
                            v8 = (v5 & 1 | ((v5 & 0x38 | ((v7 & 0x300 | (v5 >> 3) & 0x1C00) >> 2)) >> 2)) + 320;
                        }
                        else
                        {
                            if ( (v5 & 0x1682) == 1154 )
                            {
                                v8 = (v5 & 1 | ((v5 & 0x7C | ((v5 & 0x100 | ((v7 & 0x800 | (v5 >> 1) & 0x7000) >> 2)) >> 1)) >> 1)) + 384;
                            }
                            else
                            {
                                v9 = v7 & 0x800;
                                if ( (v5 & 0x15B8) == 5248 )
                                {
                                    v8 = (v5 & 7 | ((v5 & 0x40 | ((v5 & 0x200 | ((v9 | (v5 >> 1) & 0x7000) >> 1)) >> 2)) >> 3)) + 640;
                                }
                                else
                                {
                                    if ( (v5 & 0x15B8) == 5264 )
                                        v8 = (v5 & 7 | ((v5 & 0x40 | ((v5 & 0x200 | ((v9 | (v5 >> 1) & 0x7000) >> 1)) >> 2)) >> 3)) + 704;
                                    else
                                        v8 = (v5 & 0x1F | ((v5 & 0x40 | ((v5 & 0x200 | ((v9 | (v5 >> 1) & 0x7000) >> 1)) >> 2)) >> 1)) + 768;
                                }
                            }
                        }
                    }
                }
            }
            if ( v8 > 255 )
            {
                if ( v8 > 431 )
                {
                    if ( v8 > 520 )
                    {
                        if ( v8 > 575 )
                        {
                            v61 = v8 - 576;
                            if ( 0 == v61 )
                            {
                                return new uint[] { 0xCB8C04, 0xCB862B };
                            }
                            v62 = v61 - 3;
                            if ( 0 == v62 )
                            {
                                return new uint[] { 0x6090F0, 0xCB7ED9 };
                            }
                            v63 = v62 - 16;
                            if ( 0 == v63 )
                            {
                                return new uint[] { 0xCB872F, 0xCB79EA };
                            }
                            if ( v63 == 3 )
                            {
                                return new uint[] { 0xCB84C2, 0xCB7F91 };
                            }
                        }
                        else
                        {
                            if ( v8 == 575 )
                            {
                                return new uint[] { 0xCB8A57, 0xCB83F3 };
                            }
                            v57 = v8 - 531;
                            if ( 0 == v57 )
                            {
                                return new uint[] { 0x5FDCF5, 0xCB7A76 };
                            }
                            v58 = v57 - 14;
                            if ( 0 == v58 )
                            {
                                return new uint[] { 0xCB872F, 0xCB8601 };
                            }
                            v59 = v58 - 17;
                            if ( 0 == v59 )
                            {
                                return new uint[] { 0xCB84FF, 0xCB7FBB };
                            }
                            v60 = v59 - 2;
                            if ( 0 == v60 )
                            {
                                return new uint[] { 0x6090F0, 0xCB7A68 };
                            }
                            if ( v60 == 5 )
                            {
                                return new uint[] { 0xCB84FF, 0xCB8655 };
                            }
                        }
                        return new uint[2];
                    }
                    if ( v8 == 520 )
                    {
                        return new uint[] { 0xCB8B44, 0xCB8183 };
                    }
                    if ( v8 > 472 )
                    {
                        v54 = v8 - 491;
                        if ( 0 == v54 )
                        {
                            return new uint[] { 0xCB8B15, 0xCB7D6C };
                        }
                        v55 = v54 - 11;
                        if ( v55 != 0 )
                        {
                            v56 = v55 - 2;
                            if ( 0 == v56 )
                            {
                                return new uint[] { 0xCB84FF, 0xCB8069 };
                            }
                            if ( v56 == 14 )
                            {
                                return new uint[] { 0xCAD5E2, 0xCB79F8 };
                            }
                            return new uint[2];
                        }
                    }
                    else
                    {
                        if ( v8 == 472 )
                        {
                            return new uint[] { 0xCB905E, 0xCB7ECB };
                        }
                        v50 = v8 - 446;
                        if ( v50 != 0 )
                        {
                            v51 = v50 - 3;
                            if ( 0 == v51 )
                            {
                                return new uint[] { 0xCAD5E2, 0xCB812F };
                            }
                            v52 = v51 - 5;
                            if ( 0 == v52 )
                            {
                                return new uint[] { 0x6090F0, 0xCB7B84 };
                            }
                            v53 = v52 - 1;
                            if ( v53 != 0 )
                            {
                                if ( v53 == 5 )
                                {
                                    return new uint[] { 0x6090F0, 0xCB82CF };
                                }
                                return new uint[2];
                            }
                        }
                        else
                        {
                        }
                    }
                    return new uint[] { 0xCB90D3, 0xCB7DB8 };
                }
                if ( v8 == 431 )
                {
                    return new uint[] { 0xCB89DF, 0xCB84B4 };
                }
                if ( v8 > 374 )
                {
                    if ( v8 > 392 )
                    {
                        v47 = v8 - 400;
                        if ( 0 == v47 )
                        {
                            return new uint[] { 0x6090F0, 0xCB79CE };
                        }
                        v48 = v47 - 1;
                        if ( 0 == v48 )
                        {
                            return new uint[] { 0xCB95CF, 0xCB82DD };
                        }
                        v49 = v48 - 7;
                        if ( 0 == v49 )
                        {
                            return new uint[] { 0x6090F0, 0xCB7D50 };
                        }
                        if ( v49 == 11 )
                        {
                            return new uint[] { 0xCB872F, 0xCB7A14 };
                        }
                        return new uint[2];
                    }
                    if ( v8 != 392 )
                    {
                        v43 = v8 - 377;
                        if ( 0 == v43 )
                        {
                            return new uint[] { 0x6090F0, 0xCB8363 };
                        }
                        v44 = v43 - 1;
                        if ( 0 == v44 )
                        {
                            return new uint[] { 0xCB8B15, 0xCB824A };
                        }
                        v45 = v44 - 1;
                        if ( 0 == v45 )
                        {
                            return new uint[] { 0xCB872F, 0xCB7EE7 };
                        }
                        v46 = v45 - 1;
                        if ( 0 == v46 )
                        {
                            return new uint[] { 0xCB84C2, 0xCB7EAF };
                        }
                        if ( v46 == 8 )
                        {
                            return new uint[] { 0x6090F0, 0xCB85A0 };
                        }
                        return new uint[2];
                    }
                    return new uint[] { 0xCB910C, 0xCB8167 };
                }
                if ( v8 == 374 )
                {
                    return new uint[] { 0xCB8883, 0xCB81BB };
                }
                if ( v8 > 310 )
                {
                    v40 = v8 - 313;
                    if ( 0 == v40 )
                    {
                        return new uint[] { 0xCB872F, 0xCB80C5 };
                    }
                    v41 = v40 - 8;
                    if ( 0 == v41 )
                    {
                        return new uint[] { 0x6090F0, 0xCB7EBD };
                    }
                    v42 = v41 - 15;
                    if ( 0 == v42 )
                    {
                        return new uint[] { 0x6090F0, 0xCB819F };
                    }
                    if ( v42 == 21 )
                    {
                        return new uint[] { 0x6090F0, 0xCB81AD };
                    }
                    return new uint[2];
                }
                if ( v8 == 310 )
                {
                    return new uint[] { 0xCB8972, 0xCB79C0 };
                }
                v36 = v8 - 266;
                if ( 0 == v36 )
                {
                    return new uint[] { 0xCB8B15, 0xCB7B18 };
                }
                v37 = v36 - 6;
                if ( 0 == v37 )
                {
                    return new uint[] { 0xCB872F, 0xCB860F };
                }
                v38 = v37 - 5;
                if ( v38 != 0 )
                {
                    v39 = v38 - 11;
                    if ( 0 == v39 )
                    {
                        return new uint[] { 0xCB872F, 0xCB85BC };
                    }
                    if ( v39 == 17 )
                    {
                        return new uint[] { 0xCB96E1, 0xCB804D };
                    }
                    return new uint[2];
                }
                return new uint[] { 0xCB9025, 0xCB85AE };
            }
            if ( v8 == 255 )
            {
                return new uint[] { 0x605DDD, 0xCB7A84 };
            }
            if ( v8 > 112 )
            {
                if ( v8 > 190 )
                {
                    if ( v8 > 226 )
                    {
                        v33 = v8 - 228;
                        if ( 0 == v33 )
                        {
                            return new uint[] { 0xCB8700, 0xCB7D34 };
                        }
                        v34 = v33 - 6;
                        if ( 0 == v34 )
                        {
                            return new uint[] { 0x605DDD, 0xCB79DC };
                        }
                        v35 = v34 - 5;
                        if ( 0 == v35 )
                        {
                            return new uint[] { 0xCB8A57, 0xCB8266 };
                        }
                        if ( v35 == 1 )
                        {
                            return new uint[] { 0xCB872F, 0xCB7D5E };
                        }
                    }
                    else
                    {
                        if ( v8 == 226 )
                        {
                            return new uint[] { 0x6090F0, 0xCB8647 };
                        }
                        v29 = v8 - 194;
                        if ( 0 == v29 )
                        {
                            return new uint[] { 0xCB84FF, 0xCB82B3 };
                        }
                        v30 = v29 - 12;
                        if ( 0 == v30 )
                        {
                            return new uint[] { 0xCB9506, 0xCB7F9F };
                        }
                        v31 = v30 - 1;
                        if ( 0 == v31 )
                        {
                            return new uint[] { 0x605DDD, 0xCB805B };
                        }
                        v32 = v31 - 9;
                        if ( 0 == v32 )
                        {
                            return new uint[] { 0xCB84FF, 0xCB8175 };
                        }
                        if ( v32 == 6 )
                        {
                            return new uint[] { 0xCB84FF, 0xCB7C00 };
                        }
                    }
                    return new uint[2];
                }
                if ( v8 == 190 )
                {
                    return new uint[] { 0xCB8A86, 0xCB8663 };
                }
                if ( v8 > 132 )
                {
                    v26 = v8 - 133;
                    if ( 0 == v26 )
                    {
                        return new uint[] { 0xCB8A57, 0xCB79B2 };
                    }
                    v27 = v26 - 13;
                    if ( 0 == v27 )
                    {
                        return new uint[] { 0x6090F0, 0xCB8639 };
                    }
                    v28 = v27 - 21;
                    if ( 0 == v28 )
                    {
                        return new uint[] { 0x6090F0, 0xCB7DC6 };
                    }
                    if ( v28 != 9 )
                        return new uint[2];
                    return new uint[] { 0xCB9145, 0xCB7C0E };
                }
                if ( v8 == 132 )
                {
                    return new uint[] { 0xCB8A0E, 0xCB80D3 };
                }
                v22 = v8 - 115;
                if ( 0 == v22 )
                {
                    return new uint[] { 0x6090F0, 0xCB7B26 };
                }
                v23 = v22 - 4;
                if ( 0 == v23 )
                {
                    return new uint[] { 0xCB84FF, 0xCB82C1 };
                }
                v24 = v23 - 1;
                if ( 0 == v24 )
                {
                    return new uint[] { 0xCB90D3, 0xCB7A92 };
                }
                v25 = v24 - 1;
                if ( 0 == v25 )
                {
                    return new uint[] { 0xCB84FF, 0xCB80B7 };
                }
                if ( v25 != 4 )
                    return new uint[2];
                return new uint[] { 0xCB909A, 0xCB7A06 };
            }
            if ( v8 == 112 )
            {
                return new uint[] { 0xCB90D3, 0xCB84A6 };
            }
            if ( v8 > 72 )
            {
                if ( v8 > 93 )
                {
                    v19 = v8 - 100;
                    if ( 0 == v19 )
                    {
                        return new uint[] { 0xCB8B15, 0xCB7E93 };
                    }
                    v20 = v19 - 2;
                    if ( 0 == v20 )
                    {
                        return new uint[] { 0x6090F0, 0xCB81D7 };
                    }
                    v21 = v20 - 7;
                    if ( v21 != 0 )
                    {
                        if ( v21 == 2 )
                        {
                            return new uint[] { 0xCB872F, 0xCB814B };
                        }
                        return new uint[2];
                    }
                    return new uint[] { 0xCB90D3, 0xCB7C38 };
                }
                if ( v8 == 93 )
                {
                    return new uint[] { 0xCB8F50, 0xCB8274 };
                }
                v15 = v8 - 78;
                if ( v15 != 0 )
                {
                    v16 = v15 - 3;
                    if ( 0 == v16 )
                    {
                        return new uint[] { 0xCB8B15, 0xCB8191 };
                    }
                    v17 = v16 - 7;
                    if ( 0 == v17 )
                    {
                        return new uint[] { 0xCB8D09, 0xCB8258 };
                    }
                    v18 = v17 - 1;
                    if ( 0 == v18 )
                    {
                        return new uint[] { 0xCB8F50, 0xCB79A4 };
                    }
                    if ( v18 == 2 )
                    {
                        return new uint[] { 0xCB8B15, 0xCB7A5A };
                    }
                    return new uint[2];
                }
                return new uint[] { 0xCB9145, 0xCB81C9 };
            }
            if ( v8 == 72 )
            {
                return new uint[] { 0x6090F0, 0xCB7D26 };
            }
            if ( v8 > 45 )
            {
                if ( v8 == 47 )
                {
                    return new uint[] { 0xCB90D3, 0xCB7C2A };
                }
                if ( v8 == 52 )
                {
                    return new uint[] { 0x605DDD, 0xCB83E5 };
                }
                if ( v8 == 64 )
                {
                    return new uint[] { 0xCB84FF, 0xCB7FAD };
                }
                if ( v8 != 71 )
                    return new uint[2];
                return new uint[] { 0xCB9145, 0xCB83B7 };
            }
            if ( v8 == 45 )
            {
                return new uint[] { 0xCB905E, 0xCB7D42 };
            }
            v10 = v8 - 4;
            if ( 0 == v10 )
            {
                return new uint[] { 0x6090F0, 0xCB813D };
            }
            v11 = v10 - 1;
            if ( 0 == v11 )
            {
                return new uint[] { 0xCB84FF, 0xCB861D };
            }
            v12 = v11 - 4;
            if ( 0 == v12 )
            {
                return new uint[] { 0xCB84FF, 0xCB7C1C };
            }
            v13 = v12 - 10;
            if ( 0 == v13 )
            {
                return new uint[] { 0xCB872F, 0xCB84F1 };
            }
            if ( v13 != 2 )
                return new uint[2];
            return new uint[] { 0xCB8AE6, 0xCB820C };
            #endregion
        }
    }
}
