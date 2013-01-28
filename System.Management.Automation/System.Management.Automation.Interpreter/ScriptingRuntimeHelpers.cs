namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Reflection;

    internal class ScriptingRuntimeHelpers
    {
        internal static readonly MethodInfo BooleanToObjectMethod = typeof(ScriptingRuntimeHelpers).GetMethod("BooleanToObject");
        internal static object False = false;
        internal static readonly MethodInfo Int32ToObjectMethod = typeof(ScriptingRuntimeHelpers).GetMethod("Int32ToObject");
        internal static object True = true;

        internal static object BooleanToObject(bool b)
        {
            if (!b)
            {
                return False;
            }
            return True;
        }

        internal static object GetPrimitiveDefaultValue(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.DBNull:
                    return null;

                case TypeCode.Boolean:
                    return False;

                case TypeCode.Char:
                    return '\0';

                case TypeCode.SByte:
                    return (sbyte) 0;

                case TypeCode.Byte:
                    return (byte) 0;

                case TypeCode.Int16:
                    return (short) 0;

                case TypeCode.UInt16:
                    return (ushort) 0;

                case TypeCode.Int32:
                    return Int32ToObject(0);

                case TypeCode.UInt32:
                    return 0;

                case TypeCode.Int64:
                    return 0L;

                case TypeCode.UInt64:
                    return (ulong) 0L;

                case TypeCode.Single:
                    return 0f;

                case TypeCode.Double:
                    return 0.0;

                case TypeCode.Decimal:
                    return 0M;

                case TypeCode.DateTime:
                    return new DateTime();
            }
            return null;
        }

        internal static object Int32ToObject(int i)
        {
            return i;
        }
    }
}

