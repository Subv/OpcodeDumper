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
        public FileStream wowStream;
        public UnmanagedBuffer Disasm;

        public List<uint> Offsets = new List<uint>();
        public List<uint> IndirectJumpTable = new List<uint>();

        public virtual int StructureOffset { get { return 0; } }
        public virtual int PaddingSize { get { return 8; } }
        public int CalculateDispatcherFn() { return _dispatcher; }
        public int CalculateCheckerFn()    { return _checker; }
        public int CalculateConnectionFn() { return _connIndex; }

        private int _dispatcher;
        private int _checker;
        private int _connIndex;
        private JamGroup _groupName;

        public JamGroup GetGroup() { return _groupName; }

        public JamDispatch(FileStream wow)
        {
            this.wowStream = wow;
            Disasm = UnmanagedBuffer.CreateFromFile(wow);
            var binary = new BinaryReader(wow);
            binary.BaseStream.Seek(StructureOffset - 0x401400, SeekOrigin.Begin);

            var nameOffset = binary.ReadInt32() - 0x400C00;

            _checker = binary.ReadInt32() - 0x400C00;
            binary.ReadBytes(PaddingSize);
            _connIndex = binary.ReadInt32(); // Not the actual function - just skipping by
            if (_connIndex > 0)
                _connIndex -= 0x400C00;
            _dispatcher = binary.ReadInt32() - 0x400C00;
            _connIndex = binary.ReadInt32() - 0x400C00;

            binary.BaseStream.Seek(nameOffset, SeekOrigin.Begin);

            var groupName = String.Empty;
            while (binary.PeekChar() != '\0')
                groupName += binary.ReadChar();
            if (!Enum.TryParse(groupName, out _groupName))
                Console.WriteLine("Found JAM Group {0}, which is not defined in the JamGroup enum!", groupName);
        }
    }
}
