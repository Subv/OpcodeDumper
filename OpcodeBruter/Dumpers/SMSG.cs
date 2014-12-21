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
                    Program.Environment.Execute(connectionFn, Program.Disasm, false);
                    if (Program.Environment.Eax.Al == 0)
                    {
                        var requiresInstanceConnectionFn = Program.Environment.GetCalledOffsets()[0] - 0x400C00;

                        Program.Environment.Reset();
                        Program.Environment.Push(opcode);
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
