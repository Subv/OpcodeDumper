using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace OpcodeBruter
{
    public abstract class JamDispatch
    {
        public List<uint> Offsets = new List<uint>();
        public List<uint> IndirectJumpTable = new List<uint>();

        public abstract int JumpTableAddress { get; }
        public abstract int JumpTableSize { get; }
        public abstract int IndirectJumpTableAddress { get; }
        public abstract int IndirectJumpTableSize { get; }

        public abstract int CalculateOffset(uint opcode);
        public abstract uint CalculateHandler(uint opcode);

        public JamDispatch(FileStream wow)
        {
            if (JumpTableSize > 0)
            {
                byte[] data = new byte[JumpTableSize * 4];
                wow.Seek(JumpTableAddress, SeekOrigin.Begin);
                wow.Read(data, 0, JumpTableSize * 4);

                if (IndirectJumpTableSize != 0)
                {
                    byte[] indirectData = new byte[IndirectJumpTableSize];
                    wow.Seek(IndirectJumpTableAddress, SeekOrigin.Begin);
                    wow.Read(indirectData, 0, IndirectJumpTableSize);

                    for (int i = 0; i < IndirectJumpTableSize; ++i)
                        IndirectJumpTable.Add(indirectData[i]);
                }

                for (int i = 0; i < JumpTableSize * 4; i += 4)
                    Offsets.Add(BitConverter.ToUInt32(data, i));
            }
        }
    }
}
