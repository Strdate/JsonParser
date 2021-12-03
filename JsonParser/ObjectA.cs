using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonParser
{
    public class ObjectA
    {
        public string StringProp { get; set; }

        public double CoolNumber { get; set; }

        public List<int> intArray { get; set; }

        public int Integer { get; set; }

        public ObjectB objB { get; set; }

        public List<ObjectB> booArray { get; set; }
    }
}
