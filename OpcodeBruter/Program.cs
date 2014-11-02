using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace OpcodeBruter
{
    class Program
    {
        public static bool Debug = false;
        
        public static Dictionary<JamGroup, JamDispatch> Dispatchers = new Dictionary<JamGroup, JamDispatch>();

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

        public static JamGroup GetJAMGroup(uint opcode)
        {
            if (JamGroups.ClientQuest.Check(opcode))
                return JamGroup.ClientQuest;
            else if (JamGroups.Client.Check(opcode))
                return JamGroup.Client;
            else if (JamGroups.ClientGuild.Check(opcode))
                return JamGroup.ClientGuild;
            else if (JamGroups.ClientSocial.Check(opcode))
                return JamGroup.ClientSocial;
            else if (JamGroups.ClientSpell.Check(opcode))
                return JamGroup.ClientSpell;
            else if (JamGroups.ClientMovement.Check(opcode))
                return JamGroup.ClientMovement;
            else if (JamGroups.ClientLFG.Check(opcode))
                return JamGroup.ClientLFG;
            else if (JamGroups.ClientChat.Check(opcode))
                return JamGroup.ClientChat;
            else if (JamGroups.ClientGarrison.Check(opcode))
                return JamGroup.ClientGarrison;

            return JamGroup.None;
        }

        public static JamDispatch GetJAMDispatcher(JamGroup group)
        {
            return Dispatchers[group];
        }
        
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                ShowHelp();
                return;
            }
            
            #region Convert Hex-Rays code to C#
            if (args.Contains("-hr"))
            {
                Console.WriteLine(Utils.GenerateHandlerFromHexrays(File.ReadAllText("./hr.txt")));
                return;
            }
            #endregion

            // Open the file
            var filePath = AppDomain.CurrentDomain.BaseDirectory + "./Wow.exe";
            if (args.Contains("-e"))
                filePath = args[Array.IndexOf(args, "-e") + 1];
            FileStream wow = File.OpenRead(filePath);
            if (!wow.CanRead)
                return;

            InitializeDictionary(wow);
            if (!args.Contains("-s") || !args.Contains("-c"))
            {
                Console.WriteLine("Loading opcodes from GitHub, build 19033...");
                if (!Opcodes.TryPopulate())
                    return;
            }
            
            var specificOpcodeValue = args.Contains("-o") ? Convert.ToUInt32(args[Array.IndexOf(args, "-o") + 1]) : 0x0000;

            if (!args.Contains("-s"))
            {
                Console.WriteLine("Dumping SMSG opcodes...");
                Console.WriteLine("+---------------+-------------+-------------+-------------+--------------------+");
                Console.WriteLine("|     Opcode    | Case offset |  JAM Parser | Jam Handler |     Group Name     |");
                Console.WriteLine("+---------------+-------------+-------------+-------------+--------------------+");
                
                var jamGroupCount = new Dictionary<JamGroup, uint>();
                var opcodesCount = 0;

                if (specificOpcodeValue != 0x0000)
                    DumpSmsgOpcode(specificOpcodeValue, ref opcodesCount, ref jamGroupCount);
                else
                {
                    for (uint opcode = 0; opcode < 0x1FFF; ++opcode)
                        DumpSmsgOpcode(opcode, ref opcodesCount, ref jamGroupCount);
                }
                Console.WriteLine("+---------------+-------------+-------------+-------------+--------------------+");
                Console.WriteLine(@"Dumped {0} SMSG JAM opcodes.", opcodesCount);
                for (var i = 0; i < jamGroupCount.Count; ++i)
                    Console.WriteLine("Dumped {0} SMSG {1} opcodes.", jamGroupCount.Values.ElementAt(i), jamGroupCount.Keys.ElementAt(i).ToString());
            }
            
            if (!args.Contains("-c"))
            {
                // Dump CMSGs
                Console.WriteLine("Dumping CMSG opcodes...");
                var cliStorage = new Cmsg.CliOpcodes(wow, specificOpcodeValue);
            }
            
        }
        protected static void DumpSmsgOpcode(uint opcode, ref int opcodesCount, ref Dictionary<JamGroup, uint> jamGroupCount)
        {
            var jamGroup = GetJAMGroup(opcode);
            /*if (jamGroup != JamGroup.Client)
                    continue;*/
            
            JamDispatch dispatcher = GetJAMDispatcher(jamGroup);

            if (dispatcher == null)
                return;

            int offset = dispatcher.CalculateOffset(opcode);
            if (offset < 0)
                return;

            uint address = dispatcher.CalculateHandler(opcode);

            var jamData = new uint[2];
            switch (jamGroup)
            {
                case JamGroup.Client:
                case JamGroup.ClientGuild:
                    jamData = dispatcher.CalculateJamFunctionOffsets(address);
                    break;
                default:
                    jamData = dispatcher.CalculateJamFunctionOffsets(opcode);
                    break;
            }
            // This can only happen if either disassembly failed,
            // or no CALL opcodes were found.
            if (jamData[0] == 0 || jamData[1] == 0)
                return;
            
            ++opcodesCount;
            if (!jamGroupCount.ContainsKey(jamGroup))
                jamGroupCount.Add(jamGroup, 0);
            ++jamGroupCount[jamGroup];
            
            Console.WriteLine("| {5} (0x{0:X4}) |  0x{1:X8} |  0x{2:X8} |  0x{3:X8} | {4} | {6}",
                              opcode,
                              address,
                              jamData[0], jamData[1],
                              jamGroup.ToString().PadLeft(18),
                              opcode.ToString().PadLeft(4),
                              Opcodes.GetOpcodeNameForServer(opcode));
        }
        
        protected static void ShowHelp()
        {
            Console.WriteLine("Arguments:");
            Console.WriteLine("-e [Value]     : Indicates the path to Wow.exe. Mandatory");
            Console.WriteLine("-o [Value]     : Tries to compute data for a specific opcode value.");
            Console.WriteLine("                 [Value] should be a decimal number");
            Console.WriteLine("-c             : Do not dump CMSG.");
            Console.WriteLine("-s             : Do not dump SMSG.");
            Console.WriteLine("-help          : This help message.");
        }
    }
}
