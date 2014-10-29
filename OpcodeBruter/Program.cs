using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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
            if (JamGroups.Client.Check(opcode))
                return JamGroup.Client;
            else if (JamGroups.ClientGuild.Check(opcode))
                return JamGroup.ClientGuild;
            else if (JamGroups.ClientGarrison.Check(opcode))
                return JamGroup.ClientGarrison;
            else if (JamGroups.ClientLFG.Check(opcode))
                return JamGroup.ClientLFG;
            else if (JamGroups.ClientChat.Check(opcode))
                return JamGroup.ClientChat;
            else if (JamGroups.ClientMovement.Check(opcode))
                return JamGroup.ClientMovement;
            else if (JamGroups.ClientSocial.Check(opcode))
                return JamGroup.ClientSocial;
            else if (JamGroups.ClientSpell.Check(opcode))
                return JamGroup.ClientSpell;
            else if (JamGroups.ClientQuest.Check(opcode))
                return JamGroup.ClientQuest;
            return JamGroup.None;
        }

        public static JamDispatch GetJAMDispatcher(JamGroup group)
        {
            return Dispatchers[group];
        }

        static void Main(string[] args)
        {
            // Open the file
            var filePath = AppDomain.CurrentDomain.BaseDirectory + "./Wow.exe";
            if (args.Length == 1)
                filePath = args[0];
            FileStream wow = File.OpenRead(filePath);
            if (!wow.CanRead)
                return;

            InitializeDictionary(wow);

            Console.WriteLine("+---------------+-------------+-------------+-------------+--------------------+");
            Console.WriteLine("|     Opcode    | Case offset |  JAM Parser | Jam Handler |     Group Name     |");
            Console.WriteLine("+---------------+-------------+-------------+-------------+--------------------+");
            
            var opcodesCount = 0;
            for (uint opcode = 0; opcode < 0x1FFF; ++opcode)
            {
                var jamGroup = GetJAMGroup(opcode);
                JamDispatch dispatcher = GetJAMDispatcher(jamGroup);

                if (dispatcher == null)
                    continue;

                int offset = dispatcher.CalculateOffset(opcode);
                if (offset < 0)
                    continue;

                uint address = dispatcher.CalculateHandler(opcode);

                var jamData = new uint[2];
                switch (jamGroup)
                {
                    case JamGroup.Client:
                    case JamGroup.ClientGuild:
                        jamData = dispatcher.CalculateJamFunctionOffsets(address);
                        break;
                    case JamGroup.ClientSpell:
                        jamData = dispatcher.CalculateJamFunctionOffsets(opcode);
                        break;
                }
                // This can only happen if either disassembly failed,
                // or no CALL opcodes were found.                
                // if (jamData[0] == 0 || jamData[1] == 0)
                //     continue;
                
                ++opcodesCount;
                
                Console.WriteLine("| {5} (0x{0:X4}) |  0x{1:X8} |  0x{2:X8} |  0x{3:X8} | {4} |",
                                  opcode,
                                  address,
                                  jamData[0], jamData[1],
                                  jamGroup.ToString().PadLeft(18),
                                  opcode.ToString().PadLeft(4));
            }
            Console.WriteLine("+---------------+-------------+-------------+-------------+--------------------+");
            Console.WriteLine(@"Dumper {0} JAM opcodes.", opcodesCount);
        }
    }
}
