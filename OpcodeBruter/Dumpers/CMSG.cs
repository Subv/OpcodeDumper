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

        private static byte[] SecondPattern = new byte[] {
            0x55,                             // push    ebp
            0x8B, 0xEC,                       // mov     ebp, esp
            0x56,                             // push    esi
            0x8B, 0xF1,                       // mov     esi, ecx
            0x8B, 0x4D, 0x08,                 // mob     ecx, [ebp + 8]
            0x6A, 0xFF,                       // push    <opcode>
            0xE8, 0xFF, 0xFF, 0xFF, 0xFF      // call    CDataStore::PutInt32
        };

        public static void Dump(uint specificOpcode = 0xBADD)
        {
            Logger.WriteLine();
            Logger.WriteLine(">> Dumping CMSG opcodes...");
            Logger.WriteLine("+---------------+------------+-------------+---------------+");
            Logger.WriteLine("|    Opcode     |   CliPut   | Constructor | Vtable offset |");
            Logger.WriteLine("+---------------+------------+-------------+---------------+");
            var opcodeCount = DumpFirstPattern(specificOpcode);
            opcodeCount += DumpSecondPattern(specificOpcode);
            Logger.WriteLine("+---------------+------------+-------------+---------------+");
            Logger.WriteLine("Dumped {0} CMSG JAM opcodes.", opcodeCount);
        }

        private static int DumpFirstPattern(uint specificOpcode = 0xBADD)
        {
            if (specificOpcode != 0xBADD)
            {
                Pattern[11] = (byte)((specificOpcode & 0xFF00) >> 8);
                Pattern[10] = (byte)(specificOpcode & 0x00FF);
            }

            var patternOffsets = Program.ClientBytes.FindPattern(Pattern, 0xFF);
            var callIndex = (uint)(Array.IndexOf<byte>(Pattern, 0xE8) + 0x400C00);
            var opcodeCount = 0;

            foreach (var currPatternOffset in patternOffsets)
            {
                Program.BaseStream.Seek(currPatternOffset, SeekOrigin.Begin);
                var bytes = Program.ClientStream.ReadBytes(Pattern.Length);

                var callOffset = BitConverter.ToInt32(bytes, 15);

                // Check isn't needed - use it if IDA shows some false positives.
                // Happens for second pattern, so just be safe.
                var subCall = (uint)(currPatternOffset + callIndex) + callOffset + 5;
                if (subCall != 0x00411144) // CDataStore::PutInt32
                    continue;

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
                    Program.BaseStream.Seek(vtOffset, SeekOrigin.Begin);
                    var ctor = Program.ClientStream.ReadUInt32();
                    var cliPut = Program.ClientStream.ReadUInt32();
                    var cliPutWithMsg = Program.ClientStream.ReadUInt32();
                    if (cliPutWithMsg != (currPatternOffset + 0x400C00))
                        continue;

                    Logger.WriteLine("| {0} (0x{1:X4}) | 0x{2:X8} |  0x{3:X8} |   0x{4:X8}  | {5}",
                                  opcodeValue.ToString().PadLeft(4),
                                  opcodeValue,
                                  cliPut,
                                  ctor,
                                  vtOffset + 0x401400,
                                  Opcodes.GetOpcodeNameForClient(opcodeValue));
                    ++opcodeCount;
                    break;
                }
            }

            return opcodeCount;
        }

        private static int DumpSecondPattern(uint specificOpcode = 0xBADD)
        {
            if (specificOpcode != 0xBADD)
            {
                // Doesn't fit.
                if (specificOpcode > 0xFF)
                    return 0;

                SecondPattern[10] = (byte)(specificOpcode & 0x00FF);
            }

            var patternOffsets = Program.ClientBytes.FindPattern(SecondPattern, 0xFF);
            var callIndex = (uint)(Array.IndexOf<byte>(SecondPattern, 0xE8) + 0x400C00);
            var opcodeCount = 0;

            foreach (var currPatternOffset in patternOffsets)
            {
                Program.BaseStream.Seek(currPatternOffset, SeekOrigin.Begin);
                var bytes = Program.ClientStream.ReadBytes(Pattern.Length);

                var callOffset = BitConverter.ToInt32(bytes, 15);

                // Check isn't needed - use it if IDA shows some false positives.
                // Happens for second pattern, so just be safe.
                var subCall = (uint)(currPatternOffset + callIndex) + callOffset + 5;
                if (subCall != 0x00411144) // CDataStore::PutInt32
                    continue;

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
                    Program.BaseStream.Seek(vtOffset, SeekOrigin.Begin);
                    var ctor = Program.ClientStream.ReadUInt32();
                    var cliPut = Program.ClientStream.ReadUInt32();
                    var cliPutWithMsg = Program.ClientStream.ReadUInt32();
                    if (cliPutWithMsg != (currPatternOffset + 0x400C00))
                        continue;

                    Logger.WriteLine("| {0} (0x{1:X4}) | 0x{2:X8} |  0x{3:X8} |   0x{4:X8}  | {5}",
                                  opcodeValue.ToString().PadLeft(4),
                                  opcodeValue,
                                  cliPut,
                                  ctor,
                                  vtOffset + 0x401400,
                                  Opcodes.GetOpcodeNameForClient(opcodeValue));
                    ++opcodeCount;
                    break;
                }
            }

            return opcodeCount;
        }
    }
}
