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

        public static Emulator Environment { get; private set; }
        public static BinaryReader ClientStream { get; private set; }
        public static Stream BaseStream { get { return ClientStream.BaseStream; } }
        public static byte[] ClientBytes { get; private set; }
        public static UnmanagedBuffer Disasm { get; private set; }

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
            Dispatchers[JamGroup.Client] = new JamGroups.Client();
            Dispatchers[JamGroup.ClientGuild] = new JamGroups.ClientGuild();
            Dispatchers[JamGroup.ClientGarrison] = new JamGroups.ClientGarrison();
            Dispatchers[JamGroup.ClientLFG] = new JamGroups.ClientLFG();
            Dispatchers[JamGroup.ClientChat] = new JamGroups.ClientChat();
            Dispatchers[JamGroup.ClientMovement] = new JamGroups.ClientMovement();
            Dispatchers[JamGroup.ClientSocial] = new JamGroups.ClientSocial();
            Dispatchers[JamGroup.ClientSpell] = new JamGroups.ClientSpell();
            Dispatchers[JamGroup.ClientQuest] = new JamGroups.ClientQuest();
        }

        static void Main(string[] args)
        {   
            if (!Config.Load(args))
                return;

            if (Config.NoCmsg && Config.NoSmsg)
            {
                Logger.WriteConsoleLine("Please give me something to do.");
                Config.ShowHelp();
                return;
            }

            Logger.CreateOutputStream(Config.OutputFile);

            ClientStream = new BinaryReader(File.OpenRead(Config.Executable));
            if (!BaseStream.CanRead)
                return;

            ClientBytes = File.ReadAllBytes(Config.Executable);
            Disasm = new UnmanagedBuffer(ClientBytes);

            Environment = Emulator.Create(BaseStream);

            InitializeDictionary(BaseStream as FileStream);
            if (!Config.NoGhNames && !Opcodes.TryPopulate())
                return;

            if (!Config.NoSmsg)
                SMSG.Dump();

            if (!Config.NoCmsg)
                CMSG.Dump(Config.SpecificOpcodeValue);
        }
    }
}
