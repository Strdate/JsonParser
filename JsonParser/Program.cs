using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonParser
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new JsonParser();
            string json = "{ \"__type\": \"ObjectA\"," +
                "\"StringProp\": \"Hello world!!\", " +
                "\"objB\": { " +
                "  \"__type\": \"ObjectB\"," +
                "  \"Foo\": \"bar\"" +
                "}, " +
                "\"__type_booArray\": \"ObjectB\"," +
                "\"booArray\": [" +
                "  { " +
                "    \"Foo\": \"hello :)\"" +
                "  }, " +
                "  { " +
                "    \"__type\": \"ObjectB\"," +
                "    \"Foo\": \"world :)\"," +
                "    \"WierdStr\": \"\\\"I'm escaped!\\\" :)\"" +
                "  }" +
                "]," +
                "\"CoolNumber\": 152.998," +
                "\"Integer\": 15504," +
                "\"__type_intArray\": \"Int32\"," +
                "\"intArray\": [27, 98, 1145983, 5]" +
                "}";

            string json2 = "{" +
                "\"StringProp\": \"Hello world!!\", " +
                "\"objB\": { " +
                "  \"__type\": \"ObjectB\"," +
                "  \"Foo\": \"bar\"" +
                "} " +
                "}";

            object res = parser.ReadJson(json);
            object res2 = parser.ReadJson(json2, "ObjectA");

            string serialized = parser.WriteJson(res);

            Console.ReadLine();
        }
    }
}
