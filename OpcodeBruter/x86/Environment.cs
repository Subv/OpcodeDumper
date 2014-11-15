using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using Bea;
using OpcodeBruter;
using System.IO.Compression;

namespace x86
{
    /// <summary>
    /// Description of Environment.
    /// </summary>
    public class Emulator
    {
        public List<uint> CallOffsets = new List<uint>();
        public List<uint> JumpBacks = new List<uint>();
        
        #region Processor
        public Eax Eax = new Eax();
        public Ebx Ebx = new Ebx();
        public Ecx Ecx = new Ecx();
        public Edx Edx = new Edx();
        public Esi Esi = new Esi();
        public Edi Edi = new Edi();
        public Esp Esp = new Esp();
        public Ebp Ebp = new Ebp();
        public uint Eip = 1;
        public EFlags EFlags = EFlags.ZeroFlag;
        public uint Cs = 0; // Holds the Code segment in which your program runs.
        public uint Ds = 0; // Holds the Data segment that your program accesses.
        
        public Register GetRegisterByName(string reg)
        {
            switch (reg)
            {
                case "eax":
                case "ax": case "al": case "ah":
                    return Eax;
                case "ebx":
                case "bx": case "bl": case "bh":
                    return Ebx;
                case "ecx":
                case "cx": case "cl":case "ch":
                    return Ecx;
                case "edx":
                case "dx": case "dl": case "dh":
                    return Edx;
                case "esi": case "si":
                    return Esi;
                case "edi": case "di":
                    return Edi;
                case "esp": case "sp":
                    return Esp;
                case "ebp": case "bp":
                    return Ebp;
            }
            return null;
        }
        
        public void ToggleFlagIf(EFlags flags, bool condition) { if (condition) SetFlags(flags); else ClearFlags(flags); }
        public void SetFlags(EFlags flags)   { EFlags = EFlags.Set(flags); }
        public void ClearFlags(EFlags flags) { EFlags = EFlags.Clear(flags); }
        public void ClearFlags()             { EFlags = EFlags.Clear(); }
        
        public void UpdateFlags(ulong result, uint size, EFlags flags)
        {
            if (flags.HasFlag(EFlags.ZeroFlag))
                ToggleFlagIf(EFlags.ZeroFlag, result == 0);
            
            // http://graphics.stanford.edu/~seander/bithacks.html#ParityWith64Bits
            if (flags.HasFlag(EFlags.ParityFlag))
                ToggleFlagIf(EFlags.ParityFlag, ((((((((ulong)result & 0xFF) * (ulong)0x0101010101010101) & 0x8040201008040201) % 0x1FF) & 1) == 0)));

            if (flags.HasFlag(EFlags.SignFlag))
            {
                uint signFlag = (uint)(1 << (int)((size << 3) - 1));
                ToggleFlagIf(EFlags.SignFlag, (result & signFlag) == signFlag);
            }
            
            if (flags.HasFlag(EFlags.AdjustFlag))
                ToggleFlagIf(EFlags.AdjustFlag, (result & 8) == 8);
            
            if (flags.HasFlag(EFlags.OverflowFlag))
            {
                long signedResult = (long)result;
                if (size == 1)
                    ToggleFlagIf(EFlags.OverflowFlag, signedResult > sbyte.MaxValue || signedResult < sbyte.MinValue);
                else if (size == 2)
                    ToggleFlagIf(EFlags.OverflowFlag, signedResult > short.MaxValue || signedResult < short.MinValue);
                else if (size == 4)
                    ToggleFlagIf(EFlags.OverflowFlag, signedResult > int.MaxValue || signedResult < int.MinValue);
            }
            
            if (flags.HasFlag(EFlags.CarryFlag))
                ToggleFlagIf(EFlags.CarryFlag, ((result >> ((int)size << 3)) & 1) == 1);
        }
        #endregion Processor
        
        #region Memory
        public readonly uint BaseAddress; //< Memory base address.
        public byte[] Memory;
        
        public byte ReadByteFromMemory(uint address)
        {
            if (address < BaseAddress)
                return 0xCC;
            return Memory[address - BaseAddress];
        }
        
        public unsafe ushort ReadWordFromMemory(uint address)
        {
            uint offset = address - BaseAddress;
            if (offset < Memory.Length - 1)
            {
                fixed (byte* p = Memory)
                {
                    return *(ushort*)(p + offset);
                }
            }
            return 0xCCCC;
        }
        
        public unsafe uint ReadDwordFromMemory(uint address)
        {
            uint offset = address - BaseAddress;
            if (offset < Memory.Length - 3)
            {
                fixed (byte* p = Memory)
                {
                    return *(uint*)(p + offset);
                }
            }
            return 0xCCCCCCCC;
        }

        public void WriteByteToMemory(uint address, byte value)
        {
            uint offset = address - BaseAddress;
            if (offset < Memory.Length)
                Memory[offset] = value;
        }
        
        public unsafe void WriteWordToMemory(uint address, ushort value)
        {
            uint offset = address - BaseAddress;
            if (offset < Memory.Length - 1)
            {
                fixed (byte* p = Memory)
                {
                    (*(ushort*)(p + offset)) = value;
                }
            }
        }
        
        public unsafe void WriteDwordToMemory(uint address, uint value)
        {
            uint offset = address - BaseAddress;
            if (offset < Memory.Length - 3)
            {
                fixed (byte* p = Memory)
                {
                    (*(uint*)(p + offset)) = value;
                }
            }
        }
        
        public void WriteMemory(uint address, object value)
        {
            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                    WriteByteToMemory(address, (byte)value);
                    break;
                case TypeCode.Int16:
                case TypeCode.UInt16:
                    WriteWordToMemory(address, (ushort)value);
                    break;
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Single:
                    WriteDwordToMemory(address, (uint)value);
                    break;
            }
        }

        public uint ReadMemory(uint address, uint size)
        {
            switch (size)
            {
                case 1: return ReadByteFromMemory(address);
                case 2: return ReadWordFromMemory(address);
                case 4: return ReadDwordFromMemory(address);
            }
            return 0xCCCCCCCC;
        }
        #endregion
        
        #region Code execution
        private bool ExecuteJump(string disasmStr, Disasm disasm, string mnemonic)
        {
            bool shouldJump = false;

            if (mnemonic == "jmp")
                shouldJump = true;
            else if (mnemonic == "je" || mnemonic == "jz") // Jump short if equal (ZF=1).
                shouldJump = EFlags.HasFlag(EFlags.ZeroFlag);
            else if (mnemonic == "jne" || mnemonic == "jnz") // Jump short if not equal (ZF=0).
                shouldJump = !EFlags.HasFlag(EFlags.ZeroFlag);  
            else if (mnemonic == "ja" || mnemonic == "jnbe") // Jump short if not below or equal (CF=0 and ZF=0).
                shouldJump = !EFlags.HasFlag(EFlags.CarryFlag) && !EFlags.HasFlag(EFlags.ZeroFlag);  
            else if (mnemonic == "jb" || mnemonic == "jc" || mnemonic == "jnae")  // Jump short if not above or equal (CF=1). 
                shouldJump = EFlags.HasFlag(EFlags.CarryFlag);  
            else if (mnemonic == "jng" || mnemonic == "jle") // Jump short if not greater (ZF=1 or SF != OF).
                shouldJump = EFlags.HasFlag(EFlags.ZeroFlag) || (EFlags.HasFlag(EFlags.SignFlag) != EFlags.HasFlag(EFlags.OverflowFlag));  
            else if (mnemonic == "jnc" || mnemonic == "jnb" || mnemonic == "jae") // Jump short if not carry (CF=0). 
                shouldJump = !EFlags.HasFlag(EFlags.CarryFlag);  
            else if (mnemonic == "jna" || mnemonic == "jbe") // Jump short if not above (CF=1 or ZF=1).
                shouldJump = EFlags.HasFlag(EFlags.CarryFlag) || EFlags.HasFlag(EFlags.ZeroFlag);
            else if (mnemonic == "jge" || mnemonic == "jnl") // Jump short if greater or equal (SF=OF).
                shouldJump = EFlags.HasFlag(EFlags.SignFlag) == EFlags.HasFlag(EFlags.OverflowFlag);  
            else if (mnemonic == "jg" || mnemonic == "jnle") // Jump short if greater (ZF=0 and SF=OF).
                shouldJump = !EFlags.HasFlag(EFlags.ZeroFlag) && (EFlags.HasFlag(EFlags.SignFlag) == EFlags.HasFlag(EFlags.OverflowFlag)); 
            else if (mnemonic == "jl" || mnemonic == "jnge") // Jump short if less (SF != OF). 
                shouldJump = EFlags.HasFlag(EFlags.SignFlag) != EFlags.HasFlag(EFlags.OverflowFlag);
            else if (mnemonic == "jecxz") // Jump short if ECX register is 0. 
                shouldJump = Ecx.Value == 0;
            else if (mnemonic == "jcxz") // Jump short if CX register is 0. 
                shouldJump = Ecx.Cx == 0;
            else if (mnemonic == "jno") // Jump short if not overflow (OF=0)
                shouldJump = !EFlags.HasFlag(EFlags.OverflowFlag);
            else if (mnemonic == "jnp" || mnemonic == "jpo") // Jump short if not parity (PF=0)
                shouldJump = !EFlags.HasFlag(EFlags.ParityFlag);
            else if (mnemonic == "jns") // Jump short if not sign (SF=0)
                shouldJump = !EFlags.HasFlag(EFlags.SignFlag);
            else if (mnemonic == "jo") // Jump short if overflow (OF=1)
                shouldJump = !EFlags.HasFlag(EFlags.OverflowFlag);
            else if (mnemonic == "jp" || mnemonic == "jpe") // Jump short if parity (PF=1)
                shouldJump = !EFlags.HasFlag(EFlags.ParityFlag);
            else if (mnemonic == "js") // Jump short if sign (SF=1)
                shouldJump = !EFlags.HasFlag(EFlags.SignFlag);
            else
                throw new Exception("Invalid jump type detected.");
            
            if (!shouldJump)
                return false;

            // We should jump.
            return true;
        }
        
        private bool ExecuteASM(Disasm disasm, UnmanagedBuffer buffer, bool followCalls)
        {
            var entryPoint = buffer.Ptr.ToInt32();
            var disassemblyString = disasm.CompleteInstr;
            // Sanitize the instruction.
            if (disassemblyString.IndexOf(";") != -1)
                disassemblyString = disassemblyString.Remove(disassemblyString.IndexOf(";"));

            disassemblyString = Regex.Replace(disassemblyString, @"\s+", " ", RegexOptions.IgnoreCase).Trim();
            
            if (Regex.IsMatch(disassemblyString, @"[!@#$%^&()={}\\|/?<>.~`""'_]"))
                return false;
            
            if (disassemblyString.Length == 0 || disassemblyString.EndsWith(":"))
                return false;
            
            var tokens = disassemblyString.Split(' ');
            var mnemonic = tokens[0];
            
            //Console.WriteLine("0x{0:X8} {1}", disasm.EIP.ToInt64(), disasm.CompleteInstr);
            
            if (disassemblyString.Contains("call far"))
                return false;
            
            List<InstructionArgument> arguments = new List<InstructionArgument>();
            // TODO Support for floating point opcodes

            var parametersList = disassemblyString.Replace(mnemonic, String.Empty).Trim().Split(new char[]{ ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var parameter in parametersList)
                arguments.Add(new InstructionArgument(this, parameter.Trim()));
            if (mnemonic[0] == 'j')
            {
                if (ExecuteJump(disassemblyString, disasm, mnemonic))
                {
                    var jumpOffset = arguments[0].GetValue();
                    if (arguments[0].Type == ArgumentType.Memory)
                        jumpOffset += (uint)entryPoint - 0x400C00;
                    disasm.EIP = new IntPtr(jumpOffset);
                    return true;
                } else return false;
            }
            else
            {
                InstructionArgument src = null;
                InstructionArgument dst = null;
                InstructionArgument extra = null;
                
                if (parametersList.Length == 1)
                    dst = src = arguments[0];
                else if (parametersList.Length == 2)
                {
                    dst = arguments[0];
                    src = arguments[1];
                }
                else if (parametersList.Length == 3)
                    extra = arguments[2];
                
                switch (mnemonic)
                {
                    case "ret": case "retn":
                        // Actually use the stack here.
                        if (JumpBacks.Count > 0)
                        {
                            disasm.EIP = new IntPtr(JumpBacks[0]);
                            JumpBacks.RemoveAt(0);
                            return true;
                        }
                        return false;
                    case "leave":
                        break;
                    case "nop": break;
                    #region Data transfer
                    case "mov":
                        if (parametersList.Length != 2 || dst.Type == ArgumentType.Immediate)
                            break;
                        if (src.Type == ArgumentType.Immediate && src.Size > dst.Size)
                            break;
                        if (src.Type != ArgumentType.Immediate && src.Size != dst.Size)
                            break;
                        dst.SetValue(src.GetValue());
                        break;
                    case "movzx":
                    {
                        if (parametersList.Length != 2 || dst.Type != ArgumentType.Register ||
                            src.Type == ArgumentType.Immediate || src.Size >= dst.Size)
                            break;
                        //! TODO Unhack this - Handles indirect jumptables
                        //! TODO: Really need to fix memory - this superbly fails now
                        //if (dst.Register == null)
                        //{
                            /*if (src.Size == 4)
                                addr = buffer.ReadDword(src.Address - 0x400C00);
                            else if (src.Size == 2)
                                addr = (uint)buffer.ReadWord(src.Address - 0x400C00);
                            else if (src.Size == 1)
                                addr = (uint)buffer.ReadByte(src.Address - 0x400C00);*/
                        //}
                        dst.SetValue(src.GetValue());
                        break;
                    }
                    case "movsb":
                        WriteByteToMemory(Edi.Value, ReadByteFromMemory(Esi.Value));
                        Ecx.Value--;
                        if (EFlags.HasFlag(EFlags.DirectionFlag))
                        {
                            Edi.Value--;
                            Esi.Value--;
                        }
                        else
                        {
                            Edi.Value++;
                            Esi.Value++;
                        }
                        break;
                    case "movsw":
                        WriteWordToMemory(Edi.Value, ReadWordFromMemory(Esi.Value));
                        Ecx.Value--;
                        if (EFlags.HasFlag(EFlags.DirectionFlag))
                        {
                            Edi.Value -= 2;
                            Esi.Value -= 2;
                        }
                        else
                        {
                            Edi.Value += 2;
                            Esi.Value += 2;
                        }
                        break;
                    case "movsd":
                        WriteDwordToMemory(Edi.Value, ReadDwordFromMemory(Esi.Value));
                        Ecx.Value--;
                        if (EFlags.HasFlag(EFlags.DirectionFlag))
                        {
                            Edi.Value -= 4;
                            Esi.Value -= 4;
                        }
                        else
                        {
                            Edi.Value += 4;
                            Esi.Value += 4;
                        }
                        break;
                    case "stosb":
                        WriteByteToMemory(Edi.Value, Eax.Al);
                        Ecx.Value--;
                        if (EFlags.HasFlag(EFlags.DirectionFlag))
                        {
                            Edi.Value--;
                            Esi.Value--;
                        }
                        else
                        {
                            Edi.Value++;
                            Esi.Value++;
                        }
                        break;
                    case "stosw":
                        WriteWordToMemory(Edi.Value, Eax.Ax);
                        Ecx.Value--;
                        if (EFlags.HasFlag(EFlags.DirectionFlag))
                        {
                            Edi.Value -= 2;
                            Esi.Value -= 2;
                        }
                        else
                        {
                            Edi.Value += 2;
                            Esi.Value += 2;
                        }
                        break;
                    case "stosd":
                        WriteDwordToMemory(Edi.Value, Eax.Value);
                        Ecx.Value--;
                        if (EFlags.HasFlag(EFlags.DirectionFlag))
                        {
                            Edi.Value -= 4;
                            Esi.Value -= 4;
                        }
                        else
                        {
                            Edi.Value += 4;
                            Esi.Value += 4;
                        }
                        break;
                    case "xchg":
                        if (parametersList.Length != 2 || dst.Type != ArgumentType.Register
                            || src.Type == ArgumentType.Immediate || src.Size != dst.Size)
                            break;

                        uint tmp = src.GetValue();
                        src.SetValue(dst.GetValue());
                        dst.SetValue(tmp);
                        break;
                    case "lea":
                        if (parametersList.Length == 2 && dst.Type == ArgumentType.Register && src.Type == ArgumentType.Memory && src.Size > 1)
                            dst.SetValue(src.Address);
                        break;
                    #endregion
                    #region Basic maths
                    case "inc":
                        if (parametersList.Length == 1 && dst.Type != ArgumentType.Immediate)
                        {
                            ulong val = (ulong)dst.GetValue() + 1;
                            UpdateFlags(val, dst.Size, EFlags.OverflowFlag | EFlags.SignFlag | EFlags.ZeroFlag | EFlags.AdjustFlag | EFlags.ParityFlag);
                            dst.SetValue((uint)val);
                        }
                        break;
                    case "dec":
                        if (parametersList.Length == 1 && dst.Type != ArgumentType.Immediate)
                        {
                            ulong val = (ulong)dst.GetValue() - 1;
                            UpdateFlags(val, dst.Size, EFlags.OverflowFlag | EFlags.SignFlag | EFlags.ZeroFlag | EFlags.AdjustFlag | EFlags.ParityFlag);
                            dst.SetValue((uint)val);
                        }
                        break;
                    case "not":
                        if (parametersList.Length == 1 && dst.Type != ArgumentType.Immediate)
                            dst.SetValue(~dst.GetValue());
                        break;
                    case "neg":
                        if (parametersList.Length == 1 && dst.Type != ArgumentType.Immediate)
                        {
                            ulong val = (ulong)(-(long)dst.GetValue());
                            UpdateFlags(val, dst.Size, EFlags.OverflowFlag | EFlags.SignFlag | EFlags.ZeroFlag | EFlags.AdjustFlag | EFlags.ParityFlag | EFlags.CarryFlag);
                            dst.SetValue((uint)val);
                        }
                        break;
                    case "bswap":
                        if (parametersList.Length != 1 || dst.Type != ArgumentType.Register || dst.Size != 4)
                            break;
                        uint dval = dst.GetValue();
                        dst.SetValue((uint)((dval << 24) | ((dval & 0xFF00) << 8) | ((dval & 0xFF0000) >> 8) | (dval >> 24)));
                        break;
                    case "add":
                        if (parametersList.Length == 2)
                        {
                            ulong val = (ulong)dst.GetValue() + src.GetValue();
                            UpdateFlags(val, dst.Size, EFlags.OverflowFlag | EFlags.SignFlag | EFlags.ZeroFlag | EFlags.AdjustFlag | EFlags.ParityFlag | EFlags.CarryFlag);
                            dst.SetValue((uint)val);
                        }
                        break;
                    case "sub":
                        if (parametersList.Length == 2)
                        {
                            ulong val = (ulong)dst.GetValue() - (ulong)src.GetValue();
                            UpdateFlags(val, dst.Size, EFlags.OverflowFlag | EFlags.SignFlag | EFlags.ZeroFlag | EFlags.AdjustFlag | EFlags.ParityFlag | EFlags.CarryFlag);
                            dst.SetValue((uint)val);
                        }
                        break;
                    case "mul": // unsigned multiply
                        // The OF and CF flags are set to 0 if the upper half of the result is 0, otherwise they are set to 1
                        if (parametersList.Length != 1 || src.Type == ArgumentType.Immediate)
                            break;
                        switch (src.Size)
                        {
                            case 1:
                                ushort r16 = (ushort)(Eax.Al * src.GetValue());
                                Eax.Ax = r16;
                                ToggleFlagIf(EFlags.OverflowFlag | EFlags.CarryFlag, (r16 >> 8) == 0);
                                break;
                            case 2:
                                uint r32 = Eax.Ax * src.GetValue();
                                Edx.Dx = (ushort)(r32 >> 16);    // high order
                                Eax.Ax = (ushort)(r32 & 0xFFFF); // low order
                                ToggleFlagIf(EFlags.OverflowFlag | EFlags.CarryFlag, Edx.Dx == 0);
                                break;
                            case 4:
                                ulong r64 = Eax.Ax * src.GetValue();
                                Edx.Value = (uint)(r64 >> 32);    // high order
                                Eax.Value = (uint)(r64 & 0xFFFFFFFF); // low order
                                ToggleFlagIf(EFlags.OverflowFlag | EFlags.CarryFlag, Edx.Value == 0);
                                break;
                        }
                        break;
                    case "div":
                        if (src.Type == ArgumentType.Immediate)
                            break;
                        ulong dividend, quotient;
                        uint divisor = src.GetValue();
                        if (divisor == 0)
                            break;

                        if (src.Size == 1)
                        {
                            dividend = Eax.Ax;
                            quotient = dividend / divisor;
                            if (quotient > 0xFF)
                                break;
                            Eax.Al = (byte)quotient;
                            Eax.Ah = (byte)(dividend % divisor);
                        }
                        else if (src.Size == 2)
                        {
                            dividend = ((uint)Edx.Dx << 16) & Eax.Ax;
                            quotient = dividend / divisor;
                            if (quotient > 0xFFFF)
                                break;
                            Eax.Ax = (ushort)quotient;
                            Edx.Dx = (ushort)(dividend % divisor);
                        }
                        else if (src.Size == 4)
                        {
                            dividend = ((ulong)Edx.Value << 32) & Eax.Value;
                            quotient = dividend / divisor;
                            if (quotient > 0xFFFFFFFF)
                                break;
                            Eax.Value = (uint)quotient;
                            Edx.Value = (uint)(dividend % divisor);
                        }
                        break;
                    case "imul": // Signed multiply
                    case "idiv": // Signed division
                        break; // NYI
                    #endregion
                    #region Bitwise operators
                    case "and":
                        if (parametersList.Length == 2)
                        {
                            uint result = dst.GetValue() & src.GetValue();
                            dst.SetValue(result);
                            ClearFlags(EFlags.OverflowFlag | EFlags.CarryFlag);
                            UpdateFlags(result, dst.Size, EFlags.SignFlag | EFlags.ZeroFlag | EFlags.ParityFlag);
                        }
                        break;
                    case "or":
                        if (parametersList.Length == 2)
                        {
                            uint result = dst.GetValue() | src.GetValue();
                            dst.SetValue(result);
                            ClearFlags(EFlags.OverflowFlag | EFlags.CarryFlag);
                            UpdateFlags(result, dst.Size, EFlags.SignFlag | EFlags.ZeroFlag | EFlags.ParityFlag);
                        }
                        else throw new ArgumentException();

                        break;
                    case "xor":
                        if (parametersList.Length == 2)
                        {
                            uint result = dst.GetValue() ^ src.GetValue();
                            dst.SetValue(result);
                            ClearFlags(EFlags.OverflowFlag | EFlags.CarryFlag);
                            UpdateFlags(result, dst.Size, EFlags.SignFlag | EFlags.ZeroFlag | EFlags.ParityFlag);
                        }
                        break;
                    case "stc": // set carry flag
                        SetFlags(EFlags.CarryFlag);
                        break;
                    case "std": // set direction flag
                        SetFlags(EFlags.DirectionFlag);
                        break;
                    case "clc": // clear carry flag
                        ClearFlags(EFlags.CarryFlag);
                        break;
                    case "cld": // clear direction flag
                        ClearFlags(EFlags.DirectionFlag);
                        break;
                    case "shr": // shift right
                    {
                        if (parametersList.Length != 2 || dst.Type == ArgumentType.Immediate
                            || src.Type == ArgumentType.Memory || (src.Type == ArgumentType.Register && src.RegisterName != "cl"))
                            break;
                        uint result = dst.GetValue() >> (byte)src.GetValue();
                        dst.SetValue(result);

                        if (src.Type == ArgumentType.Immediate && src.GetValue() == 1)
                            UpdateFlags(result, dst.Size, EFlags.OverflowFlag | EFlags.SignFlag | EFlags.ZeroFlag  | EFlags.ParityFlag | EFlags.CarryFlag);
                        else
                            UpdateFlags(result, dst.Size, EFlags.SignFlag | EFlags.ZeroFlag | EFlags.ParityFlag | EFlags.CarryFlag);
                        break;
                    }
                    case "shl": // shift left
                    case "sal": // shift arithmetic left (signed)
                    {
                        if (parametersList.Length != 2 || dst.Type == ArgumentType.Immediate || src.Type == ArgumentType.Memory || (src.Type == ArgumentType.Register && src.RegisterName != "cl"))
                            break;

                        ulong result = (ulong)dst.GetValue() << (byte)src.GetValue();
                        dst.SetValue((uint)result);

                        if (src.Type == ArgumentType.Immediate && src.GetValue() == 1)
                            UpdateFlags(result, dst.Size, EFlags.OverflowFlag | EFlags.SignFlag | EFlags.ZeroFlag | EFlags.ParityFlag | EFlags.CarryFlag);
                        else
                            UpdateFlags(result, dst.Size, EFlags.SignFlag | EFlags.ZeroFlag | EFlags.ParityFlag | EFlags.CarryFlag);
                        break;
                    }
                    case "sar": // shift arithmetic right (signed)
                    {
                        if (parametersList.Length != 2 || dst.Type == ArgumentType.Immediate || src.Type == ArgumentType.Memory || (src.Type == ArgumentType.Register && src.RegisterName != "cl"))
                            break;
                        uint result = (uint)((int)dst.GetValue() >> (byte)src.GetValue());
                        dst.SetValue(result);

                        if (src.Type == ArgumentType.Immediate && src.GetValue() == 1)
                            UpdateFlags(result, dst.Size, EFlags.OverflowFlag | EFlags.SignFlag | EFlags.ZeroFlag | EFlags.ParityFlag | EFlags.CarryFlag);
                        else
                            UpdateFlags(result, dst.Size, EFlags.SignFlag | EFlags.ZeroFlag | EFlags.ParityFlag | EFlags.CarryFlag);
                        break;
                    }
                    case "ror": // rotate right
                    {
                        // bits >> n | (bits << (32 - n));
                        uint data = dst.GetValue();
                        uint bitsToShift = src.GetValue();
                        data = data >> (int)bitsToShift | (data << (32 - (int)bitsToShift));
                        dst.SetValue(data);

                        if (src.Type == ArgumentType.Immediate && src.GetValue() == 1)
                            UpdateFlags(data, dst.Size, EFlags.OverflowFlag | EFlags.CarryFlag);
                        else
                            UpdateFlags(data, dst.Size, EFlags.CarryFlag);
                        break;
                    }
                    case "rol": // rotate left
                    {
                        // bits << n | (bits >> (32 - n));
                        uint data2 = dst.GetValue();
                        uint bitsToShift2 = src.GetValue();
                        uint data = data2 << (int)bitsToShift2 | (data2 >> (32 - (int)bitsToShift2));
                        dst.SetValue(data);

                        if (src.Type == ArgumentType.Immediate && src.GetValue() == 1)
                            UpdateFlags(data, dst.Size, EFlags.OverflowFlag | EFlags.CarryFlag);
                        else
                            UpdateFlags(data, dst.Size, EFlags.CarryFlag);
                        break;
                    }
                    #endregion
                    case "call":
                        // TODO Use the stack to keep s and r (saved registers (0) and jumpback offset (+4))
                        var offset = (int)disasm.Instruction.AddrValue - buffer.Ptr.ToInt32();
                        CallOffsets.Add((uint)offset);
                        if (followCalls)
                        {
                            JumpBacks.Add((uint)(disasm.EIP.ToInt32() + 5));
                            disasm.EIP = new IntPtr((int)disasm.Instruction.AddrValue);
                            return true;
                        }
                        return false;
                    case "cmp":
                        UpdateFlags((ulong)dst.GetValue() - (ulong)src.GetValue(), dst.Size, EFlags.OverflowFlag | EFlags.SignFlag | EFlags.ZeroFlag | EFlags.AdjustFlag | EFlags.ParityFlag | EFlags.CarryFlag);
                        break;
                    case "test":
                        ClearFlags(EFlags.OverflowFlag | EFlags.CarryFlag);
                        UpdateFlags(dst.GetValue() & src.GetValue(), dst.Size, EFlags.SignFlag | EFlags.ZeroFlag | EFlags.ParityFlag);
                        break;
                    case "sbb":
                        if (parametersList.Length != 2)
                            break;
                        ulong newValue = (ulong)dst.GetValue() - (ulong)src.GetValue();
                        if (EFlags.HasFlag(EFlags.CarryFlag))
                            newValue -= 1;
                        dst.SetValue((uint)newValue);
                        UpdateFlags(newValue, dst.Size, EFlags.OverflowFlag | EFlags.SignFlag | EFlags.ZeroFlag | EFlags.AdjustFlag | EFlags.CarryFlag);
                        break;
                    #region Stack operations
                    case "push":
                        // todo: check for stack overflow (stack pointer < base address)
                        if (parametersList.Length == 1)
                        {
                            uint s = dst.Size;
                            if (s == 2 || s == 4)
                            {
                                WriteMemory(Esp - s, dst.GetValue());
                                Esp.Value -= s;
                            }
                        }
                        break;
                    case "pop":
                        // todo: check for stack overflow (stack pointer > base address + memory size)
                        if (parametersList.Length == 1 && dst.Type != ArgumentType.Immediate)
                        {
                            uint s = dst.Size;
                            if (s == 2 || s == 4)
                            {
                                dst.SetValue(ReadMemory(Esp, s));
                                Esp.Value += s;
                            }
                        }
                        break;
                    case "pusha":
                        if (parametersList.Length == 0)
                        {
                            WriteMemory(Esp - 2, Eax.Ax);
                            WriteMemory(Esp - 4, Ecx.Cx);
                            WriteMemory(Esp - 6, Edx.Dx);
                            WriteMemory(Esp - 8, Ebx.Bx);
                            WriteMemory(Esp - 10, Esp.Sp);
                            WriteMemory(Esp - 12, Ebp.Bp);
                            WriteMemory(Esp - 14, Esi.Si);
                            WriteMemory(Esp - 16, Edi.Di);
                            Esp.Value -= 16;
                        }
                        break;
                    case "pushad":
                        if (parametersList.Length == 0)
                        {
                            WriteMemory(Esp - 4, Eax.Value);
                            WriteMemory(Esp - 8, Ecx.Value);
                            WriteMemory(Esp - 12, Edx.Value);
                            WriteMemory(Esp - 16, Ebx.Value);
                            WriteMemory(Esp - 20, Esp.Value);
                            WriteMemory(Esp - 24, Ebp.Value);
                            WriteMemory(Esp - 28, Esi.Value);
                            WriteMemory(Esp - 32, Edi.Value);
                            Esp.Value -= 32;
                        }
                        break;
                    case "popa":
                        if (parametersList.Length == 0)
                        {
                            Edi.Di = ReadWordFromMemory(Esp);
                            Esi.Si = ReadWordFromMemory(Esp + 2);
                            Ebp.Bp = ReadWordFromMemory(Esp + 4);
                            //Esp.Sp = ReadWord(Esp + 6);    // ignored
                            Ebx.Bx = ReadWordFromMemory(Esp + 8);
                            Edx.Dx = ReadWordFromMemory(Esp + 10);
                            Ecx.Cx = ReadWordFromMemory(Esp + 12);
                            Eax.Ax = ReadWordFromMemory(Esp + 14);
                            Esp.Value += 16;
                        }
                        else throw new ArgumentException();
                        break;
                    case "popad":
                        if (parametersList.Length == 0)
                        {
                            Edi.Value = ReadDwordFromMemory(Esp);
                            Esi.Value = ReadDwordFromMemory(Esp + 4);
                            Ebp.Value = ReadDwordFromMemory(Esp + 8);
                            //Esp.Value = ReadDoubleword(Esp + 12);    // ignored
                            Ebx.Value = ReadDwordFromMemory(Esp + 16);
                            Edx.Value = ReadDwordFromMemory(Esp + 20);
                            Ecx.Value = ReadDwordFromMemory(Esp + 24);
                            Eax.Value = ReadDwordFromMemory(Esp + 28);
                            Esp.Value += 32;
                        }
                        break;
                        // todo: pushf and popf, and effects on eflags
                    #endregion
                }
            }
            return false;
        }
        
        public void Execute(byte[] bytesArray)
        {
            UnmanagedBuffer buffer = new UnmanagedBuffer(bytesArray);
            Disasm disasm = new Disasm();
            disasm.EIP = buffer.Ptr;
            var byteTally = 0;
            while (byteTally < bytesArray.Length)
            {
                var result = BeaEngine.Disasm(disasm);
                if (result == (int)BeaConstants.SpecialInfo.UNKNOWN_OPCODE)
                    return;
                
                byteTally += result;
                var jumped = ExecuteASM(disasm, buffer, false);
                if (!jumped)
                    disasm.EIP = new IntPtr(disasm.EIP.ToInt64() + result);
            }
        }
        
        public void Reset()
        {
            CallOffsets = new List<uint>();
            JumpBacks = new List<uint>();
            
            Eax = new Eax();
            Ebx = new Ebx();
            Ecx = new Ecx();
            Edx = new Edx();
            Esi = new Esi();
            Edi = new Edi();
            Esp = new Esp();
            Ebp = new Ebp();
            Eip = 1;
            EFlags = EFlags.ZeroFlag;
            Esp.Value = 200;
        }

        public void Execute(int offset, UnmanagedBuffer buffer, bool followCalls = false)
        {
            Execute((uint)offset, buffer, followCalls);
        }

        public void Execute(uint offset, UnmanagedBuffer buffer, bool followCalls = false)
        {
            Disasm disasm = new Disasm();
            disasm.EIP = new IntPtr(buffer.Ptr.ToInt64() + offset);
            var bufferPTR = buffer.Ptr.ToInt64();
            var distToStart = new List<int>();
            var oldCallSize = CallOffsets.Count;
            while (true)
            {
                var result = BeaEngine.Disasm(disasm);
                if (result == (int)BeaConstants.SpecialInfo.UNKNOWN_OPCODE)
                    return;

                // Returns true if jumped or following calls
                var jumped = ExecuteASM(disasm, buffer, followCalls);
                if ((disasm.CompleteInstr.Contains("leave") || disasm.CompleteInstr.Contains("ret")) && !jumped)
                    break;

                if (!jumped)
                    disasm.EIP = new IntPtr(disasm.EIP.ToInt64() + result);
            }
            for (var i = 0; i < CallOffsets.Count; ++i)
                CallOffsets[i] += 0x400C00;
        }
        #endregion
        
        public Emulator(uint memoryBaseAddress, uint memorySize = 0x06400000)
        {
            if (uint.MaxValue - memoryBaseAddress > memorySize)
                BaseAddress = memoryBaseAddress;
            else throw new ArgumentOutOfRangeException();

            if (memorySize > 256 && memorySize <= 0xFFFFFFFF)
            {
                Memory = new byte[memorySize];
                Esp.Value = memoryBaseAddress + 200; // Allocate 200 bytes for the stack (we should not need more)
            }
            else throw new ArgumentOutOfRangeException("Memory size must be between 256 bytes and 4 gigabytes.");
        }
        
        public static Emulator Create(FileStream file)
        {
            var binary = new BinaryReader(file); // Can't using it.

            var emu = new Emulator(0, 0x32000000);
            var peInfo = new PeHeaderReader(file.Name, emu);

            // Load .text
            var code = peInfo.TEXT;
            binary.BaseStream.Seek(code.PhysicalAddress, SeekOrigin.Begin);
            for (uint i = 0; i < code.SizeOfRawData; ++i)
                emu.WriteByteToMemory(code.VirtualAddress + i + peInfo.OptionalHeader32.ImageBase, binary.ReadByte());
            
            /*var data = peInfo.DATA;
            binary.BaseStream.Seek(data.PhysicalAddress, SeekOrigin.Begin);
            for (uint i = 0; i < data.SizeOfRawData; ++i)
                emu.WriteByteToMemory(data.VirtualAddress + i - peInfo.OptionalHeader32.BaseOfData, binary.ReadByte());

            var rodata = peInfo.RODATA;
            binary.BaseStream.Seek(rodata.PhysicalAddress, SeekOrigin.Begin);
            for (uint i = 0; i < rodata.SizeOfRawData; ++i)
                emu.WriteByteToMemory(rodata.VirtualAddress + i - peInfo.OptionalHeader32.BaseOfData, binary.ReadByte());

            var rdata = peInfo.RDATA;
            binary.BaseStream.Seek(rdata.PhysicalAddress, SeekOrigin.Begin);
            for (uint i = 0; i < rdata.SizeOfRawData; ++i)
                emu.WriteByteToMemory(rdata.VirtualAddress + i - peInfo.OptionalHeader32.BaseOfData, binary.ReadByte());
            */
            //var offs = emu.Memory.FindPattern(new byte[] { 0,1,2,3,4, 0x3F, 0x3F,5,6,7,8,0x3F,9}, 0);
            return emu;
        }
        
        public uint[] GetCalledOffsets(int count = 0, int start = 0)
        {
            if (CallOffsets.Count == 0)
                return new uint[0];
            if (count == 0 && start == 0)
                return CallOffsets.ToArray();
            return CallOffsets.Skip(start < 0 ? Math.Max(0, CallOffsets.Count + start) : start).Take(count).ToArray();
        }
        
        public void Push(object value = null)
        {
            if (value == null)
            {
                Esp.Value -= 4;
                return;
            }
            
            var fieldSize = 0u;
            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                    fieldSize = 1;
                    break;
                case TypeCode.Int16:
                case TypeCode.UInt16:
                    fieldSize = 2;
                    break;
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Single:
                    fieldSize = 4;
                    break;
            }
            WriteMemory(Esp - fieldSize, value);
            Esp.Value -= fieldSize;
        }
    }
}
