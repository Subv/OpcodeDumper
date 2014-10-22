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
            Dispatchers[JamGroup.Guild] = new JamGroups.Guild(wow);
        }

        public static JamGroup GetJAMGroup(uint opcode)
        {
            if (((opcode - 1) & 0x88A) == 2 || ((opcode - 1) & 0x4A2) == 162)
                return JamGroup.Client;
            else if (((opcode - 1) & 0xA8E) == 520)
                return JamGroup.Guild;

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
