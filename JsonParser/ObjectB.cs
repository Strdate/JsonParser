using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DumbJsonSerializer
{
    [Serializable]
    public class ObjectB
    {
        public string Foo { get; private set; }

        public string WierdStr { get; set; }
    }
}
