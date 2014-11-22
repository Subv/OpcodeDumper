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
    public class ClientGuild : JamDispatch
    {
        public override int StructureOffset {
            get { return 0xFA01C4; }
        }

        public ClientGuild() : base()
        {
        }
    }
}
