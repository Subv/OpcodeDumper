using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using x86;

namespace OpcodeBruter.JamGroups
{
    public class ClientMovement : JamDispatch
    {
        public ClientMovement()
            : base()
        {

        }

        public override int StructureOffset {
            get { return 0x10151AC; }
        }
    }
}
