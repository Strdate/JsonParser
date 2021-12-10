using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DumbJsonSerializer
{
    public class JsonSerializer
    {
        private TypeManager typeManager = new TypeManager();

        public static string VERSION = "0.0.2";
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
