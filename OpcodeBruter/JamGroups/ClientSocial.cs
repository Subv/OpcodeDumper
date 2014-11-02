using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace OpcodeBruter.JamGroups
{
    public class ClientSocial : JamDispatch
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
            return ((opcode - 1) & 0x1C86) == 1156;
        }

        public ClientSocial(FileStream wow)
            : base(wow)
        {

        }

        public override uint CalculateHandler(uint opcode)
        {
            return 0x05E7E7B; // This one doesn't have a jump table
        }

        public override int CalculateOffset(uint opcode)
        {
            var v5 = (opcode - 1) & 1;
            return (int)(v5 | (((opcode - 1) & 0x78 | (((opcode - 1) & 0x300 | ((opcode - 1) >> 3) & 0x1C00) >> 1)) >> 2));
        }
        
        public override uint[] CalculateJamFunctionOffsets(uint opcode)
        {
            #region Imported from Hex-Rays
            var returnValues = new List<uint>();
            uint v5; // edx@1

            v5 = (opcode - 1) & 1;
            switch ( v5 | (((opcode - 1) & 0x78 | (((opcode - 1) & 0x300 | ((opcode - 1) >> 3) & 0x1C00) >> 1)) >> 2) )
            {
                case 30:
                    returnValues.Add(0x62944E);
                    returnValues.Add(0x6290FA);
                    returnValues.Add(0x5E7E5F);
                    break;
                case 47:
                    returnValues.Add(0x629522);
                    returnValues.Add(0x629108);
                    returnValues.Add(0x5E7E44);
                    break;
                case 65:
                    returnValues.Add(0x62956E);
                    returnValues.Add(0x6290EC);
                    returnValues.Add(0x40DF84);
                    break;
                case 66:
                    returnValues.Add(0xCAD5E2);
                    returnValues.Add(0x6290D0);
                    break;
                case 72:
                    returnValues.Add(0x6294D6);
                    returnValues.Add(0x6290DE);
                    returnValues.Add(0x5E7E29);
                    break;
                default:
                    if ( (v5 | (((opcode - 1) & 0x78 | (((opcode - 1) & 0x300 | ((opcode - 1) >> 3) & 0x1C00) >> 1)) >> 2)) != 77 )
                        return new uint[2];
                    returnValues.Add(0x5F4D75);
                    returnValues.Add(0x629116);
                    break;
            }
            return returnValues.ToArray();
            #endregion
        }
    }
}
