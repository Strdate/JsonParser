using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DumbJsonParser
{
    class JsonReader
    {
        private readonly TypeManager typeManager;

        private readonly string text;

        private int p;

        internal JsonReader(string text, TypeManager typeManager)
        {
            this.text = text;
            this.typeManager = typeManager;
        }

        private char ReadNext()
        {
            char c = text[p];
            p++;
            return c;
        }

        internal object Read(string implicitType = null)
        {
            //try {
            TypeHandlingRecord record = null;
            if(implicitType != null) {
                record = TypeManager.TypeByName(implicitType);
            }
            return ReadJsonObject(record);
            //} catch(Exception e) {
            //    throw new Exception($"Error at character {p}: {e.Message}", e);
            //}
        }

        private object ReadJsonObject(TypeHandlingRecord type = null)
        {
            bool firstPass = true;

            Expect('{');
            ParsedObject obj = null;
            while(true) {
                if (text[p] == '}') {
                    p++;
                    if(firstPass) {
                        if(type != null) {
                            obj = typeManager.InstantiateObj(type);
                        } // else warning
                        break;
                    }
                    break;
                }
                string fieldName = ReadString();
                Expect(':');
                if(firstPass && fieldName == "__type") {
                    string typeName = ReadString();
                    obj = typeManager.InstantiateObj(typeName);
                } else {
                    if(firstPass) {
                        if (type != null) {
                            obj = typeManager.InstantiateObj(type);
                        } // else warning
                    }
                    PropertyRecord prop = null;
                    if (obj != null) {
                        prop = obj.GetProperty(fieldName);
                    }
                    if(prop != null) {
                        object val = ReadJsonType(prop.typeRecord);
                        obj.SetProperty(prop, val);
                    } else {
                        ReadJsonType(null);
                    }
                }
                firstPass = false;
                SkipComma();
            }
            return obj.GetObject();
        }

        private object ReadJsonType(TypeHandlingRecord type)
        {
            SkipWS();
            char nchar = text[p];
            if (nchar == '{') {
                return ReadJsonObject(type);
            } else if (nchar == '[') {
                return ReadArray(type);
            } else if (nchar == '"') {
                return ReadString();
            } else if (Char.IsNumber(nchar) || nchar == '-') {
                return ReadNumber(type);
            } else if (nchar == 't' || nchar == 'f') {
                return ReadBool();
            } else {
                throw new Exception("Expected JSON type");
            }
        }

        private object ReadArray(TypeHandlingRecord type)
        {
            object list = null;
            MethodInfo addMethod = null;
            if(type != null) {
                list = type.sourceListType.InstantiateList(out addMethod);
            }
            Expect('[');
            while(true) {
                SkipWS();
                char nchar = text[p];
                if(nchar == ']') {
                    p++;
                    return list;
                }
                object val = ReadJsonType(type != null ? type.sourceListType : null);
                if(list != null) {
                    addMethod.Invoke(list, new object[] { val });
                }
                SkipComma();
            }
        }

        private object ReadNumber(TypeHandlingRecord type)
        {
            StringBuilder builder = new StringBuilder();
            bool isInteger = true;
            while (true) {
                char cur = text[p];
                if (Char.IsNumber(cur) || cur == '-') {
                    builder.Append(cur);
                    p++;
                } else if (cur == '.') {
                    builder.Append(cur);
                    p++;
                    isInteger = false;
                } else {
                    if (type == null) {
                        return null;
                    }
                    Type t = type.type;
                    if (isInteger) {
                        if (t == typeof(int)) {
                            return int.Parse(builder.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture);
                        } else if (t == typeof(long)) {
                            return long.Parse(builder.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture);
                        } else if (t == typeof(uint)) {
                            return uint.Parse(builder.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture);
                        } else if (t == typeof(ulong)) {
                            return ulong.Parse(builder.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture);
                        } else if (t == typeof(short)) {
                            return short.Parse(builder.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture);
                        } else if (t == typeof(ushort)) {
                            return ushort.Parse(builder.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture);
                        } else if (t == typeof(byte)) {
                            return byte.Parse(builder.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture);
                        } else if (t == typeof(sbyte)) {
                            return sbyte.Parse(builder.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture);
                        } else {
                            throw new Exception($"Cannot parse '{builder}' to {t.Name}");
                        }
                    } else {
                        if (t == typeof(double)) {
                            return double.Parse(builder.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture);
                        } else if (t == typeof(float)) {
                            return float.Parse(builder.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture);
                        } else if (t == typeof(uint)) {
                            return uint.Parse(builder.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture);
                        } else if (t == typeof(decimal)) {
                            return decimal.Parse(builder.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture);
                        } else {
                            throw new Exception($"Cannot parse '{builder}' to {t.Name}");
                        }
                    }
                }
            }
        }

        private object ReadBool()
        {
            char cur = text[p];
            if(cur == 't') {
                if(text[p + 1] != 'r' || text[p + 2] != 'u' || text[p + 3] != 'e') {
                    throw new Exception("Expected 'true'");
                } else {
                    p += 4;
                    return true;
                }
            } else {
                if (text[p] != 'f' || text[p + 1] != 'a' || text[p + 2] != 'l' || text[p + 3] != 's' || text[p + 4] != 'e') {
                    throw new Exception("Expected 'false'");
                } else {
                    p += 5;
                    return false;
                }
            }
        }

        private string ReadString()
        {
            bool esc = false;
            StringBuilder builder = new StringBuilder();
            Expect('"');
            while (true) {
                char cur = ReadNext();
                if (esc) {
                    if (cur == '\\' || cur == '"') {
                        builder.Append(cur);
                    } else if (cur == 'n') {
                        builder.Append('\n');
                    } else {
                        throw new Exception("Found unescaped backslash");
                    }
                    esc = false;
                } else {
                    if(cur == '\\') {
                        esc = true;
                        continue;
                    } else if(cur == '"') {
                        return builder.ToString();
                    } else {
                        builder.Append(cur);
                    }
                }
            }
        }

        private char Expect(params char[] chars)
        {
            while(true) {
                char cur = ReadNext();
                if(Char.IsWhiteSpace(cur)) {
                    continue;
                }
                foreach(char el in chars) {
                    if(el == cur) {
                        return el;
                    }
                }
                throw new Exception($"Expected {CharArrayToString(chars)} but got {cur}");
            }
        }

        private void SkipWS()
        {
            while (true) {
                char cur = text[p];
                if (Char.IsWhiteSpace(cur)) {
                    p++;
                    continue;
                }
                return;
            }
        }

        private void SkipComma()
        {
            SkipWS();
            char chr = text[p];
            char comma = ',';
            if (chr == comma) {
                p++;
            }
        }

        private static string CharArrayToString(char[] chars)
        {
            string res = "";
            for(int i = 0; i < chars.Length; i++) {
                if(i == 0) {
                    res = "'" + chars[i].ToString() + "'";
                } else {
                    res = res + " or '" + chars[i].ToString() + "'";
                }
            }
            return res;
        }
    }
}
