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
            FileStream wow = File.OpenRead(args[0]);
            if (!wow.CanRead)
                return;

            InitializeDictionary(wow);

            for (uint i = 0; i < 0x1FFF; ++i)
            {
                JamGroup group = GetJAMGroup(i);
                JamDispatch dispatcher = GetJAMDispatcher(group);

                if (dispatcher == null)
                    continue;

                int offset = dispatcher.CalculateOffset(i);
                
                if (offset < 0)
                    continue;

                uint address = dispatcher.CalculateHandler(i);
                Console.WriteLine("Opcode {0} (0x{0:X}) from group {3} with switch value {1} has its handler at {2} (Hex: 0x{2:X})", i, offset, address, group.ToString());
            }
        }
    }
}
