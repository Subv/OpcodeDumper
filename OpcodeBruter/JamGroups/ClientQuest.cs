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
        
        public override uint[] CalculateJamFunctionOffsets(uint a5)
        {
            #region Imported from Hex-Rays
            uint v7; // eax@1
            uint v8; // eax@7
            uint v9; // eax@8
            uint v10; // eax@9
            uint v11; // eax@23
            uint v12; // eax@24
            uint v13; // eax@25
            uint v14; // eax@26
            uint v15; // eax@27
            uint v16; // eax@28
            uint v17; // eax@42
            uint v18; // eax@43
            uint v19; // eax@44
            uint v20; // eax@51
            uint v21; // eax@52
            uint v22; // eax@58
            uint v23; // eax@59
            uint v24; // eax@60
            uint v25; // eax@61
            uint v26; // eax@62
            uint v27; // eax@63

            v7 = (a5 - 1) & 1 | (((a5 - 1) & 0x7C | (((a5 - 1) & 0x300 | (((a5 - 1) & 0x800 | ((a5 - 1) >> 1) & 0x7000) >> 1)) >> 1)) >> 1);
            if ( v7 > 137 )
            {
                if ( v7 > 218 )
                {
                    v22 = v7 - 224;
                    if ( 0 == v22 )
                    {
                        return new uint[] { 0xA3C7D0, 0x5F4D75 };
                    }
                    v23 = v22 - 15;
                    if ( 0 == v23 )
                    {
                        return new uint[] { 0x5F870B, 0x5F4746 };
                    }
                    v24 = v23 - 11;
                    if ( 0 == v24 )
                    {
                        return new uint[] { 0x5F5C6D, 0x5F46E4 };
                    }
                    v25 = v24 - 14;
                    if ( 0 == v25 )
                    {
                        return new uint[] { 0x604D59, 0x5F4597 };
                    }
                    v26 = v25 - 15;
                    if ( 0 == v26 )
                    {
                        return new uint[] { 0xCAEF72, 0x5F457B };
                    }
                    v27 = v26 - 7;
                    if ( 0 == v27 )
                    {
                        return new uint[] { 0x5F482C, 0x5F44B1 };
                    }
                    if ( v27 == 1 )
                    {
                        return new uint[] { 0x5F6395, 0x5F4589 };
                    }
                }
                else
                {
                    if ( v7 == 218 )
                    {
                        return new uint[] { 0x5F493F, 0x5F470E };
                    }
                    if ( v7 > 164 )
                    {
                        v20 = v7 - 171;
                        if ( 0 == v20 )
                        {
                            return new uint[] { 0x5FDCF5, 0x5F460A };
                        }
                        v21 = v20 - 5;
                        if ( 0 == v21 )
                        {
                            return new uint[] { 0x60971A, 0x5F4738 };
                        }
                        if ( v21 == 25 )
                        {
                            return new uint[] { 0x5F5271, 0x5F4656 };
                        }
                    }
                    else
                    {
                        if ( v7 == 164 )
                        {
                            return new uint[] { 0x5FDCF5, 0x5F4543 };
                        }
                        v17 = v7 - 144;
                        if ( 0 == v17 )
                        {
                            return new uint[] { 0xCB872F, 0x5F4495 };
                        }
                        v18 = v17 - 2;
                        if ( 0 == v18 )
                        {
                            return new uint[] { 0x5F6359, 0x5F4664 };
                        }
                        v19 = v18 - 9;
                        if ( 0 == v19 )
                        {
                            return new uint[] { 0x5F6B49, 0x5F4648 };
                        }
                        if ( v19 == 1 )
                        {
                            return new uint[] { 0x5FDCF5, 0x5F4487 };
                        }
                    }
                }
                return new uint[2];
            }
            if ( v7 == 137 )
            {
                return new uint[] { 0xCAD5E2, 0x5F455F };
            }
            if ( v7 > 63 )
            {
                v11 = v7 - 67;
                if ( 0 == v11 )
                {
                    return new uint[] { 0x5F6C7F, 0x5F4551 };
                }
                v12 = v11 - 25;
                if ( 0 == v12 )
                {
                    return new uint[] { 0x5F5F56, 0x5F456D };
                }
                v13 = v12 - 6;
                if ( 0 == v13 )
                {
                    return new uint[] { 0x5F640B, 0x5F4505 };
                }
                v14 = v13 - 1;
                if ( 0 == v14 )
                {
                    return new uint[] { 0x5F8537, 0x5F44DB };
                }
                v15 = v14 - 2;
                if ( 0 == v15 )
                {
                    return new uint[] { 0x5F6C42, 0x5F44BF };
                }
                v16 = v15 - 7;
                if ( 0 == v16 )
                {
                    return new uint[] { 0xCAD5E2, 0x5F4700 };
                }
                if ( v16 == 11 )
                {
                    return new uint[] { 0x5F6DA1, 0x5F44A3 };
                }
                return new uint[2];
            }
            if ( v7 == 63 )
            {
                return new uint[] { 0x5F63CE, 0x5F4479 };
            }
            if ( v7 > 39 )
            {
                if ( v7 == 42 )
                {
                    return new uint[] { 0x5F85D6, 0x5F44F7 };
                }
                if ( v7 == 49 )
                {
                    return new uint[] { 0x5FDCF5, 0x5F4672 };
                }
                if ( v7 == 57 )
                {
                    return new uint[] { 0x5F4BB4, 0x5F472A };
                }
                return new uint[2];
            }
            if ( v7 == 39 )
            {
                return new uint[] { 0x5F482C, 0x5F44E9 };
            }
            v8 = v7 - 2;
            if ( 0 == v8 )
            {
                return new uint[] { 0xCAD5E2, 0x5F47A3 };
            }
            v9 = v8 - 15;
            if ( 0 == v9 )
            {
                return new uint[] { 0x5FDCF5, 0x5F44CD };
            }
            v10 = v9 - 1;
            if ( 0 == v10 )
            {
                return new uint[] { 0x5FDCF5, 0x5F471C };
            }
            if ( v10 != 5 )
                return new uint[2];
            return new uint[] { 0x5F482C, 0x5F4513 };
            #endregion
        }
    }
}
