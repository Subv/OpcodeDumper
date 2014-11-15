using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using Bea;
using System.Diagnostics;
using x86;

namespace OpcodeBruter.JamGroups
{
    public class ClientSpell : JamDispatch
    {
        public override int StructureOffset {
            get { return 0x1014BD0; }
        }

        public ClientSpell(FileStream wow)
            : base(wow)
        {

        }
    }
}
