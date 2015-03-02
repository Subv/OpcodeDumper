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
            Logger.WriteLine(">> Dumping SMSG opcodes...");
            Logger.WriteLine("+---------------+-------------+-------------+--------------------+---------+---------+");
            Logger.WriteLine("|     Opcode    |  JAM Parser | Jam Handler |     Group Name     | ConnIdx | Skipped |");
            Logger.WriteLine("+---------------+-------------+-------------+--------------------+---------+---------+");

            if (Config.SpecificOpcodeValue == 0xBADD)
            {
                for (uint i = 0; i < 0x1FFF; ++i)
                    if (DumpOpcode(i, jamGroupCount))
                        ++opcodesCount;
            }
            else if (DumpOpcode(Config.SpecificOpcodeValue, jamGroupCount))
                ++opcodesCount;

            Logger.WriteLine("+---------------+-------------+-------------+--------------------+---------+---------+");
            Logger.WriteLine(@"Dumped {0} SMSG JAM opcodes.", opcodesCount);
            foreach (var pair in jamGroupCount)
                Logger.WriteLine("Dumped {0} SMSG {1} opcodes.", pair.Value, pair.Key);
            Logger.WriteLine();
            Logger.WriteLine("ConnIdx: The connexion index for the opcode (HasFlag_FromInstanceRealm).");
            Logger.WriteLine("Skipped: Is the opcode skipped if flooded? (HasFlag_IsSkippedForExcessCount).");
        }
        
        private static bool DumpOpcode(uint opcode, Dictionary<JamGroup, uint> jamGroupCount)
        {
            foreach (var dispatcher in Program.Dispatchers)
            {
                if (dispatcher.GetGroup() == JamGroup.None || dispatcher.CalculateCheckerFn() == 0 || dispatcher.StructureOffset <= 0)
                    continue;

                int connectionFn = dispatcher.CalculateConnectionFn();

                Program.Env.Reset();
                Program.Env.Push(opcode);
                Program.Env.Execute(dispatcher.CalculateCheckerFn(), Program.Disasm, false);
                if (Program.Env.Eax.Value == 0)
                    continue;

                var connIndex = 0u;
                if (connectionFn != 0)
                {
                    Program.Env.Reset();
                    Program.Env.Push();
                    Program.Env.Push();
                    Program.Env.Push();
                    Program.Env.Push(opcode);
                    Program.Env.Push();
                    Program.Env.Execute(connectionFn, Program.Disasm, false);
                    if (Program.Env.Eax.Al == 0)
                    {
                        var requiresInstanceConnectionFn = Program.Env.GetCalledOffsets()[0] - 0x400C00;

                        Program.Env.Reset();
                        Program.Env.Push(opcode);
                        Program.Env.Execute(requiresInstanceConnectionFn, Program.Disasm, false);
                        connIndex = Program.Env.Eax.Value;
                    }
                }

                if (!dispatcher.DispatcherUpdated)
                {
                    // No need to setup the stack, the control flow always executes E8.
                    Program.Env.Reset();
                    Program.Env.Execute(dispatcher.CalculateDispatcherFn(), Program.Disasm, false);
                    dispatcher.UpdateDispatcherFn((int)(Program.Env.GetCalledOffsets()[0] - 0x400C00));
                }

                Program.Env.Reset();
                Program.Env.Push();
                Program.Env.Push((ushort)0);
                Program.Env.Push((ushort)opcode);
                Program.Env.Push();
                Program.Env.Push();
                Program.Env.Execute(dispatcher.CalculateDispatcherFn(), Program.Disasm, false);
                var jamData = Program.Env.GetCalledOffsets();
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

                var isSkippedForExcessCount = false;
                Program.Env.Reset();
                Program.Env.Push(opcode);
                Program.Env.Execute(dispatcher.CalculateSkippedFn(), Program.Disasm, false);
                if (Program.Env.GetCalledOffsets().Length != 0)
                {
                    var skippedData = Program.Env.GetCalledOffsets()[0] - 0x400C00;
                    Program.Env.Reset();
                    Program.Env.Push(opcode);
                    Program.Env.Execute(skippedData, Program.Disasm, false);
                    isSkippedForExcessCount = Program.Env.Eax.Al == 1;
                }

                if (!jamGroupCount.ContainsKey(dispatcher.GetGroup()))
                    jamGroupCount.Add(dispatcher.GetGroup(), 0);
                jamGroupCount[dispatcher.GetGroup()] += 1;

                Logger.WriteLine("| {0} (0x{1:X4}) |  0x{2:X8} |  0x{3:X8} | {4} | {5} | {6} | {7}",
                                 opcode.ToString().PadLeft(4),
                                 opcode,
                                 handler, parser,
                                 dispatcher.GetGroup().ToString().PadLeft(18),
                                 connIndex.ToString().PadLeft(7),
                                 (isSkippedForExcessCount ? "Yes" : "No").PadLeft(7),
                                 Opcodes.GetOpcodeNameForServer(opcode));
                return true;
            }
            return false;
        }
    }
}
