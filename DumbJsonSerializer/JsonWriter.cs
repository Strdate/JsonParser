using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DumbJsonSerializer
{
    internal class JsonWriter
    {
        private readonly TypeManager typeManager;
        private readonly object sourceObj;
        private readonly StringBuilder b = new StringBuilder();

        internal JsonWriter(TypeManager typeManager, object obj)
        {
            this.typeManager = typeManager;
            this.sourceObj = obj;
        }

        internal string Write()
        {
            WriteJsonType(sourceObj, TypeManager.TypeByType(sourceObj.GetType()));
            return b.ToString();
        }

        private void WriteJsonType(object obj, TypeHandlingRecord implicitType)
        {
            Type t = obj.GetType();
            TypeHandlingRecord handleType = TypeManager.TypeByType(t);

            if (handleType.handleType == HandleType.List) {
                WriteList(obj, implicitType);
            } else {
                if (handleType.handleType == HandleType.StructuredObject) {
                    WriteJsonObject(obj, t, implicitType != null ? t == implicitType.type : false);
                } else if (handleType.handleType == HandleType.ToStringEscaped) {
                    EscapeStr(obj.ToString());
                } else if (handleType.handleType == HandleType.ToStringNaked) {
                    b.Append(obj.ToString());
                } else if (handleType.handleType == HandleType.ToStringLower) {
                    b.Append($"{obj.ToString().ToLower()}");
                } else if (handleType.handleType == HandleType.DecimalNum) {
                    b.Append($"{obj.ToString().Replace(',','.')}");
                }
            }
        }

        private void WriteList(object obj, TypeHandlingRecord record)
        {
            System.Collections.IEnumerable list = obj as System.Collections.IEnumerable;
            b.Append($"[");
            bool isFirst = true;
            foreach (var item in list) {
                if (!isFirst) {
                    b.Append(",");
                } else {
                    isFirst = false;
                }
                WriteJsonType(item, record.sourceListType);
            }
            b.Append("]");
        }

        private void WriteJsonObject(object obj, Type type, bool implicitType)
        {
            b.Append("{");
            bool isFirst = true;
            if (!implicitType) {
                isFirst = false;
                b.Append($"\"__type\": \"{type.FullName}\"");
            }
            foreach (var propr in typeManager.GetProperties(type)) {
                var prop = propr.prop;
                object val = prop.GetValue(obj, null);
                if (val != null) {
                    if (!isFirst) {
                        b.Append(",");
                    } else {
                        isFirst = false;
                    }
                    b.Append($"\"{prop.Name}\":");
                    WriteJsonType(val, propr.typeRecord);
                }

            }
            b.Append("}");
        }

        private void EscapeStr(string s)
        {
            b.Append('"');
            if (s == null || s.Length == 0) {
                b.Append('"');
                return;
            }
            char c;
            int i;
            bool lastWasCR = false;

            for (i = 0; i < s.Length; i += 1) {
                c = s[i];
                switch (c) {
                    case '\\':
                    case '"':
                    case '/':
                        b.Append('\\');
                        b.Append(c);
                        break;
                    case '\r':
                        b.Append("\\n");
                        lastWasCR = true;
                        break;
                    case '\n':
                        if(lastWasCR) {
                            lastWasCR = false;
                        } else {
                            b.Append("\\n");
                        }
                        break;
                    default:
                        b.Append(c);
                        break;
                }
            }
            b.Append('"');
        }
    }
}
