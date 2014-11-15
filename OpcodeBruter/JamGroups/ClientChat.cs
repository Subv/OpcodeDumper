using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using x86;

namespace OpcodeBruter.JamGroups
{
    public class ClientChat : JamDispatch
    {
        public override int StructureOffset
        {
            get { return 0x010158A4; }
        }

        public ClientChat(FileStream wow)
            : base(wow)
        {

        }
    }
}
