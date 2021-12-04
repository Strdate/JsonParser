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
        public object ReadJson(string text, string implicitType = null)
        {
            var reader = new JsonReader(text, typeManager);
            return reader.Read(implicitType);
        }

        public string WriteJson(object obj)
        {
            var writer = new JsonWriter(typeManager, obj);
            return writer.Write();
        }
    }
}
