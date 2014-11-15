using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace x86
{
    public enum RegisterSubType
    {
        None,
        Full,
        LowWord,
        HighByte,
        LowByte
    }

    /// <summary>
    /// Basic 32-bit register container.
    /// </summary>
    [DebuggerDisplay("Value = {Value}")]
    [Serializable()]
    public class Register
    {
        #region Field
        /// <summary>
        /// The value of the register.
        /// </summary>
        public uint Value;
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the register with a value of zero.
        /// </summary>
        public Register() { }

        /// <summary>
        /// Initializes the register with the specified value.
        /// </summary>
        /// <param name="value">Register value.</param>
        public Register(uint value)
        {
            Value = value;
        }
        #endregion

        #region Properties
        /// <summary>
        /// The low 16 bits of the register.
        /// </summary>
        public ushort LowWord
        {
            get
            {
                return (ushort)(Value & 0xFFFF);
            }
            set
            {
                Value = (Value & 0xFFFF0000) | value;
            }
        }

        /// <summary>
        /// The high 8 bits of the low register word.
        /// </summary>
        public byte HighByte
        {
            get
            {
                return (byte)((Value & 0xFF00) >> 8);
            }
            set
            {
                Value = (Value & 0xFFFF00FF) | (uint)((ushort)value << 8);
            }
        }

        /// <summary>
        /// The low 8 bits of the register.
        /// </summary>
        public byte LowByte
        {
            get
            {
                return (byte)(Value & 0xFF);
            }
            set
            {
                Value = (Value & 0xFFFFFF00) | value;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets the specified part of the register to the specified value.
        /// </summary>
        /// <param name="type">The specific part of the register to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetValue(RegisterSubType type, uint value)
        {
            switch (type)
            {
                case RegisterSubType.Full: Value = value; break;
                case RegisterSubType.LowWord: LowWord = (ushort)value; break;
                case RegisterSubType.LowByte: LowByte = (byte)value; break;
                case RegisterSubType.HighByte: HighByte = (byte)value; break;
                default: throw new ArgumentException();
            }
        }

        /// <summary>
        /// Gets the value of the specified part of the register.
        /// </summary>
        /// <param name="type">The specific part of the register to get.</param>
        /// <returns></returns>
        public uint GetValue(RegisterSubType type)
        {
            switch (type)
            {
                case RegisterSubType.Full: return Value;
                case RegisterSubType.LowWord: return LowWord;
                case RegisterSubType.LowByte: return LowByte;
                case RegisterSubType.HighByte: return HighByte;
                default: throw new ArgumentException();
            }
        }
        #endregion

        #region Operator Overloads
        /// <summary>
        /// Allows automatic casting from a register to an unsigned int.
        /// </summary>
        /// <param name="reg"></param>
        /// <returns></returns>
        public static implicit operator uint(Register reg)
        {
            return reg.Value;
        }

        /// <summary>
        /// Allows automatic casting from an unsigned int to a register.
        /// </summary>
        /// <param name="reg"></param>
        /// <returns></returns>
        public static implicit operator Register(uint reg)
        {
            return new Register(reg);
        }
        #endregion
    }

    /// <summary>
    /// Accumulator for operands and results data.
    /// </summary>
    [Serializable()]
    public class Eax : Register
    {
        public readonly string Name = "eax";
        public ushort Ax { get { return LowWord; } set { LowWord = value; } }
        public byte Al { get { return LowByte; } set { LowByte = value; } }
        public byte Ah { get { return HighByte; } set { HighByte = value; } }
    }

    /// <summary>
    /// Data pointer.
    /// </summary>
    [Serializable()]
    public class Ebx : Register
    {
        public readonly string Name = "ebx";
        public ushort Bx { get { return LowWord; } set { LowWord = value; } }
        public byte Bl { get { return LowByte; } set { LowByte = value; } }
        public byte Bh { get { return HighByte; } set { HighByte = value; } }
    }

    /// <summary>
    /// Counter for string and loop operations.
    /// </summary>
    [Serializable()]
    public class Ecx : Register
    {
        public readonly string Name = "ecx";
        public ushort Cx { get { return LowWord; } set { LowWord = value; } }
        public byte Cl { get { return LowByte; } set { LowByte = value; } }
        public byte Ch { get { return HighByte; } set { HighByte = value; } }
    }

    /// <summary>
    /// I/O pointer.
    /// </summary>
    [Serializable()]
    public class Edx : Register
    {
        public readonly string Name = "edx";
        public ushort Dx { get { return LowWord; } set { LowWord = value; } }
        public byte Dl { get { return LowByte; } set { LowByte = value; } }
        public byte Dh { get { return HighByte; } set { HighByte = value; } }
    }

    /// <summary>
    /// Stack pointer.
    /// </summary>
    [Serializable()]
    public class Esp : Register
    {
        public readonly string Name = "esp";
        public ushort Sp { get { return LowWord; } set { LowWord = value; } }
    }

    /// <summary>
    /// Stack frame pointer.
    /// </summary>
    [Serializable()]
    public class Ebp : Register
    {
        public readonly string Name = "ebp";
        public ushort Bp { get { return LowWord; } set { LowWord = value; } }
    }

    /// <summary>
    /// Source pointer for string operations.
    /// </summary>
    [Serializable()]
    public class Esi : Register
    {
        public readonly string Name = "esi";
        public ushort Si { get { return LowWord; } set { LowWord = value; } }
    }

    /// <summary>
    /// Destination pointer for string operations.
    /// </summary>
    [Serializable()]
    public class Edi : Register
    {
        public readonly string Name = "edi";
        public ushort Di { get { return LowWord; } set { LowWord = value; } }
    }

    /// <summary>
    /// Mapping of all basic execution status flags. System and reserved flags are not included.
    /// </summary>
    [Flags]
    public enum EFlags
    {
        /// <summary>
        /// CF. Set if an arithmetic operation generates a carry or a borrow out 
        /// of the most-significant bit of the result; cleared if otherwise.  This flag 
        /// indicates an overflow condition for unsigned-integer arithmetic. It is also 
        /// used in multiple-precision arithmetic.
        /// </summary>
        CarryFlag = 1 << 0,

        /// <summary>
        /// PF. Set if the least-significant byte of the result contains an even 
        /// number of 1 bits; cleared otherwise.
        /// </summary>
        ParityFlag = 1 << 2,

        /// <summary>
        /// AF. Set if an arithmetic operation generates a carry or a borrow out 
        /// of bit 3 of the result; cleared otherwise. This flag is used in BCD arithmetic.
        /// </summary>
        AdjustFlag = 1 << 4,

        /// <summary>
        /// ZF. Set if the result is zero; cleared otherwise.
        /// </summary>
        ZeroFlag = 1 << 6,

        /// <summary>
        /// SF. Set equal to the most-significant bit of the result, which is the 
        /// sign bit of a signed integer.  Zero indicates a positive value and one indicates 
        /// a negative value.
        /// </summary>
        SignFlag = 1 << 7,

        /// <summary>
        /// DF. When set, causes string instructions to auto-decrement (process 
        /// strings from high to low addresses), otherwise they auto-increment by default.
        /// </summary>
        DirectionFlag = 1 << 10,

        /// <summary>
        /// OF. Set if the integer result is too large a positive number or too 
        /// small a negative number (excluding the sign-bit) to fit in the destination operand; 
        /// cleared otherwise. This flag indicates an overflow condition for signed-integer 
        /// (two's complement) arithmetic.
        /// </summary>
        OverflowFlag = 1 << 11,
    }
}