using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonParser
{
    class JsonParser
    {
        private TypeManager typeManager = new TypeManager();
        public object ReadJson(string text)
        {
            var reader = new JsonReader(text, typeManager);
            return reader.Read();
        }
    }
}
