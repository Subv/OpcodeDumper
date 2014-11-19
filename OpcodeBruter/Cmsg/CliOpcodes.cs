using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bea;

namespace OpcodeBruter.Cmsg
{
    /// <summary>
    /// Description of CliOpcodes.
    /// </summary>
    public class CliOpcodes : IDisposable
    {
        public BinaryReader Binary;
        public byte[] WowBytes;
        public List<uint> PatternOffsets;
        public uint[] OffsetIndex;

        private static byte[] Pattern = new byte[] {
            0x55,                             // push    ebp
            0x8B, 0xEC,                       // mov     ebp, esp
            0x56,                             // push    esi
            0x8B, 0xF1,                       // mov     esi, ecx
            0x8B, 0x4D, 0x08,                 // mov     ecx, [ebp + 8]
            0x68, 0xFF, 0xFF, 0x00, 0x00,     // push    <opcode>
            0xE8, 0xFF, 0xFF, 0xFF, 0xFF      // call    CDataStore::PutInt32
        };

        public CliOpcodes(FileStream wow, uint specificOpcode = 0xBADD)
        {
            this.WowBytes = File.ReadAllBytes(wow.Name);
            this.Binary = new BinaryReader(new MemoryStream(this.WowBytes));
            this.PatternOffsets = this.WowBytes.FindPattern(Pattern, 0xFF);

            var callIndex = (uint)(Array.IndexOf<byte>(Pattern, 0xE8) + 0x400C00);
            var opcodeCount = 0;
            Logger.WriteLine("+---------------+------------+");
            Logger.WriteLine("|    Opcode     |   CliPut   |");
            Logger.WriteLine("+---------------+------------+");

            Logger.WriteLine("Found {0} CMSGs candidates. Dumping, this may take a while...", this.PatternOffsets.Count);

            foreach (var currPatternOffset in this.PatternOffsets)
            {
                Binary.BaseStream.Seek(currPatternOffset, SeekOrigin.Begin);
                var bytes = Binary.ReadBytes(Pattern.Length);

                var callOffset = BitConverter.ToInt32(bytes, 15);
                var subCall = (uint)(currPatternOffset + callIndex) + BitConverter.ToUInt32(bytes, 15) + 5;

                if (subCall != 0x40FCE6) // CDataStore::PutInt32
                    continue;

                var opcodeValue = BitConverter.ToUInt16(bytes, 10);
                if (specificOpcode != 0xBADD && specificOpcode != opcodeValue)
                    continue;

                var ptBytes = BitConverter.GetBytes(currPatternOffset + 0x400C00);
                var vtablePattern = new byte[] {
                    0xFF, 0xFF, 0xFF, 0xFF, // Ctor
                    0xFF, 0xFF, 0xFF, 0xFF, // CliPut
                    ptBytes[0], ptBytes[1], ptBytes[2], ptBytes[3], // CliPutWithMsgId (where we are at)
                    0xFF, 0xFF, 0xFF, 0xFF  // Dtor
                };

                var vtOffsets = WowBytes.FindPattern(vtablePattern, 0xFF).Where(t => (t - 0x400C00) < this.WowBytes.Length);
                foreach (var vtOffset in vtOffsets)
                {
                    Binary.BaseStream.Seek(vtOffset, SeekOrigin.Begin);
                    var ctor = Binary.ReadUInt32();
                    var cliPut = Binary.ReadUInt32();
                    var cliPutWithMsg = Binary.ReadUInt32();
                    if (cliPutWithMsg != (currPatternOffset + 0x400C00))
                        continue;

                    Logger.WriteLine("| {0} (0x{1:X4}) | 0x{2:X8} | {3}",
                                  opcodeValue.ToString().PadLeft(4),
                                  opcodeValue,
                                  cliPut,
                                  Opcodes.GetOpcodeNameForClient(opcodeValue));
                    ++opcodeCount;
                    break;
                }
            }

            Logger.WriteLine("+---------------+------------+");
            Logger.WriteLine("Dumped {0} CMSG JAM opcodes.", opcodeCount);
        }

        public void Dispose()
        {
            Binary.Close();
        }
    }
}
