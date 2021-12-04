using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonParser
{
    class JsonWriter
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
            WriteJsonType(sourceObj, null, sourceObj.GetType());
            return b.ToString();
        }

        private void WriteJsonType(object obj, string propName = null, Type implicitType = null)
        {
            Type t = obj.GetType();
            TypeHandlingRecord handleType = typeManager.GetTypeHandlingRecord(t);

            if (handleType.handleType == HandleType.List) {
                WriteList(obj, propName, t, handleType.genArgument);
            } else {
                if(propName != null) {
                    b.Append($"\"{propName}\":");
                }

                if (handleType.handleType == HandleType.StructuredObject) {
                    WriteJsonObject(obj, t, t == implicitType);
                } else if (handleType.handleType == HandleType.ToStringEscaped) {
                    b.Append($"\"{EscapeStr(obj.ToString())}\"");
                } else if (handleType.handleType == HandleType.ToStringNaked) {
                    b.Append($"{EscapeStr(obj.ToString())}");
                } else if (handleType.handleType == HandleType.DecimalNum) {
                    b.Append($"{obj.ToString().Replace(',','.')}");
                }
            }
            
            
        }

        private void WriteList(object obj, string propName, Type type, Type genericArg)
        {
            b.Append($"\"__type_{propName}\": \"{genericArg.Name}\",");
            System.Collections.IEnumerable list = obj as System.Collections.IEnumerable;
            b.Append($"\"{propName}\": [");
            bool isFirst = true;
            foreach (var item in list) {
                if (!isFirst) {
                    b.Append(",");
                } else {
                    isFirst = false;
                }
                WriteJsonType(item, null, genericArg);
            }
            b.Append("]");
        }

        private void WriteJsonObject(object obj, Type type, bool implicitType)
        {
            b.Append("{");
            bool isFirst = true;
            if (!implicitType) {
                isFirst = false;
                b.Append($"\"__type\": \"{type.Name}\"");
            }
            foreach (var prop in typeManager.GetProperties(type.Name)) {
                if (!isFirst) {
                    b.Append(",");
                } else {
                    isFirst = false;
                }
                object val = prop.GetValue(obj);
                if (val != null) {
                    WriteJsonType(val, prop.Name);
                }

            }
            b.Append("}");
        }

        private static string EscapeStr(string s)
        {
            if (s == null || s.Length == 0) {
                return "";
            }

            char c = '\0';
            int i;
            int len = s.Length;
            StringBuilder sb = new StringBuilder(len + 4);
            String t;

            for (i = 0; i < len; i += 1) {
                c = s[i];
                switch (c) {
                    case '\\':
                    case '"':
                        sb.Append('\\');
                        sb.Append(c);
                        break;
                    /*case '/':
                        sb.Append('\\');
                        sb.Append(c);
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;*/
                    default:
                        sb.Append(c);
                        /*if (c < ' ') {
                            t = "000" + String.Format("X", c);
                            sb.Append("\\u" + t.Substring(t.Length - 4));
                        } else {
                            sb.Append(c);
                        }*/
                        break;
                }
            }
            return sb.ToString();
        }
    }
}
