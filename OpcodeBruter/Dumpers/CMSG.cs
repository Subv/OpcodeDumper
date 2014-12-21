using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bea;

namespace OpcodeBruter
{
    public class CMSG
    {
        //! DOUBLECHECK THIS FOR ANY BUILD
        private static byte[] Pattern = new byte[] {
            0x55,                             // push    ebp
            0x8B, 0xEC,                       // mov     ebp, esp
            0x56,                             // push    esi
            0x8B, 0xF1,                       // mov     esi, ecx
            0x8B, 0x4D, 0x08,                 // mov     ecx, [ebp + 8]
            0x68, 0xFF, 0xFF, 0x00, 0x00,     // push    <opcode>
            0xE8, 0xFF, 0xFF, 0xFF, 0xFF      // call    CDataStore::PutInt32
        };

        public static void Dump(uint specificOpcode = 0xBADD)
        {
            if (specificOpcode != 0xBADD)
            {
                Pattern[11] = (byte)((specificOpcode & 0xFF00) >> 8);
                Pattern[10] = (byte)(specificOpcode & 0x00FF);
                Console.WriteLine(BitConverter.ToString(Pattern).Replace('-', ' '));
            }

            var patternOffsets = Program.ClientBytes.FindPattern(Pattern, 0xFF);
            var callIndex = (uint)(Array.IndexOf<byte>(Pattern, 0xE8) + 0x400C00);
            var opcodeCount = 0;

            Logger.WriteLine();
            Logger.WriteLine("Dumping CMSG opcodes...");
            Logger.WriteLine("Found {0} CMSGs candidates. Dumping, this may take a while...", patternOffsets.Count);
            Logger.WriteLine("+---------------+------------+");
            Logger.WriteLine("|    Opcode     |   CliPut   |");
            Logger.WriteLine("+---------------+------------+");

            foreach (var currPatternOffset in patternOffsets)
            {
                Program.BaseStream.Seek(currPatternOffset, SeekOrigin.Begin);
                var bytes = Program.ClientStream.ReadBytes(Pattern.Length);

                var callOffset = BitConverter.ToInt32(bytes, 15);

                // Check isn't needed - use it if IDA shows some false positives.
                // var subCall = (uint)(currPatternOffset + callIndex) + callOffset + 5;
                // if (subCall != 0x0040FB81) // CDataStore::PutInt32
                //     continue;

                var opcodeValue = specificOpcode == 0xBADD ? BitConverter.ToUInt16(bytes, 10) : specificOpcode;
                var ptBytes = BitConverter.GetBytes(currPatternOffset + 0x400C00);
                var vtablePattern = new byte[] {
                    0xFF, 0xFF, 0xFF, 0xFF, // Ctor
                    0xFF, 0xFF, 0xFF, 0xFF, // CliPut
                    ptBytes[0], ptBytes[1], ptBytes[2], ptBytes[3], // CliPutWithMsgId (where we are at)
                    0xFF, 0xFF, 0xFF, 0xFF  // Dtor
                };

                var vtOffsets = Program.ClientBytes.FindPattern(vtablePattern, 0xFF).Where(t => (t - 0x400C00) < Program.ClientBytes.Length);
                foreach (var vtOffset in vtOffsets)
                {
                    Program.BaseStream.Seek(vtOffset + 4, SeekOrigin.Begin);
                    var cliPut = Program.ClientStream.ReadUInt32();
                    var cliPutWithMsg = Program.ClientStream.ReadUInt32();
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
    }
}
