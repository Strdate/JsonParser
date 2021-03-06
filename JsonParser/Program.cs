using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DumbJsonSerializer
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new JsonSerializer();
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
                "    \"WierdStr\": \"\\\"I'm escaped!\\\" :) \\/\"" +
                "  }" +
                "]," +
                "\"CoolNumber\": 152.998," +
                "\"VeryLong\": 9223372036854775806," +
                "\"Integer\": 15504," +
                "\"missingProp\": {\"ImNotHere\": 42.42}," +
                "\"newLines\": \"New\\nline\\nboo!\"," +
                "\"vec\": { " +
                "  \"__type\": \"Vector\"," +
                "  \"x\": 10.0" +
                "  \"y\": 20.0" +
                "  \"z\": 30.0" +
                "}, " +
                "\"__type_intArray\": \"Int32\"," +
                "\"Sbytee\": -117," +
                "\"TruOrFols\": true," +
                "\"Fols\": false," +
                "\"NumInObject\": 234," +
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

            object utf8 = Encoding.UTF8.GetString( Encoding.UTF8.GetBytes(serialized) );

            

            Console.ReadLine();
        }
    }
}
