using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using x86;

namespace OpcodeBruter
{
    class Program
    {
        public static Dictionary<JamGroup, JamDispatch> Dispatchers = new Dictionary<JamGroup, JamDispatch>();
        public static Dictionary<uint, uint> OpcodeToFileOffset = new Dictionary<uint, uint>();

        public static Emulator environment;

        public static byte[][] ClientGroupPatterns = new byte[][] {
            new byte[] { 0x00, 0x43, 0x6C, 0x69, 0x65, 0x6E, 0x74, 0x00 }, // Client
            new byte[] { 0x00, 0x43, 0x6C, 0x69, 0x65, 0x6E, 0x74, 0x43, 0x68, 0x61, 0x74, 0x00 }, // ClientChat
            new byte[] { 0x00, 0x43, 0x6C, 0x69, 0x65, 0x6E, 0x74, 0x47, 0x61, 0x72, 0x72, 0x69, 0x73, 0x6F, 0x6E, 0x00 }, // ClientGarrison
            new byte[] { 0x00, 0x43, 0x6C, 0x69, 0x65, 0x6E, 0x74, 0x47, 0x75, 0x69, 0x6C, 0x64, 0x00 }, // ClientGuild
            new byte[] { 0x00, 0x43, 0x6C, 0x69, 0x65, 0x6E, 0x74, 0x4C, 0x46, 0x47, 0x00 }, // ClientLFG
            new byte[] { 0x00, 0x43, 0x6C, 0x69, 0x65, 0x6E, 0x74, 0x4D, 0x6F, 0x76, 0x65, 0x6D, 0x65, 0x6E, 0x74, 0x00 }, // ClientMovement
            new byte[] { 0x00, 0x43, 0x6C, 0x69, 0x65, 0x6E, 0x74, 0x51, 0x75, 0x65, 0x73, 0x74, 0x00 }, // ClientQuest
            new byte[] { 0x00, 0x43, 0x6C, 0x69, 0x65, 0x6E, 0x74, 0x53, 0x6F, 0x63, 0x69, 0x61, 0x6C, 0x00 }, // ClientSocial
            new byte[] { 0x00, 0x43, 0x6C, 0x69, 0x65, 0x6E, 0x74, 0x53, 0x70, 0x65, 0x6C, 0x6C, 0x00 } // ClientSpell
        };

        public static void InitializeDictionary(FileStream wow)
        {
            Dispatchers[JamGroup.None] = null;
            Dispatchers[JamGroup.Client] = new JamGroups.Client(wow);
            Dispatchers[JamGroup.ClientGuild] = new JamGroups.ClientGuild(wow);
            Dispatchers[JamGroup.ClientGarrison] = new JamGroups.ClientGarrison(wow);
            Dispatchers[JamGroup.ClientLFG] = new JamGroups.ClientLFG(wow);
            Dispatchers[JamGroup.ClientChat] = new JamGroups.ClientChat(wow);
            Dispatchers[JamGroup.ClientMovement] = new JamGroups.ClientMovement(wow);
            Dispatchers[JamGroup.ClientSocial] = new JamGroups.ClientSocial(wow);
            Dispatchers[JamGroup.ClientSpell] = new JamGroups.ClientSpell(wow);
            Dispatchers[JamGroup.ClientQuest] = new JamGroups.ClientQuest(wow);
        }

        static void Main(string[] args)
        {
            if (!Config.Load(args))
                return;

            Logger.CreateOutputStream(Config.OutputFile);

            // Open the file
            FileStream wow = File.OpenRead(Config.Executable);
            if (!wow.CanRead)
                return;

            environment = Emulator.Create(wow);

            InitializeDictionary(wow);
            if (Config.DumpCmsg || Config.DumpSmsg)
            {
                Logger.WriteConsoleLine("Loading opcodes from GitHub, build 19103...");
                if (!Opcodes.TryPopulate())
                    return;
            }

            if (Config.DumpSmsg)
            {
                Logger.WriteLine("Dumping SMSG opcodes...");

                var jamGroupCount = new Dictionary<JamGroup, uint>();
                var opcodesCount = 0;
                var namesFoundCount = 0;

                Logger.WriteLine("+---------------+-------------+-------------+--------------------+---------+");
                Logger.WriteLine("|     Opcode    |  JAM Parser | Jam Handler |     Group Name     | ConnIdx |");
                Logger.WriteLine("+---------------+-------------+-------------+--------------------+---------+");
                if (Config.SpecificOpcodeValue != 0xBADD)
                {
                    if (DumpSmsgOpcode(Config.SpecificOpcodeValue, jamGroupCount))
                        ++opcodesCount;
                }
                else
                {
                    for (uint opcode = 1; opcode < 0x1FFF; ++opcode)
                    {
                        if (DumpSmsgOpcode(opcode, jamGroupCount))
                            ++opcodesCount;
                        if (Opcodes.GetOpcodeNameForServer(opcode) != string.Empty)
                            ++namesFoundCount;
                    }
                }
                Logger.WriteLine("+---------------+-------------+-------------+--------------------+---------+");
                Logger.WriteLine(@"Dumped {0} SMSG JAM opcodes.", opcodesCount);
                for (var i = 0; i < jamGroupCount.Count; ++i)
                    Logger.WriteLine("Dumped {0} SMSG {1} opcodes.", jamGroupCount.Values.ElementAt(i), jamGroupCount.Keys.ElementAt(i).ToString());

                // Integrity check
                if (namesFoundCount != Opcodes.SMSG.Count)
                    Logger.WriteLine("Found {0} WPP SMSG opcodes over {1}.", namesFoundCount, Opcodes.SMSG.Count);
                else
                    Logger.WriteLine("All SMSGs defined in WPP have been found.");
            }

            if (Config.DumpCmsg)
            {
                // Dump CMSGs
                Logger.WriteLine("Dumping CMSG opcodes...");
                using (var c = new Cmsg.CliOpcodes(wow, Config.SpecificOpcodeValue));
            }
        }

        protected static bool DumpSmsgOpcode(uint opcode, Dictionary<JamGroup, uint> jamGroupCount)
        {
            //! NOTE: All the ESP modifications are caused by the fact that calling
            //! does not push the return address on the stack.
            foreach (var dispatcherPair in Dispatchers)
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

                environment.Reset();
                environment.Push(opcode);
                environment.Esp.Value -= 4;
                environment.Execute(checkerFn, dispatcher.Disasm, false);
                if (environment.Eax.Value == 0)
                    continue;

                var connIndex = 0u;
                if (connectionFn != 0)
                {
                    environment.Reset();
                    environment.Push();
                    environment.Push();
                    environment.Push();
                    environment.Push(opcode);
                    environment.Push();
                    environment.Esp.Value -= 4;
                    environment.Execute(connectionFn, dispatcher.Disasm, false);
                    if (environment.Eax.Al == 0)
                    {
                        var requiresInstanceConnectionFn = environment.GetCalledOffsets()[0] - 0x400C00;

                        environment.Reset();
                        environment.Push(opcode);
                        environment.Esp.Value -= 4;
                        environment.Execute(requiresInstanceConnectionFn, dispatcher.Disasm, false);
                        connIndex = environment.Eax.Value;
                    }
                }

                environment.Reset();
                environment.Execute(dispatcherFn, dispatcher.Disasm, false);
                var calleeOffset = environment.GetCalledOffsets()[0] - 0x400C00;

                environment.Reset();
                environment.Push();
                environment.Push((ushort)0);
                environment.Push((ushort)opcode);
                environment.Push();
                environment.Push();
                environment.Esp.Value -= 4;
                environment.Execute(calleeOffset, dispatcher.Disasm, false);
                var jamData = environment.GetCalledOffsets();
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
