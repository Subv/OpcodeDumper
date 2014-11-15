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
    public class CliOpcodes
    {
        public BinaryReader Binary;
        public byte[] WowBytes;
        public List<uint> PatternOffsets;
        public uint[] OffsetIndex;

        private static byte[] CtorPattern = new byte[] {
            0x8B, 0xFF,                         // mov eax, ecx
            0x83, 0xFF, 0xFF, 0x00,             // and dword ptr [? + ?], 0
            0xC7, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // mov dword ptr [?S], offset ?
            0xC3                                // retn
        };
        
        private static byte[] OtherPattern = new byte[] {
            // 0x56,                            // push esi
            0x8B, 0xFF,                         // mov esi, ecx
            0x83, 0xFF, 0xFF, 0x00,             // and dword ptr [? + ?], 0
            0x8D, 0xFF, 0xFF,                   // lea ?, [? + ?]
            0xC7, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // mov dword ptr [?], offset ?
        };
        
        private static byte[] YetAnotherPattern = new byte[] {
            0x8B, 0xFF,                         // mov eax, ecx
            0x83, 0xFF, 0xFF, 0x00,             // and dword ptr [? + ?], 0
            0xC7, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // mov dword ptr [?], offset ?
            0xC6, 0xFF, 0xFF, 0xFF,             // byte ptr [? + ?], ?
            0xC3                                // retn
        };
        
        private static byte[] AnotherFkinPattern = new byte[] {
            0x55,                               // push ebp
            0x8B, 0xEC,                         // mov ebp, esp
            0xF6, 0x45, 0x08, 0x01,             // test [ebp + 8h], 1
            0x56,                               // push esi
            0x8B, 0xFF,                         // mov ?, ?
            0xC7, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // mov dword ptr [?], offset ?
            0x74, 0xFF,                         // jz ?
            0x56,                               // push esi
        };
        
        private static byte[] Pattern = new byte[] {
            0x55,                             // push    ebp
            0x8B, 0xEC,                       // mov     ebp, esp
            0x56,                             // push    esi
            0x8B, 0xF1,                       // mov     esi, ecx
            0x8B, 0x4D, 0x08,                 // mov     ecx, [ebp + 8]
            0x68, 0xFF, 0xFF, 0x00, 0x00,     // push    <opcode>
            0xE8, 0xFF, 0xFF, 0xFF, 0xFF      // call    CDataStore::PutInt32
        };
        
        public CliOpcodes(FileStream wow, uint specificOpcode = 0x0000)
        {
            this.WowBytes = File.ReadAllBytes(wow.Name);
            this.Binary = new BinaryReader(new MemoryStream(this.WowBytes));
            this.PatternOffsets = this.WowBytes.FindPattern(Pattern, 0xFF);
            Console.WriteLine("Found {0} candidate CliPutWithMsgId functions. Analyzing, this will take a while.", this.PatternOffsets.Count);

            var opcodeDict = new List<CmsgData>(this.PatternOffsets.Count);

            foreach (var currPatternOffset in this.PatternOffsets)
            {
                Binary.BaseStream.Seek(currPatternOffset, SeekOrigin.Begin);
                var bytes = Binary.ReadBytes(Pattern.Length);

                var callOffset = BitConverter.ToInt32(bytes, 15);
                uint subCall = (uint)(currPatternOffset);
                subCall += (uint)(0x400C00 + Array.IndexOf<byte>(Pattern, 0xE8));
                subCall = subCall + (uint)callOffset;
                subCall += 5;
                
                if (subCall != 0x40FCE6) // CDataStore::PutInt32
                    continue;

                var opcodeValue = BitConverter.ToUInt16(bytes, 10);
                if (specificOpcode != 0x0000 && specificOpcode != opcodeValue)
                    continue;

                var ptBytes = BitConverter.GetBytes(currPatternOffset + 0x400C00);
                var vtablePattern = new byte[] {
                    0xFF, 0xFF, 0xFF, 0xFF, // Ctor
                    0xFF, 0xFF, 0xFF, 0xFF, // CliPut
                    ptBytes[0], ptBytes[1], ptBytes[2], ptBytes[3], // CliPutWithMsgId (where we are at)
                    0xFF, 0xFF, 0xFF, 0xFF  // Dtor                    
                };
                
                var vtOffsets = WowBytes.FindPattern(vtablePattern, 0xFF).Where(t => (t - 0x400C00) < this.WowBytes.Length);
                if (vtOffsets.Count() > 1)
                    Console.WriteLine("DEBUG: MULTIPLE MATCHES FOR OPCODE 0x{0:X4}", opcodeValue);
                foreach (var vtOffset in vtOffsets)
                {
                    Binary.BaseStream.Seek(vtOffset, SeekOrigin.Begin);
                    var ctor = Binary.ReadUInt32();
                    var cliPut = Binary.ReadUInt32();
                    var cliPutWithMsg = Binary.ReadUInt32();
                    if (cliPutWithMsg != (currPatternOffset + 0x400C00))
                        continue;
                    opcodeDict.Add(new CmsgData(opcodeValue, ctor, cliPut, cliPutWithMsg, Binary.ReadUInt32(), vtOffset));
                }
            }
            
            Console.WriteLine("+---------------+------------+------------+");
            Console.WriteLine("|    Opcode     |   VTable   |   CliPut   |");
            Console.WriteLine("+---------------+------------+------------+");
            foreach (var record in opcodeDict.OrderBy(k => k.OpcodeValue))
                Console.WriteLine("| {0} (0x{1:X4}) | 0x{2:X8} | 0x{3:X8} | {4}",
                                  record.OpcodeValue.ToString().PadLeft(4),
                                  record.OpcodeValue,
                                  record.Vtable,
                                  record.CliPutOffset,
                                  Opcodes.GetOpcodeNameForClient(record.OpcodeValue));
            Console.WriteLine("+---------------+------------+------------+");
            Console.WriteLine("Dumped {0} CMSG JAM opcodes.", opcodeDict.Count);

            // Integrity check
            var foundOpcs = 0;
            foreach (var record in opcodeDict)
                if (Opcodes.GetOpcodeNameForClient(record.OpcodeValue) != string.Empty)
                    ++foundOpcs;
            if (foundOpcs != Opcodes.CMSG.Count)
                Console.WriteLine("Found {0} WPP CMSGs opcodes over {1}.", foundOpcs, Opcodes.CMSG.Count);
            else
                Console.WriteLine("All CMSGs defined in WPP have been found.");
        }
    }
    
    internal sealed class CmsgData
    {
        public uint OpcodeValue;
        public uint CtorOffset;
        public uint CliPutOffset;
        public uint CliPutWithMsgIdOffset;
        public uint DtorOffset;
        public uint Vtable;
        
        public CmsgData(uint Opcode, uint Ctor, uint CliPut, uint CliPutWithMsgId, uint Dtor, uint Vtable)
        {
            this.OpcodeValue = Opcode;
            this.CtorOffset = Ctor;
            this.CliPutOffset = CliPut;
            this.CliPutWithMsgIdOffset = CliPutWithMsgId;
            this.DtorOffset = Dtor;
            this.Vtable = Vtable;
        }
    }
}
