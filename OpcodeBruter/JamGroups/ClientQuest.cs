using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using x86;

namespace OpcodeBruter.JamGroups
{
    public class ClientQuest : JamDispatch
    {
        public ClientQuest(FileStream wow)
            : base(wow)
        {

        }
        
        public override int StructureOffset {
            get { return 0x00F9E700; }
        }
    }
}
