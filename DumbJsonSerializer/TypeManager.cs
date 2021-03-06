using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace DumbJsonSerializer
{
    internal class TypeManager
    {
        private static Dictionary<string, TypeHandlingRecord> types = new Dictionary<string, TypeHandlingRecord>();

        private static Dictionary<Type, TypeHandlingRecord> typesByType = new Dictionary<Type, TypeHandlingRecord>();

        internal ParsedObject InstantiateObj(TypeHandlingRecord record)
        {
            return new ParsedObject(Activator.CreateInstance(record.type, true), record);
        }

        internal ParsedObject InstantiateObj(string typeName)
        {
            TypeHandlingRecord record = TypeByName(typeName);
            return new ParsedObject(Activator.CreateInstance(record.type, true), record);
        }

        internal static TypeHandlingRecord TypeByType(Type type)
        {
            if (!typesByType.TryGetValue(type, out TypeHandlingRecord record)) {
                return PrepareType(type);
            }
            return record;
        }

        internal static TypeHandlingRecord TypeByName(string name)
        {
            if(!types.TryGetValue(name, out TypeHandlingRecord record)) {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Reverse()) {
                    foreach (var type in assembly.GetTypes()) {
                        if (type.Name == name || type.FullName == name) {
                            return PrepareType(type);
                        }
                    }
                }
                throw new Exception($"Failed to find type '{name}'");
            }
            return record;
        }

        private static TypeHandlingRecord PrepareType(Type type)
        {
            TypeHandlingRecord record = new TypeHandlingRecord();
            if (type.GetCustomAttributes(
                                typeof(SerializableAttribute), true
                                ).FirstOrDefault() == null) {
                throw new Exception($"Class {type.Name} is not serializable");
            }
            if(type.IsArray) {
                throw new Exception("Arrays are not supported. Use List instead");
            } else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) {
                record.handleType = HandleType.List;
                record.sourceListType = TypeByType(type.GetGenericArguments()[0]);
            } else if (type == typeof(string)) {
                record.handleType = HandleType.ToStringEscaped;
            } else if (type == typeof(int) || type == typeof(uint) || type == typeof(long) || type == typeof(ulong) || type == typeof(short)
                 || type == typeof(ushort) || type == typeof(byte) || type == typeof(sbyte)) {
                record.handleType = HandleType.ToStringNaked;
            } else if (type == typeof(double) || type == typeof(float)) {
                record.handleType = HandleType.DecimalNum;
            } else if (type == typeof(bool)) {
                record.handleType = HandleType.ToStringLower;
            } else {
                record.handleType = HandleType.StructuredObject;
            }
            record.type = type;
            types[type.FullName] = record;
            types[type.Name] = record;
            typesByType[type] = record;
            return record;
        }

        internal static PropertyRecord GetProperty(Type type, string propertyName)
        {
            TypeByType(type);
            return typesByType[type].props[propertyName];
        }

        internal Dictionary<string, PropertyRecord>.ValueCollection GetProperties(Type type)
        {
            TypeByType(type);
            return typesByType[type].props.Values;
        }
    }

    internal class ParsedObject
    {
        private object inner;
        private TypeHandlingRecord typeRecord;

        internal ParsedObject(object inner, TypeHandlingRecord record)
        {
            this.inner = inner;
            this.typeRecord = record;
        }

        internal PropertyRecord GetProperty(string name)
        {
            if(typeRecord.props.TryGetValue(name, out PropertyRecord val)) {
                return val;
            }
            return null;
        }

        internal void SetProperty(PropertyRecord record, object value)
        {
            record.prop.SetValue(inner, value, null);
        }

        internal object GetObject() => inner;
    }

    internal class TypeHandlingRecord
    {
        internal HandleType handleType;
        internal Type type;
        internal TypeHandlingRecord sourceListType;

        private Dictionary<string, PropertyRecord> _props;
        internal Dictionary<string, PropertyRecord> props  {
            get {
                if (_props == null) {
                    InitProperties();
                }
                return _props;
            }
        }

        private ListTypeRecord _listRecord;
        private ListTypeRecord listRecord {
            get {
                if (_listRecord == null) {
                    InitListRecord();
                }
                return _listRecord;
            }
        }

        internal object InstantiateList(out MethodInfo addMethod)
        {
            addMethod = listRecord.addMethod;
            return Activator.CreateInstance(listRecord.listType);
        }

        private void InitProperties()
        {
            var dict = new Dictionary<string, PropertyRecord>();
            foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
                if (prop.GetCustomAttributes(
                    typeof(XmlIgnoreAttribute), true
                    ).FirstOrDefault() == null && !prop.PropertyType.IsArray) {
                    PropertyRecord record = new PropertyRecord();
                    record.typeRecord = TypeManager.TypeByType(prop.PropertyType);
                    record.prop = prop;
                    dict.Add(prop.Name, record);
                }
            }
            this._props = dict;
        }

        private void InitListRecord()
        {
            ListTypeRecord record = new ListTypeRecord();
            Type t = typeof(List<>);
            Type[] typeArgs = { this.type };
            t = t.MakeGenericType(typeArgs);
            MethodInfo addMethod = t.GetMethod("Add");
            record.listType = t;
            record.addMethod = addMethod;
            this._listRecord = record;
        }
    }

    internal class ListTypeRecord
    {
        internal Type listType;
        internal MethodInfo addMethod;
    }

    internal class PropertyRecord
    {
        internal PropertyInfo prop;
        internal TypeHandlingRecord typeRecord;
    }

    internal enum HandleType
    {
        ToStringEscaped,
        ToStringNaked,
        ToStringLower,
        DecimalNum,
        List,
        StructuredObject
    }
}
