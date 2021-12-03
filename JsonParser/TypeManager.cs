using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JsonParser
{
    class TypeManager
    {
        private static Dictionary<string, Type> types = new Dictionary<string, Type>();

        private static Dictionary<string, ListTypeRecord> listTypes = new Dictionary<string, ListTypeRecord>();

        internal static Dictionary<string, PropertyInfo> properties = new Dictionary<string, PropertyInfo>();

        internal ParsedObject InstantiateObj(string name)
        {
            Type t = TypeByName(name);
            return new ParsedObject(Activator.CreateInstance(t), name);
        }

        internal object InstantiateList(string name, out MethodInfo addMethod)
        {
            ListTypeRecord record = ListTypeByName(name);
            addMethod = record.addMethod;
            return Activator.CreateInstance(record.type);
        }

        private static ListTypeRecord ListTypeByName(string name)
        {
            if (!listTypes.TryGetValue(name, out ListTypeRecord record)) {
                Type t = typeof(List<>);
                Type[] typeArgs = { TypeByName(name) };
                t = t.MakeGenericType(typeArgs);
                MethodInfo addMethod = t.GetMethod("Add");
                record.type = t;
                record.addMethod = addMethod;
                listTypes[name] = record;
            }
            return record;
        }

        private static Type TypeByName(string name)
        {
            if(!types.TryGetValue(name, out Type type)) {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Reverse()) {
                    foreach (var t in assembly.GetTypes()) {
                        if (t.Name == name || t.FullName == name) {
                            types[name] = t;
                            return t;
                        }
                    }
                }
                throw new Exception($"Failed to find type '{name}'");
            }
            return type;
        }

        internal static PropertyInfo GetProperty(string typeName, string propertyName)
        {
            string combined = typeName + "@@p#" + propertyName;
            if (!properties.TryGetValue(combined, out PropertyInfo prop)) {
                Type type = TypeByName(typeName);
                prop = type.GetProperty(propertyName);
                properties[combined] = prop;
            }
            return prop;
        }
    }

    class ParsedObject
    {
        private object inner;
        private string typeString;

        internal ParsedObject(object inner, string typeString)
        {
            this.inner = inner;
            this.typeString = typeString;
        }

        internal void AssignProperty(string name, object obj)
        {
            PropertyInfo prop = TypeManager.GetProperty(typeString, name);
            if(prop != null) {
                prop.SetValue(inner, obj);
            }
        }

        internal object GetObject() => inner;
    }

    struct ListTypeRecord
    {
        internal Type type;
        internal MethodInfo addMethod;
    }
}
