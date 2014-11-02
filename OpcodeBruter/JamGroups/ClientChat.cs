using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace OpcodeBruter.JamGroups
{
    public class ClientChat : JamDispatch
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
            return ((opcode - 1) & 0x9AA) == 2056;

        }

        public ClientChat(FileStream wow)
            : base(wow)
        {

        }

        public override uint CalculateHandler(uint opcode)
        {
            return 0x0CB2F02; // This one doesn't have a jump table
        }

        public override int CalculateOffset(uint a3)
        {
            return (int)((a3 - 1) & 1 | (((a3 - 1) & 4 | (((a3 - 1) & 0x10 | (((a3 - 1) & 0x40 | (((a3 - 1) & 0x600 | ((a3 - 1) >> 1) & 0x7800) >> 2)) >> 1)) >> 1)) >> 1));
        }
        
        public override uint[] CalculateJamFunctionOffsets(uint a3)
        {
            #region Imported from Hex-Rays
            var returnValues = new List<uint>();
            uint v7; // eax@1
            uint v8; // eax@6
            uint v9; // eax@7
            uint v10; // eax@8
            uint v11; // eax@9
            uint v12; // eax@10
            uint result; // eax@11
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
                        returnValues.Add(0xCBE862);
                        returnValues.Add(0xCBDF14);
                        return returnValues.ToArray();
                    }
                    v27 = v26 - 5;
                    if ( 0 == v27 )
                    {
                        returnValues.Add(0xCBEAC6);
                        returnValues.Add(0xCBE026);
                        return returnValues.ToArray();
                    }
                    v28 = v27 - 1;
                    if ( 0 == v28 )
                    {
                        returnValues.Add(0x6018A0);
                        returnValues.Add(0xCBDF84);
                        return returnValues.ToArray();
                    }
                    v29 = v28 - 1;
                    if ( 0 == v29 )
                    {
                        returnValues.Add(0xCBF390);
                        returnValues.Add(0xCBDEA6);
                        returnValues.Add(0xCB2E9C);
                        return returnValues.ToArray();
                    }
                    v30 = v29 - 1;
                    if ( 0 == v30 )
                    {
                        returnValues.Add(0xCBEA68);
                        returnValues.Add(0xCBE0DA);
                        return returnValues.ToArray();
                    }
                    result = v30 - 1;
                    if ( 0==result )
                    {
                        returnValues.Add(0xCBEA97);
                        returnValues.Add(0xCBE104);
                        return returnValues.ToArray();
                    }
                }
                else
                {
                    if ( v7 == 84 )
                    {
                        returnValues.Add(0xCBE475);
                        returnValues.Add(0xCBE0E8);
                        return returnValues.ToArray();
                    }
                    v20 = v7 - 62;
                    if ( 0 == v20 )
                    {
                        returnValues.Add(0xCBF3CD);
                        returnValues.Add(0xCBDEC2);
                        returnValues.Add(0xCB2E7A);
                        return returnValues.ToArray();
                    }
                    v21 = v20 - 2;
                    if ( 0 == v21 )
                    {
                        returnValues.Add(0xCBEA0A);
                        returnValues.Add(0xCBDE7C);
                        return returnValues.ToArray();
                    }
                    v22 = v21 - 8;
                    if ( 0 == v22 )
                    {
                        returnValues.Add(0x5FDCF5);
                        returnValues.Add(0xCBDF4C);
                        return returnValues.ToArray();
                    }
                    v23 = v22 - 1;
                    if ( 0 == v23 )
                    {
                        returnValues.Add(0xCAD5E2);
                        returnValues.Add(0xCBDF22);
                        return returnValues.ToArray();
                    }
                    v24 = v23 - 3;
                    if ( 0 == v24 )
                    {
                        returnValues.Add(0xCBEA39);
                        returnValues.Add(0xCBDF06);
                        return returnValues.ToArray();
                    }
                    v25 = v24 - 3;
                    if ( 0 == v25 )
                    {
                        returnValues.Add(0xCBE61D);
                        returnValues.Add(0xCBE0B6);
                        return returnValues.ToArray();
                    }
                    result = v25 - 1;
                    if ( result != 0)
                    {
                        returnValues.Add(0xCAD5E2);
                        returnValues.Add(0xCBDFB8);
                        return returnValues.ToArray();
                    }
                }
                return new uint[2];
            }
            if ( v7 == 48 )
            {
                returnValues.Add(0xCBE54A);
                returnValues.Add(0xCBE112);
                return returnValues.ToArray();
            }
            if ( v7 > 18 )
            {
                v14 = v7 - 26;
                if ( 0 == v14 )
                {
                    returnValues.Add(0x5FDCF5);
                    returnValues.Add(0xCBDFD4);
                    return returnValues.ToArray();
                }
                v15 = v14 - 1;
                if ( 0 == v15 )
                {
                    returnValues.Add(0x60A24C);
                    returnValues.Add(0xCBDF68);
                    return returnValues.ToArray();
                }
                v16 = v15 - 6;
                if ( 0 == v16 )
                {
                    returnValues.Add(0xCBEC6D);
                    returnValues.Add(0xCBDF5A);
                    return returnValues.ToArray();
                }
                v17 = v16 - 9;
                if ( 0 == v17 )
                {
                    returnValues.Add(0xCBE3D7);
                    returnValues.Add(0xCBDF76);
                    return returnValues.ToArray();
                }
                v18 = v17 - 1;
                if ( 0 == v18 )
                {
                    returnValues.Add(0xCBF259);
                    returnValues.Add(0xCBDEB4);
                    returnValues.Add(0xCB2EE0);
                    return returnValues.ToArray();
                }
                v19 = v18 - 3;
                if ( 0 == v19 )
                {
                    returnValues.Add(0xCBE745);
                    returnValues.Add(0xCBDE98);
                    return returnValues.ToArray();
                }
                result = v19 - 1;
                if ( 0!=result )
                {
                    returnValues.Add(0x61E899);
                    returnValues.Add(0xCBE034);
                    return returnValues.ToArray();
                }
                return new uint[2];
            }
            if ( v7 == 18 )
            {
                returnValues.Add(0xCAD5E2);
                returnValues.Add(0xCBDF3E);
                return returnValues.ToArray();
            }
            if ( 0 == v7 )
            {
                returnValues.Add(0xCBE774);
                returnValues.Add(0xCBDFC6);
                return returnValues.ToArray();
            }
            v8 = v7 - 1;
            if ( 0 == v8 )
            {
                returnValues.Add(0xCBEEA4);
                returnValues.Add(0xCBE042);
                returnValues.Add(0x61B541);
                return returnValues.ToArray();
            }
            v9 = v8 - 1;
            if ( 0 == v9 )
            {
                returnValues.Add(0xCBE833);
                returnValues.Add(0xCBDED0);
                return returnValues.ToArray();
            }
            v10 = v9 - 2;
            if ( 0 == v10 )
            {
                returnValues.Add(0xCBF353);
                returnValues.Add(0xCBDF30);
                returnValues.Add(0xCB2EBE);
                return returnValues.ToArray();
            }
            v11 = v10 - 7;
            if ( 0 == v11 )
            {
                returnValues.Add(0x60A24C);
                returnValues.Add(0xCBE0F6);
                return returnValues.ToArray();
            }
            v12 = v11 - 3;
            if ( 0 == v12 )
            {
                returnValues.Add(0xCBE6B9);
                returnValues.Add(0xCBDE8A);
                return returnValues.ToArray();
            }
            result = v12 - 1;
            if ( result != 0 )
            {
                return new uint[2];
            }
            returnValues.Add(0xCAD5E2);
            returnValues.Add(0xCBE0A8);
            return returnValues.ToArray();
            #endregion
        }
    }
}
