using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Bea;
using x86;

namespace OpcodeBruter.JamGroups
{
    public class Client : JamDispatch
    {
        public override int StructureOffset {
            get { return 0x00F9E8F4; }
        }

        public Client(FileStream wow) : base(wow)
        {

        }
    }
}
