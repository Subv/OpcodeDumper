using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace x86
{
    [Flags]
    public enum BreakpointType
    {
        MemoryRead,
        MemoryWrite,
        InstructionOffset,
        InstructionName
    };

    static class Debugger
    {
        private static List<BP> Breakpoints = new List<BP>();

        public static void Reset()
        {
            Breakpoints.Clear();
        }

        public static void AddBreakpoint<T>(BreakpointType type, T parameter, bool stopOnHit = true, bool oneTime = false)
        {
            Breakpoints.Add(new Breakpoint<T>(type, parameter, stopOnHit, oneTime));
        }

        public static void TryTrigger<T>(Emulator emu, BreakpointType type, T value)
        {
            if (Breakpoints.Count(t => { var v = (Breakpoint<T>)t; return v.BpType.HasFlag(type) && (v.Value == null ? true : v.Value.Equals(value)); }) == 0)
                return;

            var bps = Breakpoints.Where(t => { var v = (Breakpoint<T>)t; return v.BpType.HasFlag(type) && (v.Value == null ? true : v.Value.Equals(value)); });
            foreach (var bp in bps)
            {
                if (bp.OneTimeOnly)
                    Breakpoints.Remove(bp);

                if (bp.StopOnHit)
                    emu.Interrupt();
            }
        }
    }

    public class BP
    {
        public bool StopOnHit;
        public bool OneTimeOnly;
    }

    public class Breakpoint<T> : BP
    {
        public BreakpointType BpType;
        public T Value;

        public Breakpoint(BreakpointType type, T parameter, bool stopOnHit, bool oneTime)
        {
            BpType = type;
            Value = parameter;
            StopOnHit = stopOnHit;
            OneTimeOnly = oneTime;
        }
    }
}
