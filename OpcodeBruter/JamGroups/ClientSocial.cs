using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using x86;

namespace OpcodeBruter.JamGroups
{
    public class ClientSocial : JamDispatch
    {
        public override int StructureOffset {
            get { return 0x00FA0C5C; }
        }

        public ClientSocial(FileStream wow)
            : base(wow)
        {

        }
    }
}
