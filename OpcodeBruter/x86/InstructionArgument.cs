using System;
using System.Collections.Generic;
using System.Text;


namespace x86
{
    public enum ArgumentType
    {
        Immediate,
        Register,
        Memory
    }

    public class InstructionArgument
    {
        private Emulator env;

        // argument type information
        public readonly ArgumentType Type;
        public readonly RegisterSubType RegisterType;
        public readonly string RegisterName;
        public uint Size;

        // argument values
        public readonly Register Register;
        public readonly uint Address;
        public readonly uint Immediate;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg"></param>
        public InstructionArgument(Emulator env, string arg)
        {
            this.env = env;
            arg = arg.Trim();

            if (arg.Contains("ptr"))
            {
                // get argument size
                // todo: default to dword if no size is specified
                if (arg.StartsWith("byte ptr [")) Size = 1;
                else if (arg.StartsWith("word ptr [")) Size = 2;
                else if (arg.StartsWith("dword ptr [")) Size = 4;
                else if (arg.StartsWith("fword ptr [")) Size = 4; // NYI
                else throw new ArgumentException();

                int lbi = arg.IndexOf('[');
                int rbi = arg.IndexOf(']');
                if (lbi > -1 && rbi > -1 && rbi > lbi)
                {
                    // get string inside brackets
                    string ptr = arg.Substring(lbi + 1, rbi - lbi - 1);
                    
                    // replace registers with the values contained within
                    ptr = ptr.Replace("eax", env.Eax.Value.ToString());
                    ptr = ptr.Replace("ebx", env.Ebx.Value.ToString());
                    ptr = ptr.Replace("ecx", env.Ecx.Value.ToString());
                    ptr = ptr.Replace("edx", env.Edx.Value.ToString());
                    ptr = ptr.Replace("esi", env.Esi.Value.ToString());
                    ptr = ptr.Replace("edi", env.Edi.Value.ToString());
                    ptr = ptr.Replace("esp", env.Esp.Value.ToString());
                    ptr = ptr.Replace("ebp", env.Ebp.Value.ToString());
                    ptr = ptr.Replace("ax", env.Eax.Ax.ToString());
                    ptr = ptr.Replace("bx", env.Ebx.Bx.ToString());
                    ptr = ptr.Replace("cx", env.Ecx.Cx.ToString());
                    ptr = ptr.Replace("dx", env.Edx.Dx.ToString());
                    ptr = ptr.Replace("si", env.Esi.Si.ToString());
                    ptr = ptr.Replace("di", env.Edi.Di.ToString());
                    ptr = ptr.Replace("sp", env.Esp.Sp.ToString());
                    ptr = ptr.Replace("bp", env.Ebp.Bp.ToString());
                    ptr = ptr.Replace("al", env.Eax.Al.ToString());
                    ptr = ptr.Replace("ah", env.Eax.Ah.ToString());
                    ptr = ptr.Replace("bl", env.Ebx.Bl.ToString());
                    ptr = ptr.Replace("bh", env.Ebx.Bh.ToString());
                    ptr = ptr.Replace("cl", env.Ecx.Cl.ToString());
                    ptr = ptr.Replace("ch", env.Ecx.Ch.ToString());
                    ptr = ptr.Replace("dl", env.Edx.Dl.ToString());
                    ptr = ptr.Replace("dh", env.Edx.Dh.ToString());

                    // evaluate the resulting expression
                    Address = Expression.Evaluate(ptr);
                }
                else throw new ArgumentException();

                Type = ArgumentType.Memory;
            }
            else if (arg == "eax")
            {
                Register = env.Eax;
                Type = ArgumentType.Register;
                RegisterType = x86.RegisterSubType.Full;
                RegisterName = "eax";
                Size = 4;
            }
            else if (arg == "ax")
            {
                Register = env.Eax;
                Type = ArgumentType.Register;
                RegisterType = x86.RegisterSubType.LowWord;
                RegisterName = "ax";
                Size = 2;
            }
            else if (arg == "ah")
            {
                Register = env.Eax;
                Type = ArgumentType.Register;
                RegisterType = x86.RegisterSubType.HighByte;
                RegisterName = "ah";
                Size = 1;
            }
            else if (arg == "al")
            {
                Register = env.Eax;
                Type = ArgumentType.Register;
                RegisterType = x86.RegisterSubType.LowByte;
                RegisterName = "al";
                Size = 1;
            }
            else if (arg == "ebx")
            {
                Register = env.Ebx;
                Type = ArgumentType.Register;
                RegisterType = x86.RegisterSubType.Full;
                RegisterName = "ebx";
                Size = 4;
            }
            else if (arg == "bx")
            {
                Register = env.Ebx;
                Type = ArgumentType.Register;
                RegisterType = x86.RegisterSubType.LowWord;
                RegisterName = "bx";
                Size = 2;
            }
            else if (arg == "bh")
            {
                Register = env.Ebx;
                Type = ArgumentType.Register;
                RegisterType = x86.RegisterSubType.HighByte;
                RegisterName = "bh";
                Size = 1;
            }
            else if (arg == "bl")
            {
                Register = env.Ebx;
                Type = ArgumentType.Register;
                RegisterType = x86.RegisterSubType.LowByte;
                RegisterName = "bl";
                Size = 1;
            }
            else if (arg == "ecx")
            {
                Register = env.Ecx;
                Type = ArgumentType.Register;
                RegisterType = x86.RegisterSubType.Full;
                RegisterName = "ecx";
                Size = 4;
            }
            else if (arg == "cx")
            {
                Register = env.Ecx;
                Type = ArgumentType.Register;
                RegisterType = x86.RegisterSubType.LowWord;
                RegisterName = "cx";
                Size = 2;
            }
            else if (arg == "ch")
            {
                Register = env.Ecx;
                Type = ArgumentType.Register;
                RegisterType = x86.RegisterSubType.HighByte;
                RegisterName = "ch";
                Size = 1;
            }
            else if (arg == "cl")
            {
                Register = env.Ecx;
                Type = ArgumentType.Register;
                RegisterType = x86.RegisterSubType.LowByte;
                RegisterName = "cl";
                Size = 1;
            }
            else if (arg == "edx")
            {
                Register = env.Edx;
                Type = ArgumentType.Register;
                RegisterType = x86.RegisterSubType.Full;
                RegisterName = "edx";
                Size = 4;
            }
            else if (arg == "dx")
            {
                Register = env.Edx;
                Type = ArgumentType.Register;
                RegisterType = x86.RegisterSubType.LowWord;
                RegisterName = "dx";
                Size = 2;
            }
            else if (arg == "dh")
            {
                Register = env.Edx;
                Type = ArgumentType.Register;
                RegisterType = x86.RegisterSubType.HighByte;
                RegisterName = "dh";
                Size = 1;
            }
            else if (arg == "dl")
            {
                Register = env.Edx;
                Type = ArgumentType.Register;
                RegisterType = x86.RegisterSubType.LowByte;
                RegisterName = "dl";
                Size = 1;
            }
            else if (arg == "esi")
            {
                Register = env.Esi;
                Type = ArgumentType.Register;
                RegisterType = x86.RegisterSubType.Full;
                RegisterName = "esi";
                Size = 4;
            }
            else if (arg == "si")
            {
                Register = env.Esi;
                Type = ArgumentType.Register;
                RegisterType = x86.RegisterSubType.LowWord;
                RegisterName = "si";
                Size = 2;
            }
            else if (arg == "edi")
            {
                Register = env.Edi;
                Type = ArgumentType.Register;
                RegisterType = x86.RegisterSubType.Full;
                RegisterName = "edi";
                Size = 4;
            }
            else if (arg == "di")
            {
                Register = env.Edi;
                Type = ArgumentType.Register;
                RegisterType = x86.RegisterSubType.LowWord;
                RegisterName = "di";
                Size = 2;
            }
            else if (arg == "esp")
            {
                Register = env.Esp;
                Type = ArgumentType.Register;
                RegisterType = x86.RegisterSubType.Full;
                RegisterName = "esp";
                Size = 4;
            }
            else if (arg == "sp")
            {
                Register = env.Esp;
                Type = ArgumentType.Register;
                RegisterType = x86.RegisterSubType.LowWord;
                RegisterName = "sp";
                Size = 2;
            }
            else if (arg == "ebp")
            {
                Register = env.Ebp;
                Type = ArgumentType.Register;
                RegisterType = x86.RegisterSubType.Full;
                RegisterName = "ebp";
                Size = 4;
            }
            else if (arg == "bp")
            {
                Register = env.Ebp;
                Type = ArgumentType.Register;
                RegisterType = x86.RegisterSubType.LowWord;
                RegisterName = "bp";
                Size = 2;
            }
            else
            {
                Type = ArgumentType.Immediate;

                // get immediate value
                if (arg.EndsWith("h"))
                {
                    Immediate = Convert.ToUInt32(arg.Substring(0, arg.Length - 1), 16);
                }
                else Immediate = Convert.ToUInt32(arg);

                // get immediate size
                if ((uint)Immediate < byte.MaxValue) Size = 1;
                else if ((uint)Immediate < ushort.MaxValue)  Size = 2;
                else Size = 4;
            }
        }

        /// <summary>
        /// Gets the unsigned integer value of this argument.
        /// </summary>
        /// <returns></returns>
        public uint GetValue()
        {
            switch (Type)
            {
                case ArgumentType.Register: return Register.GetValue(RegisterType);
                case ArgumentType.Memory: return env.ReadMemory(Address, Size);
                case ArgumentType.Immediate: return Immediate;
                default: throw new Exception();
            }
        }

        /// <summary>
        /// Sets the argument's value and automagically casts according to its data type.
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(uint value)
        {
            switch (Type)
            {
                case ArgumentType.Register:
                    Register.SetValue(RegisterType, value);
                    break;
                case ArgumentType.Memory:
                    switch (Size)
                    {
                        case 1: env.WriteMemory(Address, (byte)value); break;
                        case 2: env.WriteMemory(Address, (ushort)value); break;
                        case 4: env.WriteMemory(Address, value); break;
                        default: throw new ArgumentException("Invalid data type size.");
                    }
                    break;
                case ArgumentType.Immediate: throw new ArgumentException("Cannot modify the value of a readonly immediate type.");
                default: throw new ArgumentException("Invalid argument type.");
            }
        }
    }
}
