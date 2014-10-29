using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Bea;

namespace OpcodeBruter
{
    public abstract class JamDispatch
    {
        protected FileStream wow;
        protected UnmanagedBuffer wowDisasm;
        
        public List<uint> Offsets = new List<uint>();
        public List<uint> IndirectJumpTable = new List<uint>();

        /// <summary>
        /// Address of the jump table for that group.
        /// </summary>
        public abstract int JumpTableAddress { get; }
        
        /// <summary>
        /// Size of the jump table for that group.
        /// </summary>
        public abstract int JumpTableSize { get; }

        /// <summary>
        /// Address of the indirect jump table for that group.
        /// </summary>
        public abstract int IndirectJumpTableAddress { get; }
        
        /// <summary>
        /// Size of the indirect jump table for that group.
        /// </summary>
        public abstract int IndirectJumpTableSize { get; }

        public abstract int CalculateOffset(uint opcode);
        public abstract uint CalculateHandler(uint opcode);
        
        /// <summary>
        /// Calculates the IDA address of the JAM handler, and parser, of a given opcode
        /// </summary>
        public virtual uint[] CalculateJamFunctionOffsets(uint addressOrOpcode)
        {
            return new uint[2];
        }

        public JamDispatch(FileStream wow)
        {
            this.wow = wow;
            wowDisasm = UnmanagedBuffer.CreateFromFile(wow);
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
