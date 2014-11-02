using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using Bea;
using System.Diagnostics;

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

        public override uint[] CalculateJamFunctionOffsets(uint opcode)
        {
            #region Dumped from Hex-Rays.
            uint v5; // edx@1
            uint v6; // eax@1
            uint v7; // si@1
            uint v8; // eax@2
            uint v9; // esi@3
            uint v10; // eax@14
            uint v11; // eax@15
            uint v12; // eax@16
            uint v15; // eax@33
            uint v16; // eax@34
            uint v17; // eax@35
            uint v18; // eax@42
            uint v19; // eax@43
            uint v21; // eax@53
            uint v22; // eax@54
            uint v23; // eax@55
            uint v24; // eax@62
            uint v25; // eax@63
            uint v26; // eax@71
            uint v27; // eax@72
            uint v28; // eax@73
            uint v29; // eax@80
            uint v30; // eax@81
            uint v31; // eax@93
            uint v32; // eax@94
            uint v33; // eax@95
            uint v34; // eax@103
            uint v35; // eax@104
            uint v36; // eax@114
            uint v37; // eax@115
            uint v38; // eax@116
            uint v39; // eax@124
            uint v40; // eax@125
            uint v41; // eax@135
            uint v42; // eax@136
            uint v43; // eax@137
            uint v44; // eax@145
            uint v45; // eax@146
            uint v46; // eax@152
            uint v47; // eax@153
            uint v48; // eax@154
            uint v49; // eax@155
            uint v50; // eax@156
            uint v51; // eax@157

            v5 = opcode - 1;
            v6 = opcode - 1;
            v7 = opcode - 1;
            if ( ((opcode - 1) & 0xEA2) == 642 )
            {
                v8 = v5 & 1 | ((v5 & 0x1C | ((v5 & 0x40 | ((v7 & 0x100 | (v6 >> 3) & 0x1E00) >> 1)) >> 1)) >> 1);
            }
            else
            {
                v9 = v7 & 0x800;
                if ( (v6 & 0x1682) == 1666 )
                    v8 = (v5 & 1 | ((v5 & 0x7C | ((v5 & 0x100 | ((v9 | (v5 >> 1) & 0x7000) >> 2)) >> 1)) >> 1)) + 128;
                else
                    v8 = (v5 & 7 | ((v5 & 0x10 | ((v5 & 0x40 | ((v5 & 0x200 | ((v9 | (v5 >> 1) & 0x7000) >> 1)) >> 2)) >> 1)) >> 1))
                        + 384;
            }
            if ( v8 > 211 )
            {
                if ( v8 > 297 )
                {
                    if ( v8 <= 343 )
                    {
                        if ( v8 == 343 )
                        {
                            return new uint[] { 0xCB872F, 0xCACC68 };
                        }
                        if ( v8 > 323 )
                        {
                            v44 = v8 - 330;
                            if ( 0 == v44 )
                            {
                                return new uint[] { 0xCADF37, 0xCAD0F8 };
                            }
                            v45 = v44 - 1;
                            if ( 0 == v45 )
                            {
                                return new uint[] { 0xCAE325, 0xCAC912 };
                            }
                            if ( v45 == 9 )
                            {
                                return new uint[] { 0xCB84FF, 0xCAD3FE };
                            }
                            return new uint[2];
                        }
                        if ( v8 == 323 )
                        {
                            return new uint[] { 0xCB0F19, 0xCACB34 };
                        }
                        v41 = v8 - 307;
                        if ( 0 == v41 )
                        {
                            return new uint[] { 0xCB050C, 0xCAC99E };
                        }
                        v42 = v41 - 1;
                        if ( 0 == v42 )
                        {
                            return new uint[] { 0xCAD5A5, 0xCACA9A };
                        }
                        v43 = v42 - 2;
                        if ( 0 == v43 )
                        {
                            return new uint[] { 0xCAD8D6, 0xCACE4C };
                        }
                        if ( v43 != 8 )
                            return new uint[2];
                        return new uint[] { 0xCAEF72, 0xCACDA4 };
                    }
                    v46 = v8 - 347;
                    if ( 0 == v46 )
                    {
                        return new uint[] { 0xCAE644, 0xCAD03C };
                    }
                    v47 = v46 - 4;
                    if ( 0 == v47 )
                    {
                        return new uint[] { 0xCAFF17, 0xCACC76 };
                    }
                    v48 = v47 - 4;
                    if ( v48 != 0 )
                    {
                        v49 = v48 - 1;
                        if ( 0 == v49 )
                        {
                            return new uint[] { 0xCAFEDB, 0xCAD603 };
                        }
                        v50 = v49 - 22;
                        if ( v50 != 0 )
                        {
                            v51 = v50 - 12;
                            if ( 0 == v51 )
                            {
                                return new uint[] { 0x5FDCF5, 0xCADA59 };
                            }
                            if ( v51 == 3 )
                            {
                                return new uint[] { 0xCAF25F, 0xCAD269 };
                            }
                            return new uint[2];
                        }
                        return new uint[] { 0xCAFC0A, 0xCAD176 };
                    }
                }
                else
                {
                    if ( v8 == 297 )
                    {
                        return new uint[] { 0xCB0D0C, 0xCAC93C };
                    }
                    if ( v8 <= 261 )
                    {
                        if ( v8 == 261 )
                        {
                            return new uint[] { 0xCB0B03, 0xCADB27 };
                        }
                        if ( v8 <= 232 )
                        {
                            if ( v8 == 232 )
                            {
                                return new uint[] { 0xCAD348, 0xCACD4B };
                            }
                            v31 = v8 - 214;
                            if ( 0 == v31 )
                            {
                                return new uint[] { 0xCB872F, 0xCACAB6 };
                            }
                            v32 = v31 - 2;
                            if ( v32 != 0 )
                            {
                                v33 = v32 - 13;
                                if ( 0 == v33 )
                                {
                                    return new uint[] { 0xCB0DAA, 0xCAD597 };
                                }
                                if ( v33 == 2 )
                                {
                                    return new uint[] { 0xCB1532, 0xCAD8C8 };
                                }
                                return new uint[2];
                            }
                            return new uint[] { 0xCB1417, 0xCAD233 };
                        }
                        v34 = v8 - 236;
                        if ( 0 == v34 )
                        {
                            return new uint[] { 0xCB0845, 0xCAC94A };
                        }
                        v35 = v34 - 6;
                        if ( v35 != 0 )
                        {
                            if ( v35 == 16 )
                            {
                                return new uint[] { 0x6090F0, 0xCACC92 };
                            }
                            return new uint[2];
                        }
                        return new uint[] { 0xCAEFEE, 0xCACFE7 };
                    }
                    if ( v8 > 286 )
                    {
                        v39 = v8 - 288;
                        if ( 0 == v39 )
                        {
                            return new uint[] { 0xCAE357, 0xCADAC2 };
                        }
                        v40 = v39 - 3;
                        if ( 0 == v40 )
                        {
                            return new uint[] { 0xCAEC79, 0xCAD3E2 };
                        }
                        if ( v40 == 5 )
                        {
                            return new uint[] { 0xCB872F, 0xCADB8E };
                        }
                        return new uint[2];
                    }
                    if ( v8 == 286 )
                    {
                        return new uint[] { 0xCAD568, 0xCAD3D4 };
                    }
                    v36 = v8 - 262;
                    if ( v36 != 0 )
                    {
                        v37 = v36 - 5;
                        if ( 0 == v37 )
                        {
                            return new uint[] { 0xCADE7A, 0xCAD89E };
                        }
                        v38 = v37 - 4;
                        if ( 0 == v38 )
                        {
                            return new uint[] { 0xCAE567, 0xCAD8AC };
                        }
                        if ( v38 == 5 )
                        {
                            return new uint[] { 0xCAE2D8, 0xCACFCB };
                        }
                        return new uint[2];
                    }
                }
                return new uint[] { 0xCB0D0C, 0xCACB26 };
            }
            if ( v8 == 211 )
            {
                return new uint[] { 0xCAE518, 0xCACBE2 };
            }
            if ( v8 > 84 )
            {
                if ( v8 <= 163 )
                {
                    if ( v8 == 163 )
                    {
                        return new uint[] { 0xCADED9, 0xCAD106 };
                    }
                    if ( v8 > 141 )
                    {
                        v24 = v8 - 142;
                        if ( 0 == v24 )
                        {
                            return new uint[] { 0x61E899, 0xCAD40C };
                        }
                        v25 = v24 - 9;
                        if ( 0 == v25 )
                        {
                            return new uint[] { 0xCAFDD8, 0xCAD33A };
                        }
                        if ( v25 == 7 )
                        {
                            return new uint[] { 0xCAD86F, 0xCAD217 };
                        }
                        return new uint[2];
                    }
                    if ( v8 == 141 )
                    {
                        return new uint[] { 0xCADF66, 0xCAD114 };
                    }
                    v21 = v8 - 89;
                    if ( v21 != 0 )
                    {
                        v22 = v21 - 10;
                        if ( 0 == v22 )
                        {
                            return new uint[] { 0xCAD9E7, 0xCAD209 };
                        }
                        v23 = v22 - 14;
                        if ( 0 == v23 )
                        {
                            return new uint[] { 0xCADBF0, 0xCAD8BA };
                        }
                        if ( v23 != 26 )
                            return new uint[2];
                        return new uint[] { 0xCAEF72, 0xCAC982 };
                    }
                    return new uint[] { 0xCB1417, 0xCAD04A };
                }
                if ( v8 > 177 )
                {
                    v29 = v8 - 182;
                    if ( 0 == v29 )
                    {
                        return new uint[] { 0xCB84FF, 0xCAD428 };
                    }
                    v30 = v29 - 16;
                    if ( 0 == v30 )
                    {
                        return new uint[] { 0xCAE596, 0xCAD0EA };
                    }
                    if ( v30 == 12 )
                    {
                        return new uint[] { 0xCB0882, 0xCAD168 };
                    }
                    return new uint[2];
                }
                if ( v8 == 177 )
                {
                    return new uint[] { 0xCAE4E9, 0xCAC974 };
                }
                v26 = v8 - 165;
                if ( 0 == v26 )
                {
                    return new uint[] { 0xCAE2A9, 0xCADB9C };
                }
                v27 = v26 - 1;
                if ( 0 == v27 )
                {
                    return new uint[] { 0xCADBC1, 0xCAC990 };
                }
                v28 = v27 - 4;
                if ( v28 != 0 )
                {
                    if ( v28 == 1 )
                    {
                        return new uint[] { 0xCADB43, 0xCACFD9 };
                    }
                    return new uint[2];
                }
                return new uint[] { 0xCAEF33, 0xCACBF0 };
            }
            if ( v8 == 84 )
            {
                return new uint[] { 0xCAEF72, 0xCAD5D4 };
            }
            if ( v8 > 44 )
            {
                if ( v8 <= 74 )
                {
                    if ( v8 == 74 )
                    {
                        return new uint[] { 0xCAE6B4, 0xCAD41A };
                    }
                    v15 = v8 - 45;
                    if ( 0 == v15 )
                    {
                        return new uint[] { 0xCAD5E2, 0xCADB35 };
                    }
                    v16 = v15 - 8;
                    if ( 0 == v16 )
                    {
                        return new uint[] { 0xCAFFBF, 0xCACE3E };
                    }
                    v17 = v16 - 14;
                    if ( 0 == v17 )
                    {
                        return new uint[] { 0xCB0B8E, 0xCAC92E };
                    }
                    if ( v17 == 1 )
                    {
                        return new uint[] { 0xCB13DA, 0xCADB72 };
                    }
                    return new uint[2];
                }
                v18 = v8 - 75;
                if ( v18 != 0 )
                {
                    v19 = v18 - 2;
                    if ( 0 == v19 )
                    {
                        return new uint[] { 0x602D1B, 0xCACD59 };
                    }
                    if ( v19 == 5 )
                    {
                        return new uint[] { 0xCAEFAF, 0xCACAA8 };
                    }
                    return new uint[2];
                }
                return new uint[] { 0xCAF0FD, 0xCACCC4 };
            }
            if ( v8 == 44 )
            {
                return new uint[] { 0xCB0D49, 0xCACB0A };
            }
            if ( v8 > 24 )
            {
                if ( v8 != 27 )
                {
                    if ( v8 == 30 )
                    {
                        return new uint[] { 0xCAEC3A, 0xCACC84 };
                    }
                    if ( v8 == 33 )
                    {
                        return new uint[] { 0x5FDCF5, 0xCAD905 };
                    }
                    return new uint[2];
                }
                return new uint[] { 0xCAFE11, 0xCAC920 };
            }
            if ( v8 == 24 )
            {
                return new uint[] { 0xCB872F, 0xCAC958 };
            }
            v10 = v8 - 3;
            if ( 0 == v10 )
            {
                return new uint[] { 0xCADF08, 0xCACB18 };
            }
            v11 = v10 - 7;
            if ( 0 == v11 )
            {
                return new uint[] { 0xCB00FE, 0xCAC966 };
            }
            v12 = v11 - 1;
            if ( 0 == v12 )
            {
                return new uint[] { 0xCAED4F, 0xCAD225 };
            }
            if ( v12 != 3 )
                return new uint[2];
            return new uint[] { 0xCAFE9E, 0xCAD3F0 };

            #endregion

            /*
            opcode = CalculateOffset(opcode);
            if (opcode > 211)
            {
                if (opcode > 297)
                {
                    if (opcode <= 343)
                    {
                        if (opcode == 343)
                            return new uint[] { 0xCB872F, 0xCACC68 };
                        if (opcode > 323)
                        {
                            opcode -= 330;
                            if (opcode == 0)
                                return new uint[] { 0xCADF37, 0xCAD0F8 };
                            opcode -= 1;
                            if (opcode == 0)
                                return new uint[] { 0xCAE325, 0xCAC912 };
                            if (opcode == 9)
                                return new uint[] { 0xCB84FF, 0xCAD3FE };
                            return new uint[2];
                        }
                        if (opcode == 323)
                            return new uint[] { 0xCB0F19, 0xCACB34 };
                        opcode -= 307;
                        if (opcode == 0)
                            return new uint[] { 0xCB050C, 0xCAC99E };
                    }
                }
            }
            
            
            // We are passed the opcode here.
            // TODO: This is disassembling the function, aka doing what IDA already does.
            // We need to find a way to execute it (see Utils.AsDelegate(...) *cough*)
            // And INTERCEPT whenever a CALL opcode is emitted. Right now, invoking
            // the delegate causes an SEHException, which is fine and all, but
            // doesn't let us find out the offset of the call.
            // Basically we need MDbgEngine.
            /*var functionLength = 0x00CAC5E8 - 0x00CAB966;
            wow.Seek(0x00CAB966 - 0x400C00, SeekOrigin.Begin);
            var functionBytes = new byte[functionLength];
            wow.Read(functionBytes, 0, functionLength);
            
            var disasm = new Disasm();
            disasm.EIP = new IntPtr(0x00CAB966 + wowDisasm.Ptr.ToInt64() - 0x400C00);
            var bytesTally = 0;

            while (bytesTally < functionLength)
            {
                int result = BeaEngine.Disasm(disasm);
                if (result == (int)BeaConstants.SpecialInfo.UNKNOWN_OPCODE)
                    return new uint[2];

                //if (Program.Debug)
                Console.WriteLine("0x{0:X8} {1}", (disasm.EIP.ToInt64() - wowDisasm.Ptr.ToInt64() + 0x400C00), disasm.CompleteInstr);
                
                if (disasm.CompleteInstr.IndexOf("call") != -1)
                {
                    var offset = BitConverter.GetBytes((int)disasm.Instruction.AddrValue);
                    var newBytes = __asm {
                        
                    }
                }

                bytesTally += result;
                disasm.EIP = new IntPtr(disasm.EIP.ToInt64() + result);
            }
            return new uint[2];*/
        }
    }
}
