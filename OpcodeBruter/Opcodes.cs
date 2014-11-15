using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace OpcodeBruter
{
    /// <summary>
    /// This is the exact same file than WPP. Just *slightly* modified
    /// {Opcode\.(.+), 0x([A-Z0-9]+)},
    /// { "\1", 0x\2 },
    /// </summary>
    public static class Opcodes
    {
        public static Regex OpcodeRgx = new Regex(@"Opcode\.(.+), 0x([A-Z0-9]+)(?: | 0x[0-9A-Z]+)?", RegexOptions.IgnoreCase);

        private static string FilePath = @"https://raw.githubusercontent.com/TrinityCore/WowPacketParser/master/WowPacketParser/Enums/Version/V6_0_3_19103/Opcodes.cs";
        public static bool TryPopulate(bool smsg = true)
        {
            if (Program.Debug || (smsg ? SMSG : CMSG).Count != 0)
                return true;

            try
            {
                WebClient client = new WebClient();
                Stream stream = client.OpenRead(FilePath);
                StreamReader reader = new StreamReader(stream);
                var content = reader.ReadToEnd().Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                stream.Close();
                foreach (var line in content)
                {
                    var rgxResult = OpcodeRgx.Match(line);
                    if (!rgxResult.Success)
                        continue;

                    var opcodeName = rgxResult.Groups[1].Value;
                    var opcodeValue = Convert.ToInt32(rgxResult.Groups[2].Value, 16);
                    if (opcodeName.Contains("CMSG"))
                        CMSG.Add(opcodeName, opcodeValue);
                    else
                        SMSG.Add(opcodeName, opcodeValue);
                }
            }
            catch (WebException /*whatever*/) // Haha so funny I is.
            {
                Console.WriteLine("Unable to query opcodes. Try again.");
                return false;
            }

            return (smsg ? SMSG : CMSG).Count != 0;
        }

        public static string GetOpcodeNameForServer(uint opcode)
        {
            foreach (var pair in SMSG)
                if (pair.Value == opcode)
                    return pair.Key;
            return String.Empty;
        }

        public static string GetOpcodeNameForClient(uint opcode)
        {
            foreach (var pair in CMSG)
                if (pair.Value == opcode)
                    return pair.Key;
            return String.Empty;
        }

        public static Dictionary<string, int> CMSG = new Dictionary<string, int>();
        public static Dictionary<string, int> SMSG = new Dictionary<string, int>();
    }
}
