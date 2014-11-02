using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace OpcodeBruter.JamGroups
{
    public class ClientLFG : JamDispatch
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
            return ((opcode - 1) & 0x8DA) == 0;
        }

        public ClientLFG(FileStream wow)
            : base(wow)
        {

        }

        public override uint CalculateHandler(uint opcode)
        {
            return 0x00CBA49A; // This one doesn't have a jump table
        }

        public override int CalculateOffset(uint opcode)
        {
            return (int)((opcode - 1) & 1 | (((opcode - 1) & 4 | (((opcode - 1) & 0x20 | (((opcode - 1) & 0x700 | ((opcode - 1) >> 1) & 0x7800) >> 2)) >> 2)) >> 1));
        }
        
        public override uint[] CalculateJamFunctionOffsets(uint a3)
        {
            #region Imported from Hex-Rays
            uint v7; // eax@1
            uint v8; // eax@6
            uint v9; // eax@7
            uint v10; // eax@8
            uint v11; // eax@9
            uint v12; // eax@10
            uint v13; // eax@11
            uint v14; // eax@20
            uint v15; // eax@21
            uint v16; // eax@22
            uint v17; // eax@23
            uint v18; // eax@24
            uint v19; // eax@25
            uint v20; // eax@37
            uint v21; // eax@38
            uint v22; // eax@39
            uint v23; // eax@40
            uint v24; // eax@41
            uint v25; // eax@42
            uint v26; // eax@52
            uint v27; // eax@53
            uint v28; // eax@54
            uint v29; // eax@55
            uint v30; // eax@56

            v7 = (a3 - 1) & 1 | (((a3 - 1) & 4 | (((a3 - 1) & 0x10 | (((a3 - 1) & 0x40 | (((a3 - 1) & 0x600 | ((a3 - 1) >> 1) & 0x7800) >> 2)) >> 1)) >> 1)) >> 1);
            if ( v7 > 48 )
            {
                if ( v7 > 84 )
                {
                    v26 = v7 - 88;
                    if ( 0 == v26 )
                    {
                        return new uint[] { 0xA3C7D0, 0xCBE862 };
                    }
                    v27 = v26 - 5;
                    if ( 0 == v27 )
                    {
                        return new uint[] { 0xCBEAC6, 0xCBE026 };
                    }
                    v28 = v27 - 1;
                    if ( 0 == v28 )
                    {
                        return new uint[] { 0x6018A0, 0xCBDF84 };
                    }
                    v29 = v28 - 1;
                    if ( 0 == v29 )
                    {
                        return new uint[] { 0xCBF390, 0xCBDEA6 };
                    }
                    v30 = v29 - 1;
                    if ( 0 == v30 )
                    {
                        return new uint[] { 0xCBEA68, 0xCBE0DA };
                    }
                    v13 = v30 - 1;
                    if ( 0 == v13 )
                    {
                        return new uint[] { 0xCBEA97, 0xCBE104 };
                    }
                }
                else
                {
                    if ( v7 == 84 )
                    {
                        return new uint[] { 0xCBE475, 0xCBE0E8 };
                    }
                    v20 = v7 - 62;
                    if ( 0 == v20 )
                    {
                        return new uint[] { 0xCBF3CD, 0xCBDEC2 };
                    }
                    v21 = v20 - 2;
                    if ( 0 == v21 )
                    {
                        return new uint[] { 0xCBEA0A, 0xCBDE7C };
                    }
                    v22 = v21 - 8;
                    if ( 0 == v22 )
                    {
                        return new uint[] { 0x5FDCF5, 0xCBDF4C };
                    }
                    v23 = v22 - 1;
                    if ( 0 == v23 )
                    {
                        return new uint[] { 0xCAD5E2, 0xCBDF22 };
                    }
                    v24 = v23 - 3;
                    if ( 0 == v24 )
                    {
                        return new uint[] { 0xCBEA39, 0xCBDF06 };
                    }
                    v25 = v24 - 3;
                    if ( 0 == v25 )
                    {
                        return new uint[] { 0xCBE61D, 0xCBE0B6 };
                    }
                    v13 = v25 - 1;
                    if ( 0 == v13 )
                    {
                        return new uint[] { 0xCAD5E2, 0xCBDFB8 };
                    }
                }
                return new uint[2];
            }
            if ( v7 == 48 )
            {
                return new uint[] { 0xCBE54A, 0xCBE112 };
            }
            if ( v7 > 18 )
            {
                v14 = v7 - 26;
                if ( 0 == v14 )
                {
                    return new uint[] { 0x5FDCF5, 0xCBDFD4 };
                }
                v15 = v14 - 1;
                if ( 0 == v15 )
                {
                    return new uint[] { 0x60A24C, 0xCBDF68 };
                }
                v16 = v15 - 6;
                if ( 0 == v16 )
                {
                    return new uint[] { 0xCBEC6D, 0xCBDF5A };
                }
                v17 = v16 - 9;
                if ( 0 == v17 )
                {
                    return new uint[] { 0xCBE3D7, 0xCBDF76 };
                }
                v18 = v17 - 1;
                if ( 0 == v18 )
                {
                    return new uint[] { 0xCBF259, 0xCBDEB4 };
                }
                v19 = v18 - 3;
                if ( 0 == v19 )
                {
                    return new uint[] { 0xCBE745, 0xCBDE98 };
                }
                v13 = v19 - 1;
                if ( 0 == v13 )
                {
                    return new uint[] { 0x61E899, 0xCBE034 };
                }
                return new uint[2];
            }
            if ( v7 == 18 )
            {
                return new uint[] { 0xCAD5E2, 0xCBDF3E };
            }
            if ( 0 == v7 )
            {
                return new uint[] { 0xCBE774, 0xCBDFC6 };
            }
            v8 = v7 - 1;
            if ( 0 == v8 )
            {
                return new uint[] { 0xCBEEA4, 0xCBE042 };
            }
            v9 = v8 - 1;
            if ( 0 == v9 )
            {
                return new uint[] { 0xCBE833, 0xCBDED0 };
            }
            v10 = v9 - 2;
            if ( 0 == v10 )
            {
                return new uint[] { 0xCBF353, 0xCBDF30 };
            }
            v11 = v10 - 7;
            if ( 0 == v11 )
            {
                return new uint[] { 0x60A24C, 0xCBE0F6 };
            }
            v12 = v11 - 3;
            if ( 0 == v12 )
            {
                return new uint[] { 0xCBE6B9, 0xCBDE8A };
            }
            v13 = v12 - 1;
            if ( v13 != 0 )
            {
                return new uint[2];
            }
            return new uint[] { 0xCAD5E2, 0xCBE0A8 };
            #endregion
        }
    }
}
