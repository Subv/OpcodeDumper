using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Bea;

namespace OpcodeBruter
{
    public abstract class JamDispatch
    {
        public List<uint> Offsets = new List<uint>();
        public List<uint> IndirectJumpTable = new List<uint>();

        public virtual int StructureOffset { get { return 0; } }
        // Not actual padding - just two vtable entries nulled.
        public virtual int PaddingSize { get { return 8; } }
        public int CalculateDispatcherFn() { return _dispatcher; }
        public int CalculateCheckerFn()    { return _checker; }
        public int CalculateConnectionFn() { return _connIndex; }

        private int _dispatcher;
        private int _checker;
        private int _connIndex;
        private JamGroup _groupName;

        public JamGroup GetGroup() { return _groupName; }

        public JamDispatch()
        {
            Program.BaseStream.Seek(StructureOffset - 0x401400, SeekOrigin.Begin);

            var nameOffset = Program.ClientStream.ReadInt32() - 0x400C00;

            _checker = Program.ClientStream.ReadInt32() - 0x400C00;
            Program.ClientStream.ReadBytes(PaddingSize + 4);
            _dispatcher = Program.ClientStream.ReadInt32() - 0x400C00;
            _connIndex = Program.ClientStream.ReadInt32();
            if (_connIndex > 0)
                _connIndex -= 0x400C00;

            Program.BaseStream.Seek(nameOffset, SeekOrigin.Begin);

            // ReadString fails, and I'm really not sure why.
            var groupName = String.Empty;
            while (Program.ClientStream.PeekChar() != '\0')
                groupName += Program.ClientStream.ReadChar();

            if (!Enum.TryParse(groupName, out _groupName))
                Logger.WriteConsoleLine("Found JAM Group {0}, which is not defined in the JamGroup enum!", groupName);
        }
    }
}
