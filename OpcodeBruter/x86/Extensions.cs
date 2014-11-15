using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace x86
{
    public static class Extensions
    {
        private const EFlags AllEFlags = EFlags.AdjustFlag | EFlags.CarryFlag | EFlags.DirectionFlag | EFlags.OverflowFlag | EFlags.ParityFlag | EFlags.SignFlag | EFlags.ZeroFlag;

        public static EFlags Set(this EFlags eFlags, EFlags flags)
        {
            return eFlags | flags & AllEFlags;
        }

        public static EFlags Clear(this EFlags eFlags, EFlags flags)
        {
            return eFlags & (EFlags)(AllEFlags - flags);
        }

        public static EFlags Clear(this EFlags eFlags)
        {
            return EFlags.ZeroFlag;
        }
    }
}
