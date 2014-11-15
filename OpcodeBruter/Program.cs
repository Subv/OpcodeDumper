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
        public static bool Debug = false;
        
        public static Dictionary<JamGroup, JamDispatch> Dispatchers = new Dictionary<JamGroup, JamDispatch>();

        public static Emulator environment;
        
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
            if (args.Length == 0 || args.Contains("-help"))
            {
                ShowHelp();
                return;
            }

            // Open the file
            var filePath = AppDomain.CurrentDomain.BaseDirectory + "./Wow.exe";
            if (args.Contains("-e"))
                filePath = args[Array.IndexOf(args, "-e") + 1];
            
            FileStream wow = File.OpenRead(filePath);
            if (!wow.CanRead)
                return;
            
            environment = Emulator.Create(wow);

            InitializeDictionary(wow);
            if (!args.Contains("-s") || !args.Contains("-c"))
            {
                Console.WriteLine("Loading opcodes from GitHub, build 19103...");
                if (!Opcodes.TryPopulate())
                    return;
            }
            
            var specificOpcodeValue = args.Contains("-o") ? Convert.ToUInt32(args[Array.IndexOf(args, "-o") + 1]) : 0x0000;

            if (!args.Contains("-s"))
            {
                var clock = new Stopwatch();
                Console.WriteLine("Dumping SMSG opcodes...");
                Console.WriteLine("+---------------+-------------+-------------+--------------------+---------+");
                Console.WriteLine("|     Opcode    |  JAM Parser | Jam Handler |     Group Name     | ConnIdx |");
                Console.WriteLine("+---------------+-------------+-------------+--------------------+---------+");
                
                var jamGroupCount = new Dictionary<JamGroup, uint>();
                var opcodesCount = 0;
                var namesFoundCount = 0;

                clock.Start();
                if (specificOpcodeValue != 0x0000)
                {
                    if (DumpSmsgOpcode(specificOpcodeValue, jamGroupCount))
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
                clock.Stop();
                Console.WriteLine("+---------------+-------------+-------------+--------------------+---------+");
                Console.WriteLine(@"Dumped {0} SMSG JAM opcodes in {1} ms.", opcodesCount, clock.ElapsedMilliseconds);
                for (var i = 0; i < jamGroupCount.Count; ++i)
                    Console.WriteLine("Dumped {0} SMSG {1} opcodes.", jamGroupCount.Values.ElementAt(i), jamGroupCount.Keys.ElementAt(i).ToString());
                
                // Integrity check
                if (namesFoundCount != Opcodes.SMSG.Count)
                    Console.WriteLine("Found {0} WPP SMSG opcodes over {1}.", namesFoundCount, Opcodes.SMSG.Count);
                else
                    Console.WriteLine("All SMSGs defined in WPP have been found.");
            }
            
            if (!args.Contains("-c"))
            {
                // Dump CMSGs
                Console.WriteLine("Dumping CMSG opcodes...");
                new Cmsg.CliOpcodes(wow, specificOpcodeValue);
            }
            
        }

        protected static bool DumpSmsgOpcode(uint opcode, Dictionary<JamGroup, uint> jamGroupCount)
        {
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
                /*if (connectionFn != 0)
                {
                    environment.Reset();
                    environment.Push(opcode);
                    environment.Esp.Value -= 4;
                    environment.Execute(connectionFn, dispatcher.Disasm, false);
                    var requiresInstanceConnectionFn = environment.GetCalledOffsets()[0];
                    
                    // Breaks here - segment registers are not implemented.
                    environment.Reset();
                    environment.Push(opcode);
                    environment.Esp.Value -= 4;
                    environment.Execute(requiresInstanceConnectionFn, dispatcher.Disasm, false);
                    connIndex = environment.Eax.Value;
                }*/
                
                // The stack is awfully patched up together and breaks here.
                // Call the dispatcher, but do not follow E8. Capture the call,
                // Re-fix the stack, and work from there.
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
                
                Console.WriteLine("| {4} (0x{0:X4}) |  0x{1:X8} |  0x{2:X8} | {3} | {6} | {5}",
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
        
        protected static void ShowHelp()
        {
            Console.WriteLine("Arguments:");
            Console.WriteLine("-e [Value]     : Indicates the path to Wow.exe. Mandatory");
            Console.WriteLine("-o [Value]     : Tries to compute data for a specific opcode value.");
            Console.WriteLine("                 [Value] should be a decimal number");
            Console.WriteLine("-c             : Do not dump CMSG opcodes.");
            Console.WriteLine("-s             : Do not dump SMSG opcodes.");
            Console.WriteLine("-help          : This help message.");
        }
    }
}
