using System;
using System.Linq;
using System.Reflection;

namespace OpcodeBruter
{
    /// <summary>
    /// Parses command line arguments.
    /// </summary>
    public static class Config
    {
        [ConfigKey("-op", "Defines a specific opcode value to use.")]
        public static uint SpecificOpcodeValue = 0xBADD;

        [ConfigKey("-debug", "Controls for extended debug info to be written to the console.")]
        public static bool Debug = false;

        [ConfigKey("-no-gh-names", "Controls if names from WPP are to be downloaded.")]
        public static bool GhNames = false;
        
        [ConfigKey("-no-client", "Controls if CMSG opcodes are to be dumped.")]
        public static bool DumpCmsg = true;

        [ConfigKey("-no-server", "Controls if SMSG opcodes are to be dumped.")]
        public static bool DumpSmsg = true;

        [ConfigKey("-e", "Path to the wow executable.")]
        public static string Executable = AppDomain.CurrentDomain.BaseDirectory + "./Wow.exe";

        [ConfigKey("-of", "Path to the file used for output.")]
        public static string OutputFile = String.Empty;

        public static bool Load(string[] args)
        {
            var showHelp = !TryGet<uint>(args, "-op", ref SpecificOpcodeValue, 0xBADD);
            showHelp = !TryGet<bool>(args, "-debug", ref Debug, false);
            showHelp = !TryGet<bool>(args, "-no-client", ref DumpCmsg, false);
            showHelp = !TryGet<bool>(args, "-no-server", ref DumpSmsg, true);
            showHelp = !TryGet<bool>(args, "-no-gh-names", ref GhNames, false);
            showHelp = !TryGet<string>(args, "-of", ref OutputFile, String.Empty);
            showHelp = !TryGet<string>(args, "-e", ref Executable, AppDomain.CurrentDomain.BaseDirectory + "./Wow.exe");
            return showHelp ? ShowHelp() : true;
        }

        private static bool ShowHelp()
        {
            Console.WriteLine("Arguments:");
            foreach (var f in typeof(Config).GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                var attr = (ConfigKey)f.GetCustomAttribute(typeof(ConfigKey), false);
                Logger.WriteConsoleLine(" {0} : {1}", attr.Key.PadRight(11), attr.Description);
            }
            return false;
        }

        public static bool TryGet<T>(string[] args, string argName, ref T dest, T defaultValue)
        {
            var keyType = Type.GetTypeCode(typeof(T));

            var index = Array.IndexOf(args, argName);
            if (keyType == TypeCode.Boolean || index == -1)
            {
                if (index == -1)
                    dest = defaultValue;
                else if (keyType == TypeCode.Boolean)
                    using (var box = new ValueBox<T>())
                    {
                        box.ObjectValue = index != -1;
                        dest = box.GetValue();
                    }
                return false;
            }
            else
            {
                var val = args[index + 1];
                using (var box = new ValueBox<T>())
                {
                    switch (keyType)
                    {
                        case TypeCode.UInt32:
                            box.ObjectValue = Convert.ToUInt32(val);
                            break;
                        case TypeCode.Int32:
                            box.ObjectValue = Convert.ToInt32(val);
                            break;
                        case TypeCode.UInt16:
                            box.ObjectValue = Convert.ToUInt16(val);
                            break;
                        case TypeCode.Int16:
                            box.ObjectValue = Convert.ToInt16(val);
                            break;
                        case TypeCode.Byte:
                            box.ObjectValue = Convert.ToByte(val);
                            break;
                        case TypeCode.SByte:
                            box.ObjectValue = Convert.ToSByte(val);
                            break;
                        case TypeCode.Single:
                            box.ObjectValue = Convert.ToSingle(val);
                            break;
                        case TypeCode.String:
                            box.ObjectValue = Convert.ToString(val);
                            break;
                        default:
                            Logger.WriteConsoleLine("Unable to read value for argument {0}", argName);
                            break;
                    }
                    dest = box.GetValue();
                }
                return true;
            }
        }
    }

    public sealed class ValueBox<T> : IDisposable
    {
        public object ObjectValue;

        public T GetValue()
        {
            return (T)ObjectValue;
        }

        public void Dispose() { }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class ConfigKey : Attribute
    {
        public string Description;
        public string Key;

        public ConfigKey(string key, string attr, params object[] obj)
        {
            Key = key;
            Description = String.Format(attr, obj);
        }
    }
}
