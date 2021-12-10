using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DumbJsonParser
{
    [Serializable]
    public class ObjectA
    {
        public string StringProp { get; set; }

        public double CoolNumber { get; set; }

        public List<int> intArray { get; set; }

        public int Integer { get; set; }

        public long VeryLong { get; set; }

        public bool TruOrFols { get; private set; }

        public bool Fols { get; private set; }

        public string newLines { get; private set; }

        public sbyte Sbytee { get; private set; }

        public ObjectB objB { get; set; }

        public List<ObjectB> booArray { get; set; }
    }
}
