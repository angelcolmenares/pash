namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;

    internal static class EnumMinimumDisambiguation
    {
        private static Dictionary<Type, string[]> specialDisambiguateCases = new Dictionary<Type, string[]>();

        static EnumMinimumDisambiguation()
        {
            specialDisambiguateCases.Add(typeof(FileAttributes), new string[] { "Directory", "ReadOnly", "System" });
        }

        internal static string EnumAllValues(Type enumType)
        {
            string[] enumNames = enumType.GetEnumNames();
            string str = ", ";
            StringBuilder builder = new StringBuilder();
            if (enumNames.Length != 0)
            {
                for (int i = 0; i < enumNames.Length; i++)
                {
                    builder.Append(enumNames[i]);
                    builder.Append(str);
                }
                builder.Remove(builder.Length - str.Length, str.Length);
            }
            return builder.ToString();
        }

        internal static string EnumDisambiguate(string text, Type enumType)
        {
            string[] strArray2;
            string[] enumNames = enumType.GetEnumNames();
            CompareInfo.GetCompareInfo(CultureInfo.InvariantCulture.LCID);
            List<string> list = new List<string>();
            foreach (string str in enumNames)
            {
                if (str.StartsWith(text, StringComparison.OrdinalIgnoreCase))
                {
                    list.Add(str);
                }
            }
            if (list.Count == 0)
            {
                throw InterpreterError.NewInterpreterException(null, typeof(RuntimeException), null, "NoEnumNameMatch", EnumExpressionEvaluatorStrings.NoEnumNameMatch, new object[] { text, EnumAllValues(enumType) });
            }
            if (list.Count == 1)
            {
                return list[0];
            }
            foreach (string str2 in list)
            {
                if (str2.Equals(text, StringComparison.OrdinalIgnoreCase))
                {
                    return str2;
                }
            }
            if (specialDisambiguateCases.TryGetValue(enumType, out strArray2))
            {
                foreach (string str3 in strArray2)
                {
                    if (str3.StartsWith(text, StringComparison.OrdinalIgnoreCase))
                    {
                        return str3;
                    }
                }
            }
            StringBuilder builder = new StringBuilder(list[0]);
            string str4 = ", ";
            for (int i = 1; i < list.Count; i++)
            {
                builder.Append(str4);
                builder.Append(list[i]);
            }
            throw InterpreterError.NewInterpreterException(null, typeof(RuntimeException), null, "MultipleEnumNameMatch", EnumExpressionEvaluatorStrings.MultipleEnumNameMatch, new object[] { text, builder.ToString() });
        }
    }
}

