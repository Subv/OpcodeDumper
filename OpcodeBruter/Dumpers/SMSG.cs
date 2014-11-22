using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bea;

namespace OpcodeBruter
{
    /// <summary>
    /// Description of CliOpcodes.
    /// </summary>
    public class SMSG
    {
        private static byte[] Pattern = new byte[] {
            0x55,                             // push    ebp
            0x8B, 0xEC,                       // mov     ebp, esp
            0x56,                             // push    esi
            0x8B, 0xF1,                       // mov     esi, ecx
            0x8B, 0x4D, 0x08,                 // mov     ecx, [ebp + 8]
            0x68, 0xFF, 0xFF, 0x00, 0x00,     // push    <opcode>
            0xE8, 0xFF, 0xFF, 0xFF, 0xFF      // call    CDataStore::PutInt32
        };

        public static void Dump()
        {
            var jamGroupCount = new Dictionary<JamGroup, uint>();
            var opcodesCount = 0;

            Logger.WriteLine();
            Logger.WriteLine("Dumping SMSG opcodes...");
            Logger.WriteLine("+---------------+-------------+-------------+--------------------+---------+");
            Logger.WriteLine("|     Opcode    |  JAM Parser | Jam Handler |     Group Name     | ConnIdx |");
            Logger.WriteLine("+---------------+-------------+-------------+--------------------+---------+");

            if (Config.SpecificOpcodeValue == 0xBADD)
            {
                for (uint i = 0; i < 0x1FFF; ++i)
                    if (DumpOpcode(i, jamGroupCount))
                        ++opcodesCount;
            }
            else if (DumpOpcode(Config.SpecificOpcodeValue, jamGroupCount))
                ++opcodesCount;

            Logger.WriteLine("+---------------+-------------+-------------+--------------------+---------+");
            Logger.WriteLine(@"Dumped {0} SMSG JAM opcodes.", opcodesCount);
            for (var i = 0; i < jamGroupCount.Count; ++i)
                Logger.WriteLine("Dumped {0} SMSG {1} opcodes.", jamGroupCount.Values.ElementAt(i), jamGroupCount.Keys.ElementAt(i).ToString());
        }
        
        private static bool DumpOpcode(uint opcode, Dictionary<JamGroup, uint> jamGroupCount)
        {
            //! NOTE: All the ESP modifications are caused by the fact that calling
            //! does not push the return address on the stack.
            foreach (var dispatcherPair in Program.Dispatchers)
            {
                if (dispatcherPair.Key == JamGroup.None)
                    continue;

                var dispatcher = dispatcherPair.Value;
                if (dispatcher.CalculateCheckerFn() == 0)
                    continue;

                int offset = dispatcher.StructureOffset;
                if (offset <= 0)
                    continue;

                int checkerFn    = dispatcher.CalculateCheckerFn();
                int connectionFn = dispatcher.CalculateConnectionFn();
                int dispatcherFn = dispatcher.CalculateDispatcherFn();

                Program.Environment.Reset();
                Program.Environment.Push(opcode);
                Program.Environment.Esp.Value -= 4;
                Program.Environment.Execute(checkerFn, Program.Disasm, false);
                if (Program.Environment.Eax.Value == 0)
                    continue;

                var connIndex = 0u;
                if (connectionFn != 0)
                {
                    Program.Environment.Reset();
                    Program.Environment.Push();
                    Program.Environment.Push();
                    Program.Environment.Push();
                    Program.Environment.Push(opcode);
                    Program.Environment.Push();
                    Program.Environment.Esp.Value -= 4;
                    Program.Environment.Execute(connectionFn, Program.Disasm, false);
                    if (Program.Environment.Eax.Al == 0)
                    {
                        var requiresInstanceConnectionFn = Program.Environment.GetCalledOffsets()[0] - 0x400C00;

                        Program.Environment.Reset();
                        Program.Environment.Push(opcode);
                        Program.Environment.Esp.Value -= 4;
                        Program.Environment.Execute(requiresInstanceConnectionFn, Program.Disasm, false);
                        connIndex = Program.Environment.Eax.Value;
                    }
                }

                Program.Environment.Reset();
                Program.Environment.Execute(dispatcherFn, Program.Disasm, false);
                var calleeOffset = Program.Environment.GetCalledOffsets()[0] - 0x400C00;

                Program.Environment.Reset();
                Program.Environment.Push();
                Program.Environment.Push((ushort)0);
                Program.Environment.Push((ushort)opcode);
                Program.Environment.Push();
                Program.Environment.Push();
                Program.Environment.Esp.Value -= 4;
                Program.Environment.Execute(calleeOffset, Program.Disasm, false);
                var jamData = Program.Environment.GetCalledOffsets();
                if (jamData.Length < 2)
                    continue;

                var handler = jamData[0];
                var parser  = jamData[1];
                switch (dispatcher.GetGroup())
                {
                    case JamGroup.Client:
                    case JamGroup.ClientChat:
                    case JamGroup.ClientGuild:
                    case JamGroup.ClientQuest:
                        handler = jamData[1];
                        parser  = jamData[2];
                        break;
                }

                if (!jamGroupCount.ContainsKey(dispatcher.GetGroup()))
                    jamGroupCount.Add(dispatcher.GetGroup(), 0);
                jamGroupCount[dispatcher.GetGroup()] += 1;

                Logger.WriteLine("| {4} (0x{0:X4}) |  0x{1:X8} |  0x{2:X8} | {3} | {6} | {5}",
                                 opcode,
                                 handler, parser,
                                 dispatcher.GetGroup().ToString().PadLeft(18),
                                 opcode.ToString().PadLeft(4),
                                 Opcodes.GetOpcodeNameForServer(opcode),
                                 connIndex.ToString().PadLeft(7));
                return true;
            }
            return false;
        }
    }
}
