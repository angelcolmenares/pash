namespace System.Management.Automation
{
    using Microsoft.Management.Infrastructure;
    using Microsoft.PowerShell;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Configuration;
    using System.Data;
    using System.DirectoryServices;
    using System.Globalization;
    using System.IO;
    using System.Linq.Expressions;
    using System.Management;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Language;
    using System.Management.Automation.Runspaces;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;

    public static class LanguagePrimitives
    {
        private static readonly CallSite<System.Func<CallSite, object, IEnumerator>> _getEnumeratorSite = CallSite<System.Func<CallSite, object, IEnumerator>>.Create(PSEnumerableBinder.Get());
        private const string ComparisonFailure = "ComparisonFailure";
        private static Dictionary<ConversionTypePair, ConversionData> converterCache;
        private static Dictionary<Type, GetEnumerableDelegate> getEnumerableCache = new Dictionary<Type, GetEnumerableDelegate>(0x20);
        private static Type[] IntegerTypes;
        internal static Type[][] LargestTypeTable;
        private const string NotIcomparable = "NotIcomparable";
        private static Type[] NumericTypes;
        internal const string OrderedAttribute = "ordered";
        private static Dictionary<string, bool> possibleTypeConverter;
        private static Type[] RealTypes;
        private static Type[] SignedIntegerTypes;
        internal static StringToTypeCache stringToTypeCache;
        [TraceSource("ETS", "Extended Type System")]
        private static PSTraceSource tracer = PSTraceSource.GetTracer("ETS", "Extended Type System");
        private static readonly TypeCodeTraits[] typeCodeTraits;
        internal static PSTraceSource typeConversion;
        private static Type[] UnsignedIntegerTypes;

        static LanguagePrimitives()
        {
            Type[][] typeArray = new Type[11][];
            typeArray[0] = new Type[] { typeof(short), typeof(int), typeof(long), typeof(int), typeof(long), typeof(double), typeof(short), typeof(short), typeof(float), typeof(double), typeof(decimal) };
            typeArray[1] = new Type[] { typeof(int), typeof(int), typeof(long), typeof(int), typeof(long), typeof(double), typeof(int), typeof(int), typeof(double), typeof(double), typeof(decimal) };
            typeArray[2] = new Type[] { typeof(long), typeof(long), typeof(long), typeof(long), typeof(long), typeof(decimal), typeof(long), typeof(long), typeof(double), typeof(double), typeof(decimal) };
            typeArray[3] = new Type[] { typeof(int), typeof(int), typeof(long), typeof(ushort), typeof(int), typeof(ulong), typeof(int), typeof(ushort), typeof(float), typeof(double), typeof(decimal) };
            typeArray[4] = new Type[] { typeof(long), typeof(long), typeof(long), typeof(int), typeof(int), typeof(ulong), typeof(long), typeof(int), typeof(double), typeof(double), typeof(decimal) };
            typeArray[5] = new Type[] { typeof(double), typeof(double), typeof(decimal), typeof(ulong), typeof(ulong), typeof(ulong), typeof(double), typeof(ulong), typeof(double), typeof(double), typeof(decimal) };
            typeArray[6] = new Type[] { typeof(short), typeof(int), typeof(long), typeof(int), typeof(long), typeof(double), typeof(sbyte), typeof(short), typeof(float), typeof(double), typeof(decimal) };
            typeArray[7] = new Type[] { typeof(short), typeof(int), typeof(long), typeof(ushort), typeof(int), typeof(ulong), typeof(short), typeof(byte), typeof(float), typeof(double), typeof(decimal) };
            Type[] typeArray10 = new Type[11];
            typeArray10[0] = typeof(float);
            typeArray10[1] = typeof(double);
            typeArray10[2] = typeof(double);
            typeArray10[3] = typeof(float);
            typeArray10[4] = typeof(double);
            typeArray10[5] = typeof(double);
            typeArray10[6] = typeof(float);
            typeArray10[7] = typeof(float);
            typeArray10[8] = typeof(float);
            typeArray10[9] = typeof(double);
            typeArray[8] = typeArray10;
            Type[] typeArray11 = new Type[11];
            typeArray11[0] = typeof(double);
            typeArray11[1] = typeof(double);
            typeArray11[2] = typeof(double);
            typeArray11[3] = typeof(double);
            typeArray11[4] = typeof(double);
            typeArray11[5] = typeof(double);
            typeArray11[6] = typeof(double);
            typeArray11[7] = typeof(double);
            typeArray11[8] = typeof(double);
            typeArray11[9] = typeof(double);
            typeArray[9] = typeArray11;
            Type[] typeArray12 = new Type[11];
            typeArray12[0] = typeof(decimal);
            typeArray12[1] = typeof(decimal);
            typeArray12[2] = typeof(decimal);
            typeArray12[3] = typeof(decimal);
            typeArray12[4] = typeof(decimal);
            typeArray12[5] = typeof(decimal);
            typeArray12[6] = typeof(decimal);
            typeArray12[7] = typeof(decimal);
            typeArray12[10] = typeof(decimal);
            typeArray[10] = typeArray12;
            LargestTypeTable = typeArray;
            TypeCodeTraits[] traitsArray = new TypeCodeTraits[0x13];
            traitsArray[3] = TypeCodeTraits.CimIntrinsicType;
            traitsArray[4] = TypeCodeTraits.CimIntrinsicType;
            traitsArray[5] = TypeCodeTraits.CimIntrinsicType | TypeCodeTraits.SignedInteger;
            traitsArray[6] = TypeCodeTraits.CimIntrinsicType | TypeCodeTraits.UnsignedInteger;
            traitsArray[7] = TypeCodeTraits.CimIntrinsicType | TypeCodeTraits.SignedInteger;
            traitsArray[8] = TypeCodeTraits.CimIntrinsicType | TypeCodeTraits.UnsignedInteger;
            traitsArray[9] = TypeCodeTraits.CimIntrinsicType | TypeCodeTraits.SignedInteger;
            traitsArray[10] = TypeCodeTraits.CimIntrinsicType | TypeCodeTraits.UnsignedInteger;
            traitsArray[11] = TypeCodeTraits.CimIntrinsicType | TypeCodeTraits.SignedInteger;
            traitsArray[12] = TypeCodeTraits.CimIntrinsicType | TypeCodeTraits.UnsignedInteger;
            traitsArray[13] = TypeCodeTraits.CimIntrinsicType | TypeCodeTraits.Floating;
            traitsArray[14] = TypeCodeTraits.CimIntrinsicType | TypeCodeTraits.Floating;
            traitsArray[15] = TypeCodeTraits.Decimal;
            traitsArray[0x10] = TypeCodeTraits.CimIntrinsicType;
            traitsArray[0x12] = TypeCodeTraits.CimIntrinsicType;
            typeCodeTraits = traitsArray;
            typeConversion = PSTraceSource.GetTracer("TypeConversion", "Traces the type conversion algorithm", false);
            stringToTypeCache = new StringToTypeCache();
            converterCache = new Dictionary<ConversionTypePair, ConversionData>(0x100);
            NumericTypes = new Type[] { typeof(short), typeof(int), typeof(long), typeof(ushort), typeof(int), typeof(ulong), typeof(sbyte), typeof(byte), typeof(float), typeof(double), typeof(decimal) };
            IntegerTypes = new Type[] { typeof(short), typeof(int), typeof(long), typeof(ushort), typeof(int), typeof(ulong), typeof(sbyte), typeof(byte) };
            SignedIntegerTypes = new Type[] { typeof(sbyte), typeof(short), typeof(int), typeof(long) };
            UnsignedIntegerTypes = new Type[] { typeof(byte), typeof(ushort), typeof(int), typeof(ulong) };
            RealTypes = new Type[] { typeof(float), typeof(double), typeof(decimal) };
            possibleTypeConverter = new Dictionary<string, bool>(0x10);
            ResetCaches(null);
            InitializeGetEnumerableCache();
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(LanguagePrimitives.AssemblyResolveHelper);
        }

        private static void AddItemsToCollection(object valueToConvert, Type resultType, IFormatProvider formatProvider, TypeTable backupTable, StringCollection stringCollection)
        {
            try
            {
                string[] strArray = (string[]) ConvertTo(valueToConvert, typeof(string[]), false, formatProvider, backupTable);
                stringCollection.AddRange(strArray);
            }
            catch (PSInvalidCastException)
            {
                typeConversion.WriteLine("valueToConvert contains non-string type values", new object[0]);
                ArgumentException innerException = new ArgumentException(System.Management.Automation.Internal.StringUtil.Format(ExtendedTypeSystem.CannotConvertValueToStringArray, valueToConvert.ToString()));
                throw new PSInvalidCastException(System.Management.Automation.Internal.StringUtil.Format("InvalidCastTo{0}Class", resultType.Name), innerException, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), innerException.Message });
            }
            catch (Exception exception2)
            {
                CommandProcessorBase.CheckForSevereException(exception2);
                typeConversion.WriteLine("Exception creating StringCollection class: \"{0}\".", new object[] { exception2.Message });
                throw new PSInvalidCastException(System.Management.Automation.Internal.StringUtil.Format("InvalidCastTo{0}Class", resultType.Name), exception2, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), exception2.Message });
            }
        }

        internal static PSObject AsPSObjectOrNull(object obj)
        {
            if (obj == null)
            {
                return null;
            }
            return PSObject.AsPSObject(obj);
        }

        private static Assembly AssemblyResolveHelper(object sender, ResolveEventArgs args)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName == args.Name)
                {
                    return assembly;
                }
            }
            return null;
        }

        private static ConversionData CacheConversion<T>(Type fromType, Type toType, PSConverter<T> converter, ConversionRank rank)
        {
            ConversionTypePair key = new ConversionTypePair(fromType, toType);
            ConversionData data = null;
            lock (converterCache)
            {
                if (!converterCache.TryGetValue(key, out data))
                {
                    data = new ConversionData<T>(converter, rank);
                    converterCache.Add(key, data);
                }
            }
            return data;
        }

        private static GetEnumerableDelegate CalculateGetEnumerable(Type objectType)
        {
            if (typeof(DataTable).IsAssignableFrom(objectType))
            {
                return new GetEnumerableDelegate(LanguagePrimitives.DataTableEnumerable);
            }
            if ((typeof(IEnumerable).IsAssignableFrom(objectType) && !typeof(IDictionary).IsAssignableFrom(objectType)) && !typeof(System.Xml.XmlNode).IsAssignableFrom(objectType))
            {
                return new GetEnumerableDelegate(LanguagePrimitives.TypicalEnumerable);
            }
            return new GetEnumerableDelegate(LanguagePrimitives.ReturnNullEnumerable);
        }

        public static int Compare(object first, object second)
        {
            return Compare(first, second, false, CultureInfo.InvariantCulture);
        }

        public static int Compare(object first, object second, bool ignoreCase)
        {
            return Compare(first, second, ignoreCase, CultureInfo.InvariantCulture);
        }

        public static int Compare(object first, object second, bool ignoreCase, IFormatProvider formatProvider)
        {
            first = PSObject.Base(first);
            second = PSObject.Base(second);
            if (first != null)
            {
                if (second != null)
                {
                    object obj2;
                    string strA = first as string;
                    if (strA != null)
                    {
                        string strB = second as string;
                        if (strB == null)
                        {
                            try
                            {
                                strB = (string) ConvertTo(second, typeof(string), formatProvider);
                            }
                            catch (PSInvalidCastException exception)
                            {
                                throw PSTraceSource.NewArgumentException("second", "ExtendedTypeSystem", "ComparisonFailure", new object[] { first.ToString(), second.ToString(), exception.Message });
                            }
                        }
                        return string.Compare(strA, strB, ignoreCase, formatProvider as CultureInfo);
                    }
                    Type type2 = first.GetType();
                    Type type = second.GetType();
                    int num = TypeTableIndex(type2);
                    int num2 = TypeTableIndex(type);
                    if ((num != -1) && (num2 != -1))
                    {
                        return NumericCompare(first, second, num, num2);
                    }
                    try
                    {
                        obj2 = ConvertTo(second, type2, formatProvider);
                    }
                    catch (PSInvalidCastException exception2)
                    {
                        throw PSTraceSource.NewArgumentException("second", "ExtendedTypeSystem", "ComparisonFailure", new object[] { first.ToString(), second.ToString(), exception2.Message });
                    }
                    IComparable comparable = first as IComparable;
                    if (comparable != null)
                    {
                        return comparable.CompareTo(obj2);
                    }
                    if (!first.Equals(second))
                    {
                        throw PSTraceSource.NewArgumentException("first", "ExtendedTypeSystem", "NotIcomparable", new object[] { first.ToString() });
                    }
                    return 0;
                }
                switch (GetTypeCode(first.GetType()))
                {
                    case TypeCode.SByte:
                        if (Math.Sign((sbyte) first) < 0)
                        {
                            return -1;
                        }
                        return 1;

                    case TypeCode.Int16:
                        if (Math.Sign((short) first) < 0)
                        {
                            return -1;
                        }
                        return 1;

                    case TypeCode.Int32:
                        if (Math.Sign((int) first) < 0)
                        {
                            return -1;
                        }
                        return 1;

                    case TypeCode.Int64:
                        if (Math.Sign((long) first) < 0)
                        {
                            return -1;
                        }
                        return 1;

                    case TypeCode.Single:
                        if (Math.Sign((float) first) < 0)
                        {
                            return -1;
                        }
                        return 1;

                    case TypeCode.Double:
                        if (Math.Sign((double) first) < 0)
                        {
                            return -1;
                        }
                        return 1;

                    case TypeCode.Decimal:
                        if (Math.Sign((decimal) first) < 0)
                        {
                            return -1;
                        }
                        return 1;
                }
                return 1;
            }
            if (second == null)
            {
                return 0;
            }
            switch (GetTypeCode(second.GetType()))
            {
                case TypeCode.SByte:
                    if (Math.Sign((sbyte) second) < 0)
                    {
                        return 1;
                    }
                    return -1;

                case TypeCode.Int16:
                    if (Math.Sign((short) second) < 0)
                    {
                        return 1;
                    }
                    return -1;

                case TypeCode.Int32:
                    if (Math.Sign((int) second) < 0)
                    {
                        return 1;
                    }
                    return -1;

                case TypeCode.Int64:
                    if (Math.Sign((long) second) < 0)
                    {
                        return 1;
                    }
                    return -1;

                case TypeCode.Single:
                    if (Math.Sign((float) second) < 0)
                    {
                        return 1;
                    }
                    return -1;

                case TypeCode.Double:
                    if (Math.Sign((double) second) < 0)
                    {
                        return 1;
                    }
                    return -1;

                case TypeCode.Decimal:
                    if (Math.Sign((decimal) second) < 0)
                    {
                        return 1;
                    }
                    return -1;
            }
            return -1;
        }

        private static object ConvertAssignableFrom(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            typeConversion.WriteLine("Result type is assignable from value to convert's type", new object[0]);
            return valueToConvert;
        }

        private static bool ConvertCharToBool(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            typeConversion.WriteLine("Converting char to boolean.", new object[0]);
            char ch = (char) valueToConvert;
            return (ch != '\0');
        }

        private static bool ConvertClassToBool(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            typeConversion.WriteLine("Converting ref to boolean.", new object[0]);
            return (valueToConvert != null);
        }

        private static object ConvertEnumerableToArray(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            object obj3;
            try
            {
                ArrayList list = new ArrayList();
                Type type = resultType.Equals(typeof(Array)) ? typeof(object) : resultType.GetElementType();
                typeConversion.WriteLine("Converting elements in the value to convert to the result's element type.", new object[0]);
                foreach (object obj2 in GetEnumerable(valueToConvert))
                {
                    list.Add(ConvertTo(obj2, type, false, formatProvider, backupTable));
                }
                obj3 = list.ToArray(type);
            }
            catch (Exception exception)
            {
                typeConversion.WriteLine("Element conversion exception: \"{0}\".", new object[] { exception.Message });
                CommandProcessorBase.CheckForSevereException(exception);
                throw new PSInvalidCastException("InvalidCastExceptionEnumerableToArray", exception, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), exception.Message });
            }
            return obj3;
        }

        private static object ConvertEnumerableToEnum(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            IEnumerator enumerator = GetEnumerator(valueToConvert);
            StringBuilder builder = new StringBuilder();
            bool flag = false;
            while (ParserOps.MoveNext(null, null, enumerator))
            {
                if (flag)
                {
                    builder.Append(',');
                }
                else
                {
                    flag = true;
                }
                string current = enumerator.Current as string;
                if (current == null)
                {
                    object obj2 = ConvertTo(enumerator.Current, resultType, recursion, formatProvider, backupTable);
                    if (obj2 == null)
                    {
                        throw new PSInvalidCastException("InvalidCastEnumStringNotFound", null, ExtendedTypeSystem.InvalidCastExceptionEnumerationNoValue, new object[] { enumerator.Current, resultType, EnumSingleTypeConverter.EnumValues(resultType) });
                    }
                    builder.Append(obj2.ToString());
                }
                builder.Append(current);
            }
            return ConvertStringToEnum(builder.ToString(), resultType, recursion, originalValueToConvert, formatProvider, backupTable);
        }

        private static object ConvertIConvertible(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            object obj3;
            try
            {
                object obj2 = Convert.ChangeType(valueToConvert, resultType, formatProvider);
                typeConversion.WriteLine("Conversion using IConvertible succeeded.", new object[0]);
                obj3 = obj2;
            }
            catch (Exception exception)
            {
                typeConversion.WriteLine("Exception converting with IConvertible: \"{0}\".", new object[] { exception.Message });
                CommandProcessorBase.CheckForSevereException(exception);
                throw new PSInvalidCastException("InvalidCastIConvertible", exception, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), exception.Message });
            }
            return obj3;
        }

        private static Hashtable ConvertIDictionaryToHashtable(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            typeConversion.WriteLine("Converting to Hashtable.", new object[0]);
            return new Hashtable(valueToConvert as IDictionary);
        }

        private static bool ConvertIListToBool(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            typeConversion.WriteLine("Converting IList to boolean.", new object[0]);
            return IsTrue((IList) valueToConvert);
        }

        private static object ConvertIntegerToEnum(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            object obj2;
            try
            {
                obj2 = Enum.ToObject(resultType, valueToConvert);
            }
            catch (Exception exception)
            {
                typeConversion.WriteLine("Integer to System.Enum exception: \"{0}\".", new object[] { exception.Message });
                CommandProcessorBase.CheckForSevereException(exception);
                throw new PSInvalidCastException("InvalidCastExceptionIntegerToEnum", exception, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), exception.Message });
            }
            EnumSingleTypeConverter.ThrowForUndefinedEnum("UndefinedIntegerToEnum", obj2, valueToConvert, resultType);
            return obj2;
        }

        private static object ConvertNoConversion(object valueToConvert, Type resultType, bool recurse, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            ThrowInvalidCastException(valueToConvert, resultType);
            return null;
        }

        private static string ConvertNonNumericToString(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            string str;
            ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
            try
            {
                typeConversion.WriteLine("Converting object to string.", new object[0]);
                str = PSObject.ToStringParser(executionContextFromTLS, valueToConvert);
            }
            catch (ExtendedTypeSystemException exception)
            {
                typeConversion.WriteLine("Converting object to string Exception: \"{0}\".", new object[] { exception.Message });
                throw new PSInvalidCastException("InvalidCastFromAnyTypeToString", exception, ExtendedTypeSystem.InvalidCastCannotRetrieveString, new object[0]);
            }
            return str;
        }

        private static object ConvertNotSupportedConversion(object valueToConvert, Type resultType, bool recurse, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            ThrowInvalidConversionException(valueToConvert, resultType);
            return null;
        }

        private static bool ConvertNullToBool(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            typeConversion.WriteLine("Converting null to boolean.", new object[0]);
            return false;
        }

        private static char ConvertNullToChar(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            typeConversion.WriteLine("Converting null to '0'.", new object[0]);
            return '\0';
        }

        private static object ConvertNullToNullable(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            return null;
        }

        private static object ConvertNullToNumeric(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            typeConversion.WriteLine("Converting null to zero.", new object[0]);
            return Convert.ChangeType(0, resultType, CultureInfo.InvariantCulture);
        }

        private static PSReference ConvertNullToPSReference(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            return new PSReference<Null>(null);
        }

        private static object ConvertNullToRef(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            return valueToConvert;
        }

        private static string ConvertNullToString(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            typeConversion.WriteLine("Converting null to \"\".", new object[0]);
            return string.Empty;
        }

        private static SwitchParameter ConvertNullToSwitch(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            typeConversion.WriteLine("Converting null to SwitchParameter(false).", new object[0]);
            return new SwitchParameter(false);
        }

        private static object ConvertNullToVoid(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            typeConversion.WriteLine("Converting null to AutomationNull.Value.", new object[0]);
            return AutomationNull.Value;
        }

        private static object ConvertNumeric(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            object obj3;
            try
            {
                object obj2 = Convert.ChangeType(valueToConvert, resultType, formatProvider);
                typeConversion.WriteLine("Numeric conversion succeeded.", new object[0]);
                obj3 = obj2;
            }
            catch (Exception exception)
            {
                typeConversion.WriteLine("Exception converting with IConvertible: \"{0}\".", new object[] { exception.Message });
                CommandProcessorBase.CheckForSevereException(exception);
                throw new PSInvalidCastException("InvalidCastIConvertible", exception, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), exception.Message });
            }
            return obj3;
        }

        private static object ConvertNumericChar(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            object obj3;
            try
            {
                object obj2 = Convert.ChangeType(Convert.ChangeType(valueToConvert, typeof(int), formatProvider), resultType, formatProvider);
                typeConversion.WriteLine("Numeric conversion succeeded.", new object[0]);
                obj3 = obj2;
            }
            catch (Exception exception)
            {
                typeConversion.WriteLine("Exception converting with IConvertible: \"{0}\".", new object[] { exception.Message });
                CommandProcessorBase.CheckForSevereException(exception);
                throw new PSInvalidCastException("InvalidCastIConvertible", exception, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), exception.Message });
            }
            return obj3;
        }

        private static object ConvertNumericIConvertible(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            if ((originalValueToConvert != null) && (originalValueToConvert.TokenText != null))
            {
                return ConvertTo(originalValueToConvert.TokenText, resultType, recursion, formatProvider, backupTable);
            }
            string str = (string) ConvertTo(valueToConvert, typeof(string), recursion, formatProvider, backupTable);
            return ConvertTo(str, resultType, recursion, formatProvider, backupTable);
        }

        private static object ConvertNumericThroughDouble(object valueToConvert, Type resultType)
        {
            using (typeConversion.TraceScope("Numeric Conversion through System.Double.", new object[0]))
            {
                return Convert.ChangeType(Convert.ChangeType(valueToConvert, typeof(double), CultureInfo.InvariantCulture.NumberFormat), resultType, CultureInfo.InvariantCulture.NumberFormat);
            }
        }

        private static bool ConvertNumericToBool(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            typeConversion.WriteLine("Converting number to boolean.", new object[0]);
            return !valueToConvert.Equals(Convert.ChangeType(0, valueToConvert.GetType(), CultureInfo.InvariantCulture));
        }

        private static string ConvertNumericToString(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            string str;
            if ((originalValueToConvert != null) && (originalValueToConvert.TokenText != null))
            {
                return originalValueToConvert.TokenText;
            }
            typeConversion.WriteLine("Converting numeric to string.", new object[0]);
            try
            {
                str = (string) Convert.ChangeType(valueToConvert, resultType, CultureInfo.InvariantCulture.NumberFormat);
            }
            catch (Exception exception)
            {
                typeConversion.WriteLine("Converting numeric to string Exception: \"{0}\".", new object[] { exception.Message });
                CommandProcessorBase.CheckForSevereException(exception);
                throw new PSInvalidCastException("InvalidCastFromNumericToString", exception, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), exception.Message });
            }
            return str;
        }

        private static object ConvertRelatedArrays(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            typeConversion.WriteLine("The element type of result is assignable from the element type of the value to convert", new object[0]);
            Array array = (Array) valueToConvert;
            Array array2 = Array.CreateInstance(resultType.GetElementType(), array.Length);
            array.CopyTo(array2, 0);
            return array2;
        }

        private static object ConvertScalarToArray(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            object obj2;
            typeConversion.WriteLine("Value to convert is scalar.", new object[0]);
            if ((originalValueToConvert != null) && (originalValueToConvert.TokenText != null))
            {
                valueToConvert = originalValueToConvert;
            }
            try
            {
                Type type = resultType.Equals(typeof(Array)) ? typeof(object) : resultType.GetElementType();
                ArrayList list = new ArrayList();
                list.Add(ConvertTo(valueToConvert, type, false, formatProvider, backupTable));
                obj2 = list.ToArray(type);
            }
            catch (Exception exception)
            {
                typeConversion.WriteLine("Element conversion exception: \"{0}\".", new object[] { exception.Message });
                CommandProcessorBase.CheckForSevereException(exception);
                throw new PSInvalidCastException("InvalidCastExceptionScalarToArray", exception, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), exception.Message });
            }
            return obj2;
        }

        private static Delegate ConvertScriptBlockToDelegate(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            Exception innerException = null;
            try
            {
                return ((ScriptBlock) valueToConvert).GetDelegate(resultType);
            }
            catch (ArgumentNullException exception2)
            {
                innerException = exception2;
            }
            catch (ArgumentException exception3)
            {
                innerException = exception3;
            }
            catch (MissingMethodException exception4)
            {
                innerException = exception4;
            }
            catch (MemberAccessException exception5)
            {
                innerException = exception5;
            }
            typeConversion.WriteLine("Converting script block to delegate Exception: \"{0}\".", new object[] { innerException.Message });
            throw new PSInvalidCastException("InvalidCastFromScriptBlockToDelegate", innerException, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), innerException.Message });
        }

        internal static Type ConvertStringToAttribute(string typeName)
        {
            Exception exception;
            Type type = ConvertStringToType(typeName, out exception);
            if ((type != null) && type.IsSubclassOf(typeof(System.Attribute)))
            {
                return type;
            }
            return (ConvertStringToType(typeName + "Attribute", out exception) ?? type);
        }

        private static bool ConvertStringToBool(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            typeConversion.WriteLine("Converting string to boolean.", new object[0]);
            return IsTrue((string) valueToConvert);
        }

        private static char[] ConvertStringToCharArray(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            typeConversion.WriteLine("Returning value to convert's ToCharArray().", new object[0]);
            return ((string) valueToConvert).ToCharArray();
        }

        private static CimSession ConvertStringToCimSession(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            CimSession session;
            typeConversion.WriteLine("Returning CimSession.Create(value to convert).", new object[0]);
            try
            {
                session = CimSession.Create((string) valueToConvert);
            }
            catch (CimException exception)
            {
                typeConversion.WriteLine("Exception in CimSession.Create: \"{0}\".", new object[] { exception.Message });
                throw new PSInvalidCastException("InvalidCastFromStringToCimSession", exception, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), exception.Message });
            }
            return session;
        }

        private static object ConvertStringToDecimal(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            object obj2;
            if (((string) valueToConvert).Length == 0)
            {
                typeConversion.WriteLine("Returning numeric zero.", new object[0]);
                return Convert.ChangeType(0, resultType, CultureInfo.InvariantCulture);
            }
            typeConversion.WriteLine("Converting to decimal.", new object[0]);
            try
            {
                obj2 = Convert.ChangeType(valueToConvert, resultType, CultureInfo.InvariantCulture.NumberFormat);
            }
            catch (Exception exception)
            {
                typeConversion.WriteLine("Exception converting to decimal: \"{0}\". Converting to decimal passing through double.", new object[] { exception.Message });
                CommandProcessorBase.CheckForSevereException(exception);
                if (exception is FormatException)
                {
                    try
                    {
                        return ConvertNumericThroughDouble(valueToConvert, resultType);
                    }
                    catch (Exception exception2)
                    {
                        typeConversion.WriteLine("Exception converting to integer through double: \"{0}\".", new object[] { exception2.Message });
                        CommandProcessorBase.CheckForSevereException(exception2);
                    }
                }
                throw new PSInvalidCastException("InvalidCastFromStringToDecimal", exception, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), exception.Message });
            }
            return obj2;
        }

        private static object ConvertStringToEnum(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            string str = valueToConvert as string;
            object enumValue = null;
            typeConversion.WriteLine("Calling case sensitive Enum.Parse", new object[0]);
            try
            {
                enumValue = Enum.Parse(resultType, str);
            }
            catch (ArgumentException exception)
            {
                typeConversion.WriteLine("Enum.Parse Exception: \"{0}\".", new object[] { exception.Message });
                try
                {
                    typeConversion.WriteLine("Calling case insensitive Enum.Parse", new object[0]);
                    enumValue = Enum.Parse(resultType, str, true);
                }
                catch (ArgumentException exception2)
                {
                    typeConversion.WriteLine("Enum.Parse Exception: \"{0}\".", new object[] { exception2.Message });
                }
                catch (Exception exception3)
                {
                    CommandProcessorBase.CheckForSevereException(exception3);
                    typeConversion.WriteLine("Case insensitive Enum.Parse threw an exception.", new object[0]);
                    throw new PSInvalidCastException("CaseInsensitiveEnumParseThrewAnException", exception3, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), exception3.Message });
                }
            }
            catch (Exception exception4)
            {
                CommandProcessorBase.CheckForSevereException(exception4);
                typeConversion.WriteLine("Case Sensitive Enum.Parse threw an exception.", new object[0]);
                throw new PSInvalidCastException("CaseSensitiveEnumParseThrewAnException", exception4, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), exception4.Message });
            }
            if (enumValue == null)
            {
                typeConversion.WriteLine("Calling substring disambiguation.", new object[0]);
                try
                {
                    string str2 = EnumMinimumDisambiguation.EnumDisambiguate(str, resultType);
                    enumValue = Enum.Parse(resultType, str2);
                }
                catch (Exception exception5)
                {
                    CommandProcessorBase.CheckForSevereException(exception5);
                    typeConversion.WriteLine("Substring disambiguation threw an exception.", new object[0]);
                    throw new PSInvalidCastException("SubstringDisambiguationEnumParseThrewAnException", exception5, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), exception5.Message });
                }
            }
            EnumSingleTypeConverter.ThrowForUndefinedEnum("EnumParseUndefined", enumValue, valueToConvert, resultType);
            tracer.WriteLine("returning \"{0}\" from conversion to Enum.", new object[] { enumValue });
            return enumValue;
        }

        private static object ConvertStringToInteger(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            object obj2;
            if (((string) valueToConvert).Length == 0)
            {
                typeConversion.WriteLine("Returning numeric zero.", new object[0]);
                return Convert.ChangeType(0, resultType, CultureInfo.InvariantCulture);
            }
            typeConversion.WriteLine("Converting to integer.", new object[0]);
            TypeConverter integerSystemConverter = GetIntegerSystemConverter(resultType);
            try
            {
                obj2 = integerSystemConverter.ConvertFrom(valueToConvert);
            }
            catch (Exception innerException)
            {
                CommandProcessorBase.CheckForSevereException(innerException);
                if (innerException.InnerException != null)
                {
                    innerException = innerException.InnerException;
                }
                typeConversion.WriteLine("Exception converting to integer: \"{0}\".", new object[] { innerException.Message });
                CommandProcessorBase.CheckForSevereException(innerException);
                if (innerException is FormatException)
                {
                    typeConversion.WriteLine("Converting to integer passing through double.", new object[0]);
                    try
                    {
                        return ConvertNumericThroughDouble(valueToConvert, resultType);
                    }
                    catch (Exception exception2)
                    {
                        typeConversion.WriteLine("Exception converting to integer through double: \"{0}\".", new object[] { exception2.Message });
                        CommandProcessorBase.CheckForSevereException(exception2);
                    }
                }
                throw new PSInvalidCastException("InvalidCastFromStringToInteger", innerException, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), innerException.Message });
            }
            return obj2;
        }

        private static object ConvertStringToReal(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            object obj2;
            if (((string) valueToConvert).Length == 0)
            {
                typeConversion.WriteLine("Returning numeric zero.", new object[0]);
                return Convert.ChangeType(0, resultType, CultureInfo.InvariantCulture);
            }
            typeConversion.WriteLine("Converting to double or single.", new object[0]);
            try
            {
                obj2 = Convert.ChangeType(valueToConvert, resultType, CultureInfo.InvariantCulture.NumberFormat);
            }
            catch (Exception exception)
            {
                typeConversion.WriteLine("Exception converting to double or single: \"{0}\".", new object[] { exception.Message });
                CommandProcessorBase.CheckForSevereException(exception);
                throw new PSInvalidCastException("InvalidCastFromStringToDoubleOrSingle", exception, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), exception.Message });
            }
            return obj2;
        }

        private static Regex ConvertStringToRegex(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            Regex regex;
            typeConversion.WriteLine("Returning new RegEx(value to convert).", new object[0]);
            try
            {
                regex = new Regex((string) valueToConvert);
            }
            catch (Exception exception)
            {
                typeConversion.WriteLine("Exception in RegEx constructor: \"{0}\".", new object[] { exception.Message });
                CommandProcessorBase.CheckForSevereException(exception);
                throw new PSInvalidCastException("InvalidCastFromStringToRegex", exception, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), exception.Message });
            }
            return regex;
        }

        internal static Type ConvertStringToType(string typeName, out Exception exception)
        {
			using (typeConversion.TraceScope("Conversion to System.Type", new object[0]))
            {
                exception = null;
                if (string.IsNullOrWhiteSpace(typeName))
                {
                    return null;
                }
				if (typeName == "System.Diagnostics.DebuggerHidden") typeName += "Attribute";
                Type reflectionType = stringToTypeCache.Get(typeName);
                if (reflectionType == null)
                {
                    ITypeName name = Parser.ScanType(typeName, false);
                    if (name == null)
                    {
                        return null;
                    }
                    if (!(name is TypeName))
                    {
                        try
                        {
                            reflectionType = name.GetReflectionType();
                            stringToTypeCache.Add(typeName, reflectionType);
                            return reflectionType;
                        }
                        catch (Exception exception2)
                        {
                            CommandProcessorBase.CheckForSevereException(exception2);
                            typeConversion.WriteLine("System.Type's GetType threw an exception for \"{0}\": \"{1}\".", new object[] { typeName, exception2.Message });
                            exception = exception2;
                            return null;
                        }
                    }
                    if (name.AssemblyName == null)
                    {
                        ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
                        if (executionContextFromTLS != null)
                        {
                            reflectionType = LookForTypeInAssemblies(typeName, executionContextFromTLS.AssemblyCache.Values);
                            if (reflectionType != null)
                            {
                                typeConversion.WriteLine("Found \"{0}\" in the make kit assemblies.", new object[] { reflectionType });
                            }
                        }
                        if (reflectionType == null)
                        {
                            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                            reflectionType = LookForTypeInAssemblies(typeName, assemblies);
                            if (reflectionType != null)
                            {
                                typeConversion.WriteLine("Found \"{0}\" in the loaded assemblies.", new object[] { reflectionType });
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            Type type = Type.GetType(typeName, false, true);
                            if (type != null)
                            {
                                if (IsPublic(type))
                                {
                                    reflectionType = type;
                                    typeConversion.WriteLine("Found \"{0}\" via Type.GetType(typeName).", new object[] { reflectionType });
                                }
                                else
                                {
                                    typeConversion.WriteLine("\"{0}\" is not public, so it will not be returned.", new object[] { type });
                                }
                            }
                        }
                        catch (Exception exception3)
                        {
                            CommandProcessorBase.CheckForSevereException(exception3);
                            typeConversion.WriteLine("System.Type's GetType threw an exception for \"{0}\": \"{1}\".", new object[] { typeName, exception3.Message });
                            exception = exception3;
                        }
                    }
                    if ((reflectionType == null) && !typeName.StartsWith("System.", StringComparison.OrdinalIgnoreCase))
                    {
                        reflectionType = ConvertStringToType("System." + typeName, out exception);
                        if (reflectionType != null)
                        {
							typeConversion.WriteLine("Found \"{0}\" after prepending 'System.' prefix", new object[] { reflectionType });
                        }
                    }
                    if (reflectionType == null)
                    {
                        typeConversion.WriteLine("Could not find a match for \"{0}\".", new object[] { typeName });
                    }
                    if (reflectionType != null)
                    {
                        stringToTypeCache.Add(typeName, reflectionType);
                    }
                }
                return reflectionType;
            }
        }

        private static Type ConvertStringToType(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            Exception exception;
            Type type = ConvertStringToType((string) valueToConvert, out exception);
            if (type == null)
            {
                throw new PSInvalidCastException("InvalidCastFromStringToType", exception, ExtendedTypeSystem.InvalidCastException, new object[] { valueToConvert.ToString(), ObjectToTypeNameString(valueToConvert), resultType.ToString() });
            }
            return type;
        }

        private static bool ConvertSwitchParameterToBool(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            typeConversion.WriteLine("Converting SwitchParameter to boolean.", new object[0]);
            SwitchParameter parameter = (SwitchParameter) valueToConvert;
            return parameter.ToBool();
        }

        public static T ConvertTo<T>(object valueToConvert)
        {
            return (T) ConvertTo(valueToConvert, typeof(T), true, CultureInfo.InvariantCulture, null);
        }

        public static object ConvertTo(object valueToConvert, Type resultType)
        {
            return ConvertTo(valueToConvert, resultType, true, CultureInfo.InvariantCulture, null);
        }

        public static object ConvertTo(object valueToConvert, Type resultType, IFormatProvider formatProvider)
        {
            return ConvertTo(valueToConvert, resultType, true, formatProvider, null);
        }

        internal static object ConvertTo(object valueToConvert, Type resultType, bool recursion, IFormatProvider formatProvider, TypeTable backupTypeTable)
        {
            using (typeConversion.TraceScope("Converting \"{0}\" to \"{1}\".", new object[] { valueToConvert, resultType }))
            {
                bool flag;
                if (resultType == null)
                {
                    throw PSTraceSource.NewArgumentNullException("resultType");
                }
                return FigureConversion(valueToConvert, resultType, out flag).Invoke(flag ? PSObject.Base(valueToConvert) : valueToConvert, resultType, recursion, flag ? ((PSObject) valueToConvert) : null, formatProvider, backupTypeTable);
            }
        }

        private static DirectoryEntry ConvertToADSI(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            string str;
            DirectoryEntry entry2;
            typeConversion.WriteLine("Standard type conversion to  DirectoryEntry.", new object[0]);
            try
            {
                str = PSObject.ToString(null, valueToConvert, "\n", null, null, true, true);
            }
            catch (ExtendedTypeSystemException exception)
            {
                typeConversion.WriteLine("Exception converting value to string: {0}", new object[] { exception.Message });
                throw new PSInvalidCastException("InvalidCastGetStringToADSIClass", exception, ExtendedTypeSystem.InvalidCastExceptionNoStringForConversion, new object[] { resultType.ToString(), exception.Message });
            }
            try
            {
                entry2 = new DirectoryEntry(str);
            }
            catch (Exception exception2)
            {
                CommandProcessorBase.CheckForSevereException(exception2);
                typeConversion.WriteLine("Exception creating ADSI class: \"{0}\".", new object[] { exception2.Message });
                throw new PSInvalidCastException("InvalidCastToADSIClass", exception2, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), exception2.Message });
            }
            return entry2;
        }

        private static CommaDelimitedStringCollection ConvertToCommaDelimitedStringCollection(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            typeConversion.WriteLine("Standard type conversion to a CommaDelimitedStringCollection.", new object[0]);
            CommaDelimitedStringCollection stringCollection = new CommaDelimitedStringCollection();
            AddItemsToCollection(valueToConvert, resultType, formatProvider, backupTable, stringCollection);
            return stringCollection;
        }

        private static object ConvertToNullable(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            return ConvertTo(valueToConvert, Nullable.GetUnderlyingType(resultType), recursion, formatProvider, backupTable);
        }

        private static PSObject ConvertToPSObject(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            typeConversion.WriteLine("Returning PSObject.AsPSObject(valueToConvert).", new object[0]);
            return PSObject.AsPSObject(valueToConvert);
        }

        private static PSReference ConvertToPSReference(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            typeConversion.WriteLine("Converting to PSReference.", new object[0]);
            return PSReference.CreateInstance(valueToConvert, valueToConvert.GetType());
        }

        private static StringCollection ConvertToStringCollection(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            typeConversion.WriteLine("Standard type conversion to a StringCollection.", new object[0]);
            StringCollection stringCollection = new StringCollection();
            AddItemsToCollection(valueToConvert, resultType, formatProvider, backupTable, stringCollection);
            return stringCollection;
        }

        private static object ConvertToVoid(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            typeConversion.WriteLine("returning AutomationNull.Value.", new object[0]);
            return AutomationNull.Value;
        }

        private static ManagementObject ConvertToWMI(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            string str;
            ManagementObject obj3;
            typeConversion.WriteLine("Standard type conversion to a ManagementObject.", new object[0]);
            try
            {
                str = PSObject.ToString(null, valueToConvert, "\n", null, null, true, true);
            }
            catch (ExtendedTypeSystemException exception)
            {
                typeConversion.WriteLine("Exception converting value to string: {0}", new object[] { exception.Message });
                throw new PSInvalidCastException("InvalidCastGetStringToWMI", exception, ExtendedTypeSystem.InvalidCastExceptionNoStringForConversion, new object[] { resultType.ToString(), exception.Message });
            }
            try
            {
                ManagementObject obj2 = new ManagementObject(str);
                if (obj2.SystemProperties["__CLASS"] == null)
                {
                    throw new PSInvalidCastException(System.Management.Automation.Internal.StringUtil.Format(ExtendedTypeSystem.InvalidWMIPath, str));
                }
                obj3 = obj2;
            }
            catch (Exception exception2)
            {
                CommandProcessorBase.CheckForSevereException(exception2);
                typeConversion.WriteLine("Exception creating WMI object: \"{0}\".", new object[] { exception2.Message });
                throw new PSInvalidCastException("InvalidCastToWMI", exception2, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), exception2.Message });
            }
            return obj3;
        }

        private static ManagementClass ConvertToWMIClass(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            string str;
            ManagementClass class3;
            typeConversion.WriteLine("Standard type conversion to a ManagementClass.", new object[0]);
            try
            {
                str = PSObject.ToString(null, valueToConvert, "\n", null, null, true, true);
            }
            catch (ExtendedTypeSystemException exception)
            {
                typeConversion.WriteLine("Exception converting value to string: {0}", new object[] { exception.Message });
                throw new PSInvalidCastException("InvalidCastGetStringToWMIClass", exception, ExtendedTypeSystem.InvalidCastExceptionNoStringForConversion, new object[] { resultType.ToString(), exception.Message });
            }
            try
            {
                ManagementClass class2 = new ManagementClass(str);
                if (class2.SystemProperties["__CLASS"] == null)
                {
                    throw new PSInvalidCastException(System.Management.Automation.Internal.StringUtil.Format(ExtendedTypeSystem.InvalidWMIClassPath, str));
                }
                class3 = class2;
            }
            catch (Exception exception2)
            {
                CommandProcessorBase.CheckForSevereException(exception2);
                typeConversion.WriteLine("Exception creating WMI class: \"{0}\".", new object[] { exception2.Message });
                throw new PSInvalidCastException("InvalidCastToWMIClass", exception2, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), exception2.Message });
            }
            return class3;
        }

        private static ManagementObjectSearcher ConvertToWMISearcher(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            string str;
            ManagementObjectSearcher searcher2;
            typeConversion.WriteLine("Standard type conversion to a collection of ManagementObjects.", new object[0]);
            try
            {
                str = PSObject.ToString(null, valueToConvert, "\n", null, null, true, true);
            }
            catch (ExtendedTypeSystemException exception)
            {
                typeConversion.WriteLine("Exception converting value to string: {0}", new object[] { exception.Message });
                throw new PSInvalidCastException("InvalidCastGetStringToWMISearcher", exception, ExtendedTypeSystem.InvalidCastExceptionNoStringForConversion, new object[] { resultType.ToString(), exception.Message });
            }
            try
            {
                searcher2 = new ManagementObjectSearcher(str);
            }
            catch (Exception exception2)
            {
                CommandProcessorBase.CheckForSevereException(exception2);
                typeConversion.WriteLine("Exception running WMI object query: \"{0}\".", new object[] { exception2.Message });
                throw new PSInvalidCastException("InvalidCastToWMISearcher", exception2, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), exception2.Message });
            }
            return searcher2;
        }

        private static XmlDocument ConvertToXml(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            XmlDocument document2;
            using (typeConversion.TraceScope("Standard type conversion to XmlDocument.", new object[0]))
            {
                string str;
                try
                {
                    str = PSObject.ToString(null, valueToConvert, "\n", null, null, true, true);
                }
                catch (ExtendedTypeSystemException exception)
                {
                    typeConversion.WriteLine("Exception converting value to string: {0}", new object[] { exception.Message });
                    throw new PSInvalidCastException("InvalidCastGetStringToXmlDocument", exception, ExtendedTypeSystem.InvalidCastExceptionNoStringForConversion, new object[] { resultType.ToString(), exception.Message });
                }
                try
                {
                    using (TextReader reader = new StringReader(str))
                    {
                        XmlReaderSettings settings = InternalDeserializer.XmlReaderSettingsForUntrustedXmlDocument.Clone();
                        settings.IgnoreWhitespace = true;
                        settings.IgnoreProcessingInstructions = false;
                        settings.IgnoreComments = false;
                        XmlReader reader2 = XmlReader.Create(reader, settings);
                        XmlDocument document = new XmlDocument {
                            PreserveWhitespace = false
                        };
                        document.Load(reader2);
                        return document;
                    }
                }
                catch (Exception exception2)
                {
                    typeConversion.WriteLine("Exception loading XML: \"{0}\".", new object[] { exception2.Message });
                    CommandProcessorBase.CheckForSevereException(exception2);
                    throw new PSInvalidCastException("InvalidCastToXmlDocument", exception2, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), exception2.Message });
                }
            }
            return document2;
        }

        private static object ConvertUnrelatedArrays(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            Array array = valueToConvert as Array;
            Type elementType = resultType.GetElementType();
            Array array2 = Array.CreateInstance(elementType, array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                object obj2 = ConvertTo(array.GetValue(i), elementType, false, formatProvider, backupTable);
                array2.SetValue(obj2, i);
            }
            return array2;
        }

        private static bool ConvertValueToBool(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
        {
            typeConversion.WriteLine("Converting value to boolean.", new object[0]);
            return true;
        }

        private static Func<T1, T2> CreateCtorLambdaClosure<T1, T2>(ConstructorInfo ctor, Type realParamType, bool useExplicitConversion)
        {
            ParameterExpression expression = null;
            Expression expr = useExplicitConversion ? ((Expression) Expression.Call(CachedReflectionInfo.Convert_ChangeType, expression, Expression.Constant(realParamType))) : ((Expression) Expression.Convert(expression = Expression.Parameter(typeof(T1), "args"), realParamType));
            return Expression.Lambda<Func<T1, T2>>(Expression.New(ctor, new Expression[] { expr.Cast(realParamType) }).Cast(typeof(T2)), new ParameterExpression[] { expression }).Compile();
        }

        internal static void CreateMemberNotFoundError(PSObject pso, DictionaryEntry property, Type resultType)
        {
            string availableProperties = GetAvailableProperties(pso);
            string message = System.Management.Automation.Internal.StringUtil.Format(ExtendedTypeSystem.PropertyNotFound, new object[] { property.Key.ToString(), resultType.FullName, availableProperties });
            typeConversion.WriteLine("Issuing an error message about not being able to create an object from hashtable.", new object[0]);
            throw new InvalidOperationException(message);
        }

        internal static void CreateMemberSetValueError(SetValueException e)
        {
            typeConversion.WriteLine("Issuing an error message about not being able to set the properties for an object.", new object[0]);
            throw e;
        }

        private static IEnumerable DataTableEnumerable(object obj)
        {
            return ((DataTable) obj).Rows;
        }

        internal static void DoConversionsForSetInGenericDictionary(IDictionary dictionary, ref object key, ref object value)
        {
            foreach (Type type in dictionary.GetType().GetInterfaces())
            {
                if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
                {
                    Type[] genericArguments = type.GetGenericArguments();
                    key = ConvertTo(key, genericArguments[0], CultureInfo.InvariantCulture);
                    value = ConvertTo(value, genericArguments[1], CultureInfo.InvariantCulture);
                }
            }
        }

        public static bool Equals(object first, object second)
        {
            return Equals(first, second, false, CultureInfo.InvariantCulture);
        }

        public static bool Equals(object first, object second, bool ignoreCase)
        {
            return Equals(first, second, ignoreCase, CultureInfo.InvariantCulture);
        }

        public static bool Equals(object first, object second, bool ignoreCase, IFormatProvider formatProvider)
        {
            if (formatProvider == null)
            {
                formatProvider = CultureInfo.InvariantCulture;
            }
            first = PSObject.Base(first);
            second = PSObject.Base(second);
            if (first == null)
            {
                return (second == null);
            }
            if (second != null)
            {
                string str2;
                string strA = first as string;
                if (strA != null)
                {
                    str2 = second as string;
                    if (str2 == null)
                    {
                        str2 = (string) ConvertTo(second, typeof(string), formatProvider);
                    }
                    return (string.Compare(strA, str2, ignoreCase, formatProvider as CultureInfo) == 0);
                }
                if (first.Equals(second))
                {
                    return true;
                }
                Type type = first.GetType();
                Type type2 = second.GetType();
                int num = TypeTableIndex(type);
                int num2 = TypeTableIndex(type2);
                if ((num != -1) && (num2 != -1))
                {
                    return (NumericCompare(first, second, num, num2) == 0);
                }
                if (type.Equals(typeof(char)) && ignoreCase)
                {
                    str2 = second as string;
                    if ((str2 != null) && (str2.Length == 1))
                    {
                        char ch = char.ToUpper((char) first, formatProvider as CultureInfo);
                        char ch2 = char.ToUpper(str2[0], formatProvider as CultureInfo);
                        return ch.Equals(ch2);
                    }
                    if (type2.Equals(typeof(char)))
                    {
                        char ch3 = char.ToUpper((char) first, formatProvider as CultureInfo);
                        char ch4 = char.ToUpper((char) second, formatProvider as CultureInfo);
                        return ch3.Equals(ch4);
                    }
                }
                try
                {
                    object obj2 = ConvertTo(second, type, formatProvider);
                    return first.Equals(obj2);
                }
                catch (InvalidCastException)
                {
                }
            }
            return false;
        }

        internal static PSConverter<object> FigureCastConversion(Type fromType, Type toType, ref ConversionRank rank)
        {
            MethodInfo info = FindCastOperator("op_Implicit", toType, fromType, toType);
            if (info == null)
            {
                info = FindCastOperator("op_Explicit", toType, fromType, toType);
                if (info == null)
                {
                    info = FindCastOperator("op_Implicit", fromType, fromType, toType);
                    if (info == null)
                    {
                        info = FindCastOperator("op_Explicit", fromType, fromType, toType);
                    }
                }
            }
            if (info != null)
            {
                rank = info.Name.Equals("op_Implicit", StringComparison.OrdinalIgnoreCase) ? ConversionRank.ImplicitCast : ConversionRank.ExplicitCast;
                ConvertViaCast cast = new ConvertViaCast {
                    cast = info
                };
                return new PSConverter<object>(cast.Convert);
            }
            return null;
        }

        internal static PSConverter<object> FigureConstructorConversion(Type fromType, Type toType)
        {
            if (IsIntegralType(fromType) && (typeof(IList).IsAssignableFrom(toType) || typeof(ICollection).IsAssignableFrom(toType)))
            {
                typeConversion.WriteLine("Ignoring the collection constructor that takes an integer, since this is not semantically a conversion.", new object[0]);
                return null;
            }
            ConstructorInfo ctor = null;
            try
            {
                ctor = toType.GetConstructor(new Type[] { fromType });
            }
            catch (AmbiguousMatchException exception)
            {
                typeConversion.WriteLine("Exception finding Constructor: \"{0}\".", new object[] { exception.Message });
            }
            catch (ArgumentException exception2)
            {
                typeConversion.WriteLine("Exception finding Constructor: \"{0}\".", new object[] { exception2.Message });
            }
            if (ctor == null)
            {
                return null;
            }
            typeConversion.WriteLine("Found Constructor.", new object[0]);
            ConvertViaConstructor constructor = new ConvertViaConstructor();
            try
            {
                Type parameterType = ctor.GetParameters()[0].ParameterType;
                bool useExplicitConversion = false;
                if ((parameterType.IsValueType && !fromType.Equals(parameterType)) && (Nullable.GetUnderlyingType(parameterType) == null))
                {
                    useExplicitConversion = true;
                }
                constructor.TargetCtorLambda = CreateCtorLambdaClosure<object, object>(ctor, parameterType, useExplicitConversion);
            }
            catch (Exception exception3)
            {
                CommandProcessorBase.CheckForSevereException(exception3);
                typeConversion.WriteLine("Exception building constructor lambda: \"{0}\"", new object[] { exception3.Message });
                return null;
            }
            typeConversion.WriteLine("Conversion is figured out.", new object[0]);
            return new PSConverter<object>(constructor.Convert);
        }

        internal static ConversionData FigureConversion(Type fromType, Type toType)
        {
            ConversionData conversionData = GetConversionData(fromType, toType);
            if (conversionData != null)
            {
                return conversionData;
            }
            if (fromType.Equals(typeof(Null)))
            {
                return FigureConversionFromNull(toType);
            }
            if (toType.IsAssignableFrom(fromType))
            {
                return CacheConversion<object>(fromType, toType, new PSConverter<object>(LanguagePrimitives.ConvertAssignableFrom), toType.Equals(fromType) ? ConversionRank.Identity : ConversionRank.Assignable);
            }
            if (typeof(PSObject).IsAssignableFrom(fromType) && !typeof(InternalPSObject).Equals(fromType))
            {
                return CacheConversion<object>(fromType, toType, new PSConverter<object>(LanguagePrimitives.ConvertNoConversion), ConversionRank.None);
            }
            if (toType.Equals(typeof(PSObject)))
            {
                return CacheConversion<PSObject>(fromType, toType, new PSConverter<PSObject>(LanguagePrimitives.ConvertToPSObject), ConversionRank.PSObject);
            }
            PSConverter<object> converter = null;
            ConversionRank none = ConversionRank.None;
            if (ExecutionContext.HasEverUsedConstrainedLanguage && fromType != typeof(Hashtable)) //HACK skip hash table
            {
                ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
                if ((((executionContextFromTLS != null) && (executionContextFromTLS.LanguageMode == PSLanguageMode.ConstrainedLanguage)) && ((toType != typeof(object)) && (toType != typeof(object[])))) && !CoreTypes.Contains(toType))
                {
                    converter = new PSConverter<object>(LanguagePrimitives.ConvertNotSupportedConversion);
                    none = ConversionRank.None;
                    return CacheConversion<object>(fromType, toType, converter, none);
                }
            }
            PSConverter<object> valueDependentConversion = null;
            ConversionRank valueDependentRank = ConversionRank.None;
            ConversionData data2 = FigureLanguageConversion(fromType, toType, out valueDependentConversion, out valueDependentRank);
            if (data2 != null)
            {
                return data2;
            }
            none = (valueDependentConversion != null) ? ConversionRank.Language : ConversionRank.None;
            converter = FigureParseConversion(fromType, toType);
            if (converter == null)
            {
                converter = FigureStaticCreateMethodConversion(fromType, toType);
                if (converter == null)
                {
                    converter = FigureConstructorConversion(fromType, toType);
                    none = ConversionRank.Constructor;
                    if (converter == null)
                    {
                        converter = FigureCastConversion(fromType, toType, ref none);
                        if (converter == null)
                        {
                            if (typeof(IConvertible).IsAssignableFrom(fromType))
                            {
                                if (IsNumeric(GetTypeCode(fromType)))
                                {
                                    if (!toType.IsArray && (GetConversionRank(typeof(string), toType) != ConversionRank.None))
                                    {
                                        converter = new PSConverter<object>(LanguagePrimitives.ConvertNumericIConvertible);
                                        none = ConversionRank.IConvertible;
                                    }
                                }
                                else if (fromType != typeof(string))
                                {
                                    converter = new PSConverter<object>(LanguagePrimitives.ConvertIConvertible);
                                    none = ConversionRank.IConvertible;
                                }
                            }
                            else if (typeof(IDictionary).IsAssignableFrom(fromType))
                            {
                                ConstructorInfo info = toType.GetConstructor(Type.EmptyTypes);
                                if ((info != null) || (toType.IsValueType && !toType.IsPrimitive))
                                {
                                    ConvertViaNoArgumentConstructor constructor = new ConvertViaNoArgumentConstructor(info, toType);
                                    converter = new PSConverter<object>(constructor.Convert);
                                    none = ConversionRank.Constructor;
                                }
                            }
                        }
                    }
                    else
                    {
                        none = ConversionRank.Constructor;
                    }
                }
                else
                {
                    none = ConversionRank.Create;
                }
            }
            else
            {
                none = ConversionRank.Parse;
            }
            if (converter == null)
            {
                Tuple<PSConverter<object>, ConversionRank> tuple = FigureIEnumerableConstructorConversion(fromType, toType);
                if (tuple != null)
                {
                    converter = tuple.Item1;
                    none = tuple.Item2;
                }
            }
            if (converter == null)
            {
                converter = FigurePropertyConversion(fromType, toType, ref none);
            }
            if ((TypeConverterPossiblyExists(fromType) || TypeConverterPossiblyExists(toType)) || ((converter != null) && (valueDependentConversion != null)))
            {
                ConvertCheckingForCustomConverter converter3 = new ConvertCheckingForCustomConverter {
                    tryfirstConverter = valueDependentConversion,
                    fallbackConverter = converter
                };
                converter = new PSConverter<object>(converter3.Convert);
                if (valueDependentRank > none)
                {
                    none = valueDependentRank;
                }
                else if (none == ConversionRank.None)
                {
                    none = ConversionRank.Custom;
                }
            }
            else if (valueDependentConversion != null)
            {
                converter = valueDependentConversion;
                none = valueDependentRank;
            }
            if (converter == null)
            {
                converter = new PSConverter<object>(LanguagePrimitives.ConvertNoConversion);
                none = ConversionRank.None;
            }
            return CacheConversion<object>(fromType, toType, converter, none);
        }

        internal static ConversionData FigureConversion(object valueToConvert, Type resultType, out bool debase)
        {
            PSObject obj2;
            Type type;
            if ((valueToConvert == null) || (valueToConvert == AutomationNull.Value))
            {
                obj2 = null;
                type = typeof(Null);
            }
            else
            {
                obj2 = valueToConvert as PSObject;
                type = valueToConvert.GetType();
            }
            debase = false;
            ConversionData data = FigureConversion(type, resultType);
            if (data.Rank != ConversionRank.None)
            {
                return data;
            }
            if (obj2 == null)
            {
                return data;
            }
            debase = true;
            valueToConvert = PSObject.Base(valueToConvert);
            if (valueToConvert == null)
            {
                type = typeof(Null);
            }
            else
            {
                type = (valueToConvert is PSObject) ? typeof(InternalPSObject) : valueToConvert.GetType();
            }
            return FigureConversion(type, resultType);
        }

        private static ConversionData FigureConversionFromNull(Type toType)
        {
            ConversionData conversionData = GetConversionData(typeof(Null), toType);
            if (conversionData != null)
            {
                return conversionData;
            }
            if (Nullable.GetUnderlyingType(toType) != null)
            {
                return CacheConversion<object>(typeof(Null), toType, new PSConverter<object>(LanguagePrimitives.ConvertNullToNullable), ConversionRank.NullToValue);
            }
            if (!toType.IsValueType)
            {
                return CacheConversion<object>(typeof(Null), toType, new PSConverter<object>(LanguagePrimitives.ConvertNullToRef), ConversionRank.NullToRef);
            }
            return CacheConversion<object>(typeof(Null), toType, new PSConverter<object>(LanguagePrimitives.ConvertNoConversion), ConversionRank.None);
        }

        internal static Tuple<PSConverter<object>, ConversionRank> FigureIEnumerableConstructorConversion(Type fromType, Type toType)
        {
            if (!toType.IsAbstract)
            {
                try
                {
                    bool flag = false;
                    bool flag2 = false;
                    Type type = null;
                    ConstructorInfo ctor = null;
                    if ((toType.IsGenericType && !toType.ContainsGenericParameters) && ((typeof(IList).IsAssignableFrom(toType) || typeof(ICollection).IsAssignableFrom(toType)) || typeof(IEnumerable).IsAssignableFrom(toType)))
                    {
                        Type[] genericArguments = toType.GetGenericArguments();
                        if (genericArguments.Length != 1)
                        {
                            typeConversion.WriteLine("toType has more than one generic arguments. Here we only care about the toType which contains only one generic argument and whose constructor takes IEnumerable<T>, ICollection<T> or IList<T>.", new object[0]);
                            return null;
                        }
                        type = genericArguments[0];
                        if ((typeof(Array).Equals(fromType) || typeof(object[]).Equals(fromType)) || type.IsAssignableFrom(fromType))
                        {
                            flag2 = type.IsAssignableFrom(fromType);
                            ConstructorInfo[] constructors = toType.GetConstructors();
                            Type type2 = typeof(IEnumerable<>).MakeGenericType(new Type[] { type });
                            Type type3 = typeof(ICollection<>).MakeGenericType(new Type[] { type });
                            Type type4 = typeof(IList<>).MakeGenericType(new Type[] { type });
                            foreach (ConstructorInfo info2 in constructors)
                            {
                                ParameterInfo[] parameters = info2.GetParameters();
                                if (parameters.Length == 1)
                                {
                                    Type parameterType = parameters[0].ParameterType;
                                    if ((type2.Equals(parameterType) || type3.Equals(parameterType)) || type4.Equals(parameterType))
                                    {
                                        ctor = info2;
                                        flag = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (flag)
                    {
                        ConvertViaIEnumerableConstructor constructor = new ConvertViaIEnumerableConstructor();
                        try
                        {
                            ConstructorInfo info3 = typeof(List<>).MakeGenericType(new Type[] { type }).GetConstructor(new Type[] { typeof(int) });
                            constructor.ListCtorLambda = CreateCtorLambdaClosure<int, IList>(info3, typeof(int), false);
                            Type realParamType = ctor.GetParameters()[0].ParameterType;
                            constructor.TargetCtorLambda = CreateCtorLambdaClosure<IList, object>(ctor, realParamType, false);
                            constructor.ElementType = type;
                            constructor.IsScalar = flag2;
                        }
                        catch (Exception exception)
                        {
                            CommandProcessorBase.CheckForSevereException(exception);
                            typeConversion.WriteLine("Exception building constructor lambda: \"{0}\"", new object[] { exception.Message });
                            return null;
                        }
                        ConversionRank rank = flag2 ? ConversionRank.ConstructorS2A : ConversionRank.Constructor;
                        typeConversion.WriteLine("Conversion is figured out. Conversion rank: \"{0}\"", new object[] { rank });
                        return new Tuple<PSConverter<object>, ConversionRank>(new PSConverter<object>(constructor.Convert), rank);
                    }
                    typeConversion.WriteLine("Fail to figure out the conversion from \"{0}\" to \"{1}\"", new object[] { fromType.FullName, toType.FullName });
                    return null;
                }
                catch (ArgumentException exception2)
                {
                    typeConversion.WriteLine("Exception finding IEnumerable conversion: \"{0}\".", new object[] { exception2.Message });
                }
                catch (InvalidOperationException exception3)
                {
                    typeConversion.WriteLine("Exception finding IEnumerable conversion: \"{0}\".", new object[] { exception3.Message });
                }
                catch (NotSupportedException exception4)
                {
                    typeConversion.WriteLine("Exception finding IEnumerable conversion: \"{0}\".", new object[] { exception4.Message });
                }
            }
            return null;
        }

        private static ConversionData FigureLanguageConversion(Type fromType, Type toType, out PSConverter<object> valueDependentConversion, out ConversionRank valueDependentRank)
        {
            valueDependentConversion = null;
            valueDependentRank = ConversionRank.None;
            Type underlyingType = Nullable.GetUnderlyingType(toType);
            if (underlyingType != null)
            {
                ConversionData data = FigureConversion(fromType, underlyingType);
                if (data.Rank != ConversionRank.None)
                {
                    return CacheConversion<object>(fromType, toType, new PSConverter<object>(LanguagePrimitives.ConvertToNullable), data.Rank);
                }
            }
            if (toType.Equals(typeof(void)))
            {
                return CacheConversion<object>(fromType, toType, new PSConverter<object>(LanguagePrimitives.ConvertToVoid), ConversionRank.Language);
            }
            if (toType.Equals(typeof(bool)))
            {
                PSConverter<bool> converter;
                if (typeof(IList).IsAssignableFrom(fromType))
                {
                    converter = new PSConverter<bool>(LanguagePrimitives.ConvertIListToBool);
                }
                else if (fromType.IsValueType)
                {
                    converter = new PSConverter<bool>(LanguagePrimitives.ConvertValueToBool);
                }
                else
                {
                    converter = new PSConverter<bool>(LanguagePrimitives.ConvertClassToBool);
                }
                return CacheConversion<bool>(fromType, toType, converter, ConversionRank.Language);
            }
            if (toType.Equals(typeof(string)))
            {
                return CacheConversion<string>(fromType, toType, new PSConverter<string>(LanguagePrimitives.ConvertNonNumericToString), ConversionRank.ToString);
            }
            if (toType.IsArray)
            {
                Type elementType = toType.GetElementType();
                if (fromType.IsArray)
                {
                    if (elementType.IsAssignableFrom(fromType.GetElementType()))
                    {
                        return CacheConversion<object>(fromType, toType, new PSConverter<object>(LanguagePrimitives.ConvertRelatedArrays), ConversionRank.Language);
                    }
                    return CacheConversion<object>(fromType, toType, new PSConverter<object>(LanguagePrimitives.ConvertUnrelatedArrays), ConversionRank.UnrelatedArrays);
                }
                if (IsTypeEnumerable(fromType))
                {
                    return CacheConversion<object>(fromType, toType, new PSConverter<object>(LanguagePrimitives.ConvertEnumerableToArray), ConversionRank.Language);
                }
                ConversionData data2 = FigureConversion(fromType, elementType);
                if (data2.Rank != ConversionRank.None)
                {
                    valueDependentRank = data2.Rank & ConversionRank.ValueDependent;
                    valueDependentConversion = new PSConverter<object>(LanguagePrimitives.ConvertScalarToArray);
                    return null;
                }
            }
            if (toType.Equals(typeof(Array)))
            {
                if (fromType.IsArray || fromType.Equals(typeof(Array)))
                {
                    return CacheConversion<object>(fromType, toType, new PSConverter<object>(LanguagePrimitives.ConvertAssignableFrom), ConversionRank.Assignable);
                }
                if (IsTypeEnumerable(fromType))
                {
                    return CacheConversion<object>(fromType, toType, new PSConverter<object>(LanguagePrimitives.ConvertEnumerableToArray), ConversionRank.Language);
                }
                valueDependentRank = ConversionRank.AssignableS2A;
                valueDependentConversion = new PSConverter<object>(LanguagePrimitives.ConvertScalarToArray);
                return null;
            }
            if (toType.Equals(typeof(Hashtable)))
            {
                if (typeof(IDictionary).IsAssignableFrom(fromType))
                {
                    return CacheConversion<Hashtable>(fromType, toType, new PSConverter<Hashtable>(LanguagePrimitives.ConvertIDictionaryToHashtable), ConversionRank.Language);
                }
                return null;
            }
            if (toType.Equals(typeof(PSReference)))
            {
                return CacheConversion<PSReference>(fromType, toType, new PSConverter<PSReference>(LanguagePrimitives.ConvertToPSReference), ConversionRank.Language);
            }
            if (toType.Equals(typeof(XmlDocument)))
            {
                return CacheConversion<XmlDocument>(fromType, toType, new PSConverter<XmlDocument>(LanguagePrimitives.ConvertToXml), ConversionRank.Language);
            }
            if (toType.Equals(typeof(StringCollection)))
            {
                ConversionRank rank = (fromType.IsArray || IsTypeEnumerable(fromType)) ? ConversionRank.Language : ConversionRank.LanguageS2A;
                return CacheConversion<StringCollection>(fromType, toType, new PSConverter<StringCollection>(LanguagePrimitives.ConvertToStringCollection), rank);
            }
            if (toType.Equals(typeof(CommaDelimitedStringCollection)))
            {
                ConversionRank rank2 = (fromType.IsArray || IsTypeEnumerable(fromType)) ? ConversionRank.Language : ConversionRank.LanguageS2A;
                return CacheConversion<CommaDelimitedStringCollection>(fromType, toType, new PSConverter<CommaDelimitedStringCollection>(LanguagePrimitives.ConvertToCommaDelimitedStringCollection), rank2);
            }
            if (toType.IsSubclassOf(typeof(Delegate)) && (fromType.Equals(typeof(ScriptBlock)) || fromType.IsSubclassOf(typeof(ScriptBlock))))
            {
                return CacheConversion<Delegate>(fromType, toType, new PSConverter<Delegate>(LanguagePrimitives.ConvertScriptBlockToDelegate), ConversionRank.Language);
            }
            if (toType.Equals(typeof(InternalPSCustomObject)))
            {
                Type type = typeof(PSObject);
                ConvertViaNoArgumentConstructor constructor = new ConvertViaNoArgumentConstructor(type.GetConstructor(Type.EmptyTypes), type);
                return CacheConversion<object>(fromType, toType, new PSConverter<object>(constructor.Convert), ConversionRank.Language);
            }
            if (IsInteger(GetTypeCode(fromType)) && toType.IsEnum)
            {
                return CacheConversion<object>(fromType, toType, new PSConverter<object>(LanguagePrimitives.ConvertIntegerToEnum), ConversionRank.Language);
            }
            return null;
        }

        private static PSConverter<object> FigureParseConversion(Type fromType, Type toType)
        {
            if (toType.IsEnum)
            {
                if (fromType.Equals(typeof(string)))
                {
                    return new PSConverter<object>(LanguagePrimitives.ConvertStringToEnum);
                }
                if (IsTypeEnumerable(fromType))
                {
                    return new PSConverter<object>(LanguagePrimitives.ConvertEnumerableToEnum);
                }
            }
            else if (fromType.Equals(typeof(string)))
            {
                BindingFlags bindingAttr = BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static;
                MethodInfo info = null;
                try
                {
                    info = toType.GetMethod("Parse", bindingAttr, null, new Type[] { typeof(string), typeof(IFormatProvider) }, null);
                }
                catch (AmbiguousMatchException exception)
                {
                    typeConversion.WriteLine("Exception finding Parse method with CultureInfo: \"{0}\".", new object[] { exception.Message });
                }
                catch (ArgumentException exception2)
                {
                    typeConversion.WriteLine("Exception finding Parse method with CultureInfo: \"{0}\".", new object[] { exception2.Message });
                }
                if (info != null)
                {
                    ConvertViaParseMethod method = new ConvertViaParseMethod {
                        parse = info
                    };
                    return new PSConverter<object>(method.ConvertWithCulture);
                }
                try
                {
                    info = toType.GetMethod("Parse", bindingAttr, null, new Type[] { typeof(string) }, null);
                }
                catch (AmbiguousMatchException exception3)
                {
                    typeConversion.WriteLine("Exception finding Parse method: \"{0}\".", new object[] { exception3.Message });
                }
                catch (ArgumentException exception4)
                {
                    typeConversion.WriteLine("Exception finding Parse method: \"{0}\".", new object[] { exception4.Message });
                }
                if (info != null)
                {
                    ConvertViaParseMethod method2 = new ConvertViaParseMethod {
                        parse = info
                    };
                    return new PSConverter<object>(method2.ConvertWithoutCulture);
                }
            }
            return null;
        }

        internal static PSConverter<object> FigurePropertyConversion(Type fromType, Type toType, ref ConversionRank rank)
        {
            if (typeof(PSObject).IsAssignableFrom(fromType) && !toType.IsAbstract)
            {
                ConstructorInfo info = null;
                try
                {
                    info = toType.GetConstructor(Type.EmptyTypes);
                }
                catch (AmbiguousMatchException exception)
                {
                    typeConversion.WriteLine("Exception finding Constructor: \"{0}\".", new object[] { exception.Message });
                }
                catch (ArgumentException exception2)
                {
                    typeConversion.WriteLine("Exception finding Constructor: \"{0}\".", new object[] { exception2.Message });
                }
                if ((info != null) || toType.IsValueType)
                {
                    typeConversion.WriteLine("Found Constructor.", new object[0]);
                    try
                    {
                        ConvertViaNoArgumentConstructor constructor = new ConvertViaNoArgumentConstructor(info, toType);
                        rank = ConversionRank.Constructor;
                        return new PSConverter<object>(constructor.Convert);
                    }
                    catch (ArgumentException exception3)
                    {
                        typeConversion.WriteLine("Exception converting via no argument constructor: \"{0}\".", new object[] { exception3.Message });
                    }
                    catch (InvalidOperationException exception4)
                    {
                        typeConversion.WriteLine("Exception converting via no argument constructor: \"{0}\".", new object[] { exception4.Message });
                    }
                    rank = ConversionRank.None;
                }
            }
            return null;
        }

        private static PSConverter<object> FigureStaticCreateMethodConversion(Type fromType, Type toType)
        {
            if (fromType.Equals(typeof(string)) && toType.Equals(typeof(CimSession)))
            {
                return new PSConverter<object>(LanguagePrimitives.ConvertStringToCimSession);
            }
            return null;
        }

        private static MethodInfo FindCastOperator(string methodName, Type targetType, Type originalType, Type resultType)
        {
            using (typeConversion.TraceScope("Looking for \"{0}\" cast operator.", new object[] { methodName }))
            {
                foreach (MethodInfo info in targetType.GetMember(methodName, BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static))
                {
                    if (resultType.IsAssignableFrom(info.ReturnType))
                    {
                        ParameterInfo[] parameters = info.GetParameters();
                        if ((parameters.Length == 1) && parameters[0].ParameterType.IsAssignableFrom(originalType))
                        {
                            typeConversion.WriteLine("Found \"{0}\" cast operator in type {1}.", new object[] { methodName, targetType.FullName });
                            return info;
                        }
                    }
                }
                typeConversion.TraceScope("Cast operator for \"{0}\" not found.", new object[] { methodName });
                return null;
            }
        }

        internal static T FromObjectAs<T>(object castObject)
        {
            T local = default(T);
            PSObject obj2 = castObject as PSObject;
            if (obj2 == null)
            {
                try
                {
                    return (T) castObject;
                }
                catch (InvalidCastException)
                {
                    return default(T);
                }
            }
            try
            {
                return (T) obj2.BaseObject;
            }
            catch (InvalidCastException)
            {
                return default(T);
            }
        }

        private static string GetAvailableProperties(PSObject pso)
        {
            StringBuilder builder = new StringBuilder();
            bool flag = true;
            if ((pso != null) && (pso.Properties != null))
            {
                foreach (PSPropertyInfo info in pso.Properties)
                {
                    if (!flag)
                    {
                        builder.Append(" , ");
                    }
                    builder.Append("[" + info.Name + " <" + info.TypeNameOfValue + ">]");
                    if (flag)
                    {
                        flag = false;
                    }
                }
            }
            return builder.ToString();
        }

        private static ConversionData GetConversionData(Type fromType, Type toType)
        {
            lock (converterCache)
            {
                ConversionData data = null;
                converterCache.TryGetValue(new ConversionTypePair(fromType, toType), out data);
                return data;
            }
        }

        internal static ConversionRank GetConversionRank(Type fromType, Type toType)
        {
            return FigureConversion(fromType, toType).Rank;
        }

        internal static object GetConverter(Type type, TypeTable backupTypeTable)
        {
            object typeConverter = null;
            ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
            if (executionContextFromTLS != null)
            {
                tracer.WriteLine("ecFromTLS != null", new object[0]);
                typeConverter = executionContextFromTLS.TypeTable.GetTypeConverter(type.FullName);
            }
            if ((typeConverter == null) && (backupTypeTable != null))
            {
                tracer.WriteLine("Using provided TypeTable to get the type converter", new object[0]);
                typeConverter = backupTypeTable.GetTypeConverter(type.FullName);
            }
            if (typeConverter != null)
            {
                tracer.WriteLine("typesXmlConverter != null", new object[0]);
                return typeConverter;
            }
            object[] customAttributes = type.GetCustomAttributes(typeof(TypeConverterAttribute), false);
            if (customAttributes.Length != 0)
            {
                TypeConverterAttribute attribute = (TypeConverterAttribute) customAttributes[0];
                string converterTypeName = attribute.ConverterTypeName;
                typeConversion.WriteLine("{0}'s TypeConverterAttribute points to {1}.", new object[] { type, converterTypeName });
                return NewConverterInstance(converterTypeName);
            }
            return null;
        }

        private static CultureInfo GetCultureFromFormatProvider(IFormatProvider formatProvider)
        {
            CultureInfo invariantCulture = formatProvider as CultureInfo;
            if (invariantCulture == null)
            {
                invariantCulture = CultureInfo.InvariantCulture;
            }
            return invariantCulture;
        }

        public static IEnumerable GetEnumerable(object obj)
        {
            if (obj == null)
            {
                return null;
            }
            Type type = obj.GetType();
            if (type == typeof(PSObject))
            {
                PSObject obj2 = (PSObject) obj;
                obj = obj2.BaseObject;
                type = obj.GetType();
            }
            return GetOrCalculateEnumerable(type)(obj);
        }

        private static IEnumerable GetEnumerableFromIEnumerableT(object obj)
        {
            foreach (Type type in obj.GetType().GetInterfaces())
            {
                if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
                {
                    return new EnumerableTWrapper(obj, type);
                }
            }
            return null;
        }

        public static IEnumerator GetEnumerator(object obj)
        {
            IEnumerator enumerator = _getEnumeratorSite.Target(_getEnumeratorSite, obj);
            if (enumerator is EnumerableOps.NonEnumerableObjectEnumerator)
            {
                return null;
            }
            return enumerator;
        }

        private static TypeConverter GetIntegerSystemConverter(Type type)
        {
            if (type == typeof(short))
            {
                return new Int16Converter();
            }
            if (type == typeof(int))
            {
                return new Int32Converter();
            }
            if (type == typeof(long))
            {
                return new Int64Converter();
            }
            if (type == typeof(ushort))
            {
                return new UInt16Converter();
            }
            if (type == typeof(int))
            {
                return new UInt32Converter();
            }
            if (type == typeof(ulong))
            {
                return new UInt64Converter();
            }
            if (type == typeof(byte))
            {
                return new ByteConverter();
            }
            if (type == typeof(sbyte))
            {
                return new SByteConverter();
            }
            return null;
        }

        private static GetEnumerableDelegate GetOrCalculateEnumerable(Type type)
        {
            GetEnumerableDelegate delegate2 = null;
            lock (getEnumerableCache)
            {
                if (!getEnumerableCache.TryGetValue(type, out delegate2))
                {
                    delegate2 = CalculateGetEnumerable(type);
                    getEnumerableCache.Add(type, delegate2);
                }
            }
            return delegate2;
        }

        public static PSDataCollection<PSObject> GetPSDataCollection(object inputValue)
        {
            PSDataCollection<PSObject> datas = new PSDataCollection<PSObject>();
            if (inputValue != null)
            {
                IEnumerator enumerator = GetEnumerator(inputValue);
                if (enumerator != null)
                {
                    while (enumerator.MoveNext())
                    {
                        datas.Add((enumerator.Current == null) ? null : PSObject.AsPSObject(enumerator.Current));
                    }
                }
                else
                {
                    datas.Add(PSObject.AsPSObject(inputValue));
                }
            }
            datas.Complete();
            return datas;
        }

        internal static TypeCode GetTypeCode(Type type)
        {
            if (type.IsEnum)
            {
                return TypeCode.Object;
            }
            return Type.GetTypeCode(type);
        }

        private static void InitializeGetEnumerableCache()
        {
            lock (getEnumerableCache)
            {
                getEnumerableCache.Clear();
                getEnumerableCache.Add(typeof(string), new GetEnumerableDelegate(LanguagePrimitives.ReturnNullEnumerable));
                getEnumerableCache.Add(typeof(int), new GetEnumerableDelegate(LanguagePrimitives.ReturnNullEnumerable));
                getEnumerableCache.Add(typeof(double), new GetEnumerableDelegate(LanguagePrimitives.ReturnNullEnumerable));
            }
        }

        internal static bool IsBooleanType(Type type)
        {
            if (!(type == typeof(bool)) && !(type == typeof(bool?)))
            {
                return false;
            }
            return true;
        }

        internal static bool IsBoolOrSwitchParameterType(Type type)
        {
            if (!IsBooleanType(type) && !IsSwitchParameterType(type))
            {
                return false;
            }
            return true;
        }

        internal static bool IsCimIntrinsicScalarType(Type type)
        {
            return (IsCimIntrinsicScalarType(GetTypeCode(type)) || (type == typeof(TimeSpan)));
        }

        internal static bool IsCimIntrinsicScalarType(TypeCode typeCode)
        {
            return ((typeCodeTraits[(int) typeCode] & TypeCodeTraits.CimIntrinsicType) != TypeCodeTraits.None);
        }

        private static bool IsCustomTypeConversion(object valueToConvert, Type resultType, IFormatProvider formatProvider, out object result, TypeTable backupTypeTable)
        {
            using (typeConversion.TraceScope("Custom type conversion.", new object[0]))
            {
                object obj2 = PSObject.Base(valueToConvert);
                Type type = obj2.GetType();
                object obj3 = GetConverter(type, backupTypeTable);
                if (obj3 != null)
                {
                    TypeConverter converter = obj3 as TypeConverter;
                    if (converter != null)
                    {
                        typeConversion.WriteLine("Original type's converter is TypeConverter.", new object[0]);
                        if (converter.CanConvertTo(resultType))
                        {
                            typeConversion.WriteLine("TypeConverter can convert to resultType.", new object[0]);
                            try
                            {
                                result = converter.ConvertTo(null, GetCultureFromFormatProvider(formatProvider), obj2, resultType);
                                return true;
                            }
                            catch (Exception exception)
                            {
                                typeConversion.WriteLine("Exception converting with Original type's TypeConverter: \"{0}\".", new object[] { exception.Message });
                                CommandProcessorBase.CheckForSevereException(exception);
                                throw new PSInvalidCastException("InvalidCastTypeConvertersConvertTo", exception, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), exception.Message });
                            }
                        }
                        typeConversion.WriteLine("TypeConverter cannot convert to resultType.", new object[0]);
                    }
                    PSTypeConverter converter2 = obj3 as PSTypeConverter;
                    if (converter2 != null)
                    {
                        typeConversion.WriteLine("Original type's converter is PSTypeConverter.", new object[0]);
                        PSObject sourceValue = PSObject.AsPSObject(valueToConvert);
                        if (converter2.CanConvertTo(sourceValue, resultType))
                        {
                            typeConversion.WriteLine("Original type's PSTypeConverter can convert to resultType.", new object[0]);
                            try
                            {
                                result = converter2.ConvertTo(sourceValue, resultType, formatProvider, true);
                                return true;
                            }
                            catch (Exception exception2)
                            {
                                typeConversion.WriteLine("Exception converting with Original type's PSTypeConverter: \"{0}\".", new object[] { exception2.Message });
                                CommandProcessorBase.CheckForSevereException(exception2);
                                throw new PSInvalidCastException("InvalidCastPSTypeConvertersConvertTo", exception2, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), exception2.Message });
                            }
                        }
                        typeConversion.WriteLine("Original type's PSTypeConverter cannot convert to resultType.", new object[0]);
                    }
                }
                tracer.WriteLine("No converter found in original type.", new object[0]);
                obj3 = GetConverter(resultType, backupTypeTable);
                if (obj3 != null)
                {
                    TypeConverter converter3 = obj3 as TypeConverter;
                    if (converter3 != null)
                    {
                        typeConversion.WriteLine("Destination type's converter is TypeConverter that can convert from originalType.", new object[0]);
                        if (converter3.CanConvertFrom(type))
                        {
                            typeConversion.WriteLine("Destination type's converter can convert from originalType.", new object[0]);
                            try
                            {
                                result = converter3.ConvertFrom(null, GetCultureFromFormatProvider(formatProvider), obj2);
                                return true;
                            }
                            catch (Exception exception3)
                            {
                                typeConversion.WriteLine("Exception converting with Destination type's TypeConverter: \"{0}\".", new object[] { exception3.Message });
                                CommandProcessorBase.CheckForSevereException(exception3);
                                throw new PSInvalidCastException("InvalidCastTypeConvertersConvertFrom", exception3, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), exception3.Message });
                            }
                        }
                        typeConversion.WriteLine("Destination type's converter cannot convert from originalType.", new object[0]);
                    }
                    PSTypeConverter converter4 = obj3 as PSTypeConverter;
                    if (converter4 != null)
                    {
                        typeConversion.WriteLine("Destination type's converter is PSTypeConverter.", new object[0]);
                        PSObject obj5 = PSObject.AsPSObject(valueToConvert);
                        if (converter4.CanConvertFrom(obj5, resultType))
                        {
                            typeConversion.WriteLine("Destination type's converter can convert from originalType.", new object[0]);
                            try
                            {
                                result = converter4.ConvertFrom(obj5, resultType, formatProvider, true);
                                return true;
                            }
                            catch (Exception exception4)
                            {
                                typeConversion.WriteLine("Exception converting with Destination type's PSTypeConverter: \"{0}\".", new object[] { exception4.Message });
                                CommandProcessorBase.CheckForSevereException(exception4);
                                throw new PSInvalidCastException("InvalidCastPSTypeConvertersConvertFrom", exception4, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), exception4.Message });
                            }
                        }
                        typeConversion.WriteLine("Destination type's converter cannot convert from originalType.", new object[0]);
                    }
                }
                result = null;
                return false;
            }
        }

        internal static bool IsFloating(TypeCode typeCode)
        {
            return ((typeCodeTraits[(int) typeCode] & TypeCodeTraits.Floating) != TypeCodeTraits.None);
        }

        internal static bool IsInteger(TypeCode typeCode)
        {
            return ((typeCodeTraits[(int) typeCode] & TypeCodeTraits.Integer) != TypeCodeTraits.None);
        }

        private static bool IsIntegralType(Type type)
        {
            if (((!type.Equals(typeof(sbyte)) && !type.Equals(typeof(byte))) && (!type.Equals(typeof(short)) && !type.Equals(typeof(ushort)))) && ((!type.Equals(typeof(int)) && !type.Equals(typeof(int))) && !type.Equals(typeof(long))))
            {
                return type.Equals(typeof(ulong));
            }
            return true;
        }

        internal static bool IsNull(object obj)
        {
            if (obj != null)
            {
                return (obj == AutomationNull.Value);
            }
            return true;
        }

        internal static bool IsNumeric(TypeCode typeCode)
        {
            return ((typeCodeTraits[(int) typeCode] & TypeCodeTraits.Numeric) != TypeCodeTraits.None);
        }

        internal static bool IsPublic(Type type)
        {
            if (type == null)
            {
                throw PSTraceSource.NewArgumentNullException("type");
            }
            if (!type.IsPublic)
            {
                if (!type.IsNestedPublic)
                {
                    return false;
                }
                type = type.DeclaringType;
                while (type != null)
                {
                    if (!type.IsPublic && !type.IsNestedPublic)
                    {
                        return false;
                    }
                    type = type.DeclaringType;
                }
            }
            return true;
        }

        internal static bool IsSignedInteger(TypeCode typeCode)
        {
            return ((typeCodeTraits[(int) typeCode] & TypeCodeTraits.SignedInteger) != TypeCodeTraits.None);
        }

        internal static bool IsSwitchParameterType(Type type)
        {
            if (!(type == typeof(SwitchParameter)) && !(type == typeof(SwitchParameter?)))
            {
                return false;
            }
            return true;
        }

        internal static bool IsTrue(IList objectArray)
        {
            switch (objectArray.Count)
            {
                case 0:
                    return false;

                case 1:
                {
                    IList list = objectArray[0] as IList;
                    if (list != null)
                    {
                        if (list.Count < 1)
                        {
                            return false;
                        }
                        return true;
                    }
                    return IsTrue(objectArray[0]);
                }
            }
            return true;
        }

        public static bool IsTrue(object obj)
        {
            if ((obj == null) || (obj == AutomationNull.Value))
            {
                return false;
            }
            obj = PSObject.Base(obj);
            Type type = obj.GetType();
            if (type.Equals(typeof(bool)))
            {
                return (bool) obj;
            }
            if (type.Equals(typeof(string)))
            {
                return IsTrue((string) obj);
            }
            if (IsNumeric(GetTypeCode(type)))
            {
                return !obj.Equals(Convert.ChangeType(0, type, CultureInfo.InvariantCulture));
            }
            if (type.Equals(typeof(SwitchParameter)))
            {
                SwitchParameter parameter = (SwitchParameter) obj;
                return parameter.ToBool();
            }
            IList objectArray = obj as IList;
            if (objectArray != null)
            {
                return IsTrue(objectArray);
            }
            return true;
        }

        internal static bool IsTrue(string s)
        {
            return (s.Length != 0);
        }

        internal static bool IsTypeEnumerable(Type type)
        {
            return (GetOrCalculateEnumerable(type) != new GetEnumerableDelegate(LanguagePrimitives.ReturnNullEnumerable));
        }

        internal static bool IsUnsignedInteger(TypeCode typeCode)
        {
            return ((typeCodeTraits[(int) typeCode] & TypeCodeTraits.UnsignedInteger) != TypeCodeTraits.None);
        }

        private static Type LookForTypeInAssemblies(string typeName, IEnumerable<Assembly> assemblies)
        {
            foreach (Assembly assembly in assemblies)
            {
                try
                {
                    Type type = assembly.GetType(typeName, false, true);
                    if (type != null)
                    {
                        if (IsPublic(type))
                        {
                            return type;
                        }
                        typeConversion.WriteLine("\"{0}\" is not public, so it will not be returned.", new object[] { type });
                    }
                }
                catch (Exception exception)
                {
                    CommandProcessorBase.CheckForSevereException(exception);
                    typeConversion.WriteLine("System.Reflection.Assembly's GetType threw an exception for \"{0}\": \"{1}\".", new object[] { typeName, exception.Message });
                }
            }
            return null;
        }

        private static object NewConverterInstance(string assemblyQualifiedTypeName)
        {
            int index = assemblyQualifiedTypeName.IndexOf(",", StringComparison.Ordinal);
            if (index == -1)
            {
                typeConversion.WriteLine("Type name \"{0}\" should be assembly qualified.", new object[] { assemblyQualifiedTypeName });
                return null;
            }
            string str = assemblyQualifiedTypeName.Substring(index + 2);
            string name = assemblyQualifiedTypeName.Substring(0, index);
            Type type = null;
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName == str)
                {
                    try
                    {
                        type = assembly.GetType(name, false);
                    }
                    catch (ArgumentException exception)
                    {
                        typeConversion.WriteLine("Assembly \"{0}\" threw an exception when retrieving the type \"{1}\": \"{2}\".", new object[] { str, name, exception.Message });
                        return null;
                    }
                    try
                    {
                        return Activator.CreateInstance(type);
                    }
                    catch (Exception exception2)
                    {
                        CommandProcessorBase.CheckForSevereException(exception2);
                        TargetInvocationException exception3 = exception2 as TargetInvocationException;
                        string str3 = ((exception3 == null) || (exception3.InnerException == null)) ? exception2.Message : exception3.InnerException.Message;
                        typeConversion.WriteLine("Creating an instance of type \"{0}\" caused an exception to be thrown: \"{1}\"", new object[] { assemblyQualifiedTypeName, str3 });
                        return null;
                    }
                }
            }
            typeConversion.WriteLine("Could not create an instance of type \"{0}\".", new object[] { assemblyQualifiedTypeName });
            return null;
        }

        private static int NumericCompare(object number1, object number2, int index1, int index2)
        {
            if ((index1 == 10) && ((index2 == 8) || (index2 == 9)))
            {
                return NumericCompareDecimal((decimal) number1, number2);
            }
            if ((index2 == 10) && ((index1 == 8) || (index1 == 9)))
            {
                return -NumericCompareDecimal((decimal) number2, number1);
            }
            Type conversionType = LargestTypeTable[index1][index2];
            object obj2 = Convert.ChangeType(number1, conversionType, CultureInfo.InvariantCulture);
            object obj3 = Convert.ChangeType(number2, conversionType, CultureInfo.InvariantCulture);
            return ((IComparable) obj2).CompareTo(obj3);
        }

        private static int NumericCompareDecimal(decimal decimalNumber, object otherNumber)
        {
            object obj2 = null;
            try
            {
                obj2 = Convert.ChangeType(otherNumber, typeof(decimal), CultureInfo.InvariantCulture);
            }
            catch (OverflowException)
            {
                try
                {
                    double num = (double) Convert.ChangeType(decimalNumber, typeof(double), CultureInfo.InvariantCulture);
                    double num2 = (double) Convert.ChangeType(otherNumber, typeof(double), CultureInfo.InvariantCulture);
                    return num.CompareTo(num2);
                }
                catch (Exception exception)
                {
                    CommandProcessorBase.CheckForSevereException(exception);
                    return -1;
                }
            }
            catch (Exception exception2)
            {
                CommandProcessorBase.CheckForSevereException(exception2);
                return -1;
            }
            return decimalNumber.CompareTo(obj2);
        }

        internal static string ObjectToTypeNameString(object o)
        {
            if (o == null)
            {
                return "null";
            }
            ConsolidatedString internalTypeNames = PSObject.AsPSObject(o).InternalTypeNames;
            if ((internalTypeNames != null) && (internalTypeNames.Count > 0))
            {
                return internalTypeNames[0];
            }
            return ToStringCodeMethods.Type(o.GetType(), false);
        }

        private static void RebuildConversionCache()
        {
            lock (converterCache)
            {
                converterCache.Clear();
                Type toType = typeof(string);
                Type fromType = typeof(Null);
                Type type3 = typeof(float);
                Type type4 = typeof(double);
                Type type5 = typeof(decimal);
                Type type6 = typeof(bool);
                Type type7 = typeof(char);
                foreach (Type type8 in NumericTypes)
                {
                    CacheConversion<string>(type8, toType, new PSConverter<string>(LanguagePrimitives.ConvertNumericToString), ConversionRank.NumericString);
                    CacheConversion<object>(type8, type7, new PSConverter<object>(LanguagePrimitives.ConvertIConvertible), ConversionRank.NumericString);
                    CacheConversion<object>(fromType, type8, new PSConverter<object>(LanguagePrimitives.ConvertNullToNumeric), ConversionRank.NullToValue);
                    CacheConversion<bool>(type8, type6, new PSConverter<bool>(LanguagePrimitives.ConvertNumericToBool), ConversionRank.Language);
                }
                for (int i = 0; i < UnsignedIntegerTypes.Length; i++)
                {
                    CacheConversion<object>(UnsignedIntegerTypes[i], UnsignedIntegerTypes[i], new PSConverter<object>(LanguagePrimitives.ConvertAssignableFrom), ConversionRank.Identity);
                    CacheConversion<object>(SignedIntegerTypes[i], SignedIntegerTypes[i], new PSConverter<object>(LanguagePrimitives.ConvertAssignableFrom), ConversionRank.Identity);
                    CacheConversion<object>(UnsignedIntegerTypes[i], SignedIntegerTypes[i], new PSConverter<object>(LanguagePrimitives.ConvertNumeric), ConversionRank.NumericExplicit);
                    CacheConversion<object>(SignedIntegerTypes[i], UnsignedIntegerTypes[i], new PSConverter<object>(LanguagePrimitives.ConvertNumeric), ConversionRank.NumericExplicit1);
                    for (int j = i + 1; j < UnsignedIntegerTypes.Length; j++)
                    {
                        CacheConversion<object>(UnsignedIntegerTypes[i], UnsignedIntegerTypes[j], new PSConverter<object>(LanguagePrimitives.ConvertNumeric), ConversionRank.NumericImplicit);
                        CacheConversion<object>(SignedIntegerTypes[i], SignedIntegerTypes[j], new PSConverter<object>(LanguagePrimitives.ConvertNumeric), ConversionRank.NumericImplicit);
                        CacheConversion<object>(UnsignedIntegerTypes[i], SignedIntegerTypes[j], new PSConverter<object>(LanguagePrimitives.ConvertNumeric), ConversionRank.NumericImplicit);
                        CacheConversion<object>(SignedIntegerTypes[i], UnsignedIntegerTypes[j], new PSConverter<object>(LanguagePrimitives.ConvertNumeric), ConversionRank.NumericExplicit1);
                        CacheConversion<object>(UnsignedIntegerTypes[j], UnsignedIntegerTypes[i], new PSConverter<object>(LanguagePrimitives.ConvertNumeric), ConversionRank.NumericExplicit);
                        CacheConversion<object>(SignedIntegerTypes[j], SignedIntegerTypes[i], new PSConverter<object>(LanguagePrimitives.ConvertNumeric), ConversionRank.NumericExplicit);
                        CacheConversion<object>(UnsignedIntegerTypes[j], SignedIntegerTypes[i], new PSConverter<object>(LanguagePrimitives.ConvertNumeric), ConversionRank.NumericExplicit);
                        CacheConversion<object>(SignedIntegerTypes[j], UnsignedIntegerTypes[i], new PSConverter<object>(LanguagePrimitives.ConvertNumeric), ConversionRank.NumericExplicit);
                    }
                }
                foreach (Type type9 in IntegerTypes)
                {
                    CacheConversion<object>(toType, type9, new PSConverter<object>(LanguagePrimitives.ConvertStringToInteger), ConversionRank.NumericString);
                    foreach (Type type10 in RealTypes)
                    {
                        CacheConversion<object>(type9, type10, new PSConverter<object>(LanguagePrimitives.ConvertNumeric), ConversionRank.NumericImplicit);
                        CacheConversion<object>(type10, type9, new PSConverter<object>(LanguagePrimitives.ConvertNumeric), ConversionRank.NumericExplicit);
                    }
                }
                CacheConversion<object>(type3, type4, new PSConverter<object>(LanguagePrimitives.ConvertNumeric), ConversionRank.NumericImplicit);
                CacheConversion<object>(type4, type3, new PSConverter<object>(LanguagePrimitives.ConvertNumeric), ConversionRank.NumericExplicit);
                CacheConversion<object>(type3, type5, new PSConverter<object>(LanguagePrimitives.ConvertNumeric), ConversionRank.NumericExplicit);
                CacheConversion<object>(type4, type5, new PSConverter<object>(LanguagePrimitives.ConvertNumeric), ConversionRank.NumericExplicit);
                CacheConversion<object>(type5, type3, new PSConverter<object>(LanguagePrimitives.ConvertNumeric), ConversionRank.NumericExplicit1);
                CacheConversion<object>(type5, type4, new PSConverter<object>(LanguagePrimitives.ConvertNumeric), ConversionRank.NumericExplicit1);
                CacheConversion<Regex>(toType, typeof(Regex), new PSConverter<Regex>(LanguagePrimitives.ConvertStringToRegex), ConversionRank.Language);
                CacheConversion<char[]>(toType, typeof(char[]), new PSConverter<char[]>(LanguagePrimitives.ConvertStringToCharArray), ConversionRank.StringToCharArray);
                CacheConversion<ManagementObjectSearcher>(toType, typeof(ManagementObjectSearcher), new PSConverter<ManagementObjectSearcher>(LanguagePrimitives.ConvertToWMISearcher), ConversionRank.Language);
                CacheConversion<ManagementClass>(toType, typeof(ManagementClass), new PSConverter<ManagementClass>(LanguagePrimitives.ConvertToWMIClass), ConversionRank.Language);
                CacheConversion<ManagementObject>(toType, typeof(ManagementObject), new PSConverter<ManagementObject>(LanguagePrimitives.ConvertToWMI), ConversionRank.Language);
                CacheConversion<DirectoryEntry>(toType, typeof(DirectoryEntry), new PSConverter<DirectoryEntry>(LanguagePrimitives.ConvertToADSI), ConversionRank.Language);
                CacheConversion<object>(toType, typeof(DirectorySearcher), FigureConstructorConversion(toType, typeof(DirectorySearcher)), ConversionRank.Language);
                CacheConversion<Type>(toType, typeof(Type), new PSConverter<Type>(LanguagePrimitives.ConvertStringToType), ConversionRank.Language);
                CacheConversion<object>(toType, type5, new PSConverter<object>(LanguagePrimitives.ConvertStringToDecimal), ConversionRank.NumericString);
                CacheConversion<object>(toType, type3, new PSConverter<object>(LanguagePrimitives.ConvertStringToReal), ConversionRank.NumericString);
                CacheConversion<object>(toType, type4, new PSConverter<object>(LanguagePrimitives.ConvertStringToReal), ConversionRank.NumericString);
                CacheConversion<object>(type7, type3, new PSConverter<object>(LanguagePrimitives.ConvertNumericChar), ConversionRank.Language);
                CacheConversion<object>(type7, type4, new PSConverter<object>(LanguagePrimitives.ConvertNumericChar), ConversionRank.Language);
                CacheConversion<bool>(type7, type6, new PSConverter<bool>(LanguagePrimitives.ConvertCharToBool), ConversionRank.Language);
                CacheConversion<char>(fromType, type7, new PSConverter<char>(LanguagePrimitives.ConvertNullToChar), ConversionRank.NullToValue);
                CacheConversion<string>(fromType, toType, new PSConverter<string>(LanguagePrimitives.ConvertNullToString), ConversionRank.ToString);
                CacheConversion<bool>(fromType, type6, new PSConverter<bool>(LanguagePrimitives.ConvertNullToBool), ConversionRank.NullToValue);
                CacheConversion<PSReference>(fromType, typeof(PSReference), new PSConverter<PSReference>(LanguagePrimitives.ConvertNullToPSReference), ConversionRank.NullToRef);
                CacheConversion<SwitchParameter>(fromType, typeof(SwitchParameter), new PSConverter<SwitchParameter>(LanguagePrimitives.ConvertNullToSwitch), ConversionRank.NullToValue);
                CacheConversion<object>(fromType, typeof(void), new PSConverter<object>(LanguagePrimitives.ConvertNullToVoid), ConversionRank.NullToValue);
                CacheConversion<object>(type6, type6, new PSConverter<object>(LanguagePrimitives.ConvertAssignableFrom), ConversionRank.Identity);
                CacheConversion<bool>(toType, type6, new PSConverter<bool>(LanguagePrimitives.ConvertStringToBool), ConversionRank.Language);
                CacheConversion<bool>(typeof(SwitchParameter), type6, new PSConverter<bool>(LanguagePrimitives.ConvertSwitchParameterToBool), ConversionRank.Language);
            }
        }

        internal static void ResetCaches(TypeTable typeTable)
        {
            RebuildConversionCache();
            if (typeTable != null)
            {
                lock (possibleTypeConverter)
                {
                    typeTable.ForEachTypeConverter(x => possibleTypeConverter[x] = true);
                }
            }
        }

        private static IEnumerable ReturnNullEnumerable(object obj)
        {
            return null;
        }

        internal static PSObject SetObjectProperties(object o, PSObject psObject, Type resultType, MemberNotFoundError memberNotFoundErrorAction, MemberSetValueError memberSetValueErrorAction)
        {
            if (Deserializer.IsDeserializedInstanceOfType(psObject, resultType))
            {
                try
                {
                    Dictionary<string, object> dictionary = new Dictionary<string, object>();
                    foreach (PSPropertyInfo info in psObject.Properties)
                    {
                        if (info is PSProperty)
                        {
                            dictionary.Add(info.Name, info.Value);
                        }
                    }
                    return SetObjectProperties(o, dictionary, resultType, memberNotFoundErrorAction, memberSetValueErrorAction, false);
                }
                catch (SetValueException)
                {
                    goto Label_008B;
                }
                catch (InvalidOperationException)
                {
                    goto Label_008B;
                }
            }
            IDictionary properties = PSObject.Base(psObject) as IDictionary;
            if (properties != null)
            {
                return SetObjectProperties(o, properties, resultType, memberNotFoundErrorAction, memberSetValueErrorAction, false);
            }
        Label_008B:
            ThrowInvalidCastException(psObject, resultType);
            return null;
        }

        internal static PSObject SetObjectProperties(object o, IDictionary properties, Type resultType, MemberNotFoundError memberNotFoundErrorAction, MemberSetValueError memberSetValueErrorAction, bool enableMethodCall)
        {
            PSObject pso = PSObject.AsPSObject(o);
            if (properties != null)
            {
                foreach (DictionaryEntry entry in properties)
                {
                    PSMethodInfo info = enableMethodCall ? pso.Methods[entry.Key.ToString()] : null;
                    try
                    {
                        if (info != null)
                        {
                            info.Invoke(new object[] { entry.Value });
                        }
                        else
                        {
                            PSPropertyInfo info2 = pso.Properties[entry.Key.ToString()];
                            if (info2 != null)
                            {
                                info2.Value = entry.Value;
                            }
                            else if (pso.BaseObject is PSCustomObject)
                            {
                                string key = entry.Key as string;
                                string item = entry.Value as string;
                                if (((key != null) && (item != null)) && key.Equals("PSTypeName", StringComparison.OrdinalIgnoreCase))
                                {
                                    pso.TypeNames.Insert(0, item);
                                }
                                else
                                {
                                    string name = entry.Key.ToString();
                                    pso.Properties.Add(new PSNoteProperty(name, entry.Value));
                                }
                            }
                            else
                            {
                                memberNotFoundErrorAction(pso, entry, resultType);
                            }
                        }
                    }
                    catch (SetValueException exception)
                    {
                        memberSetValueErrorAction(exception);
                    }
                }
            }
            return pso;
        }

        internal static object ThrowInvalidCastException(object valueToConvert, Type resultType)
        {
            if (PSObject.Base(valueToConvert) == null)
            {
                if (resultType.IsEnum)
                {
                    typeConversion.WriteLine("Issuing an error message about not being able to convert null to an Enum type.", new object[0]);
                    throw new PSInvalidCastException("nullToEnumInvalidCast", null, ExtendedTypeSystem.InvalidCastExceptionEnumerationNull, new object[] { resultType, EnumSingleTypeConverter.EnumValues(resultType) });
                }
                typeConversion.WriteLine("Cannot convert null.", new object[0]);
                throw new PSInvalidCastException("nullToObjectInvalidCast", null, ExtendedTypeSystem.InvalidCastFromNull, new object[] { resultType.ToString() });
            }
            typeConversion.WriteLine("Type Conversion failed.", new object[0]);
            throw new PSInvalidCastException("ConvertToFinalInvalidCastException", null, ExtendedTypeSystem.InvalidCastException, new object[] { valueToConvert.ToString(), ObjectToTypeNameString(valueToConvert), resultType.ToString() });
        }

        internal static object ThrowInvalidConversionException(object valueToConvert, Type resultType)
        {
            typeConversion.WriteLine("Issuing an error message about not being able to convert to non-core type.", new object[0]);
            throw new PSInvalidCastException("ConversionSupportedOnlyToCoreTypes", null, ExtendedTypeSystem.InvalidCastExceptionNonCoreType, new object[] { resultType.ToString() });
        }

        public static bool TryConvertTo<T>(object valueToConvert, out T result)
        {
            return TryConvertTo<T>(valueToConvert, CultureInfo.InvariantCulture, out result);
        }

        public static bool TryConvertTo<T>(object valueToConvert, IFormatProvider formatProvider, out T result)
        {
            result = default(T);
            try
            {
                result = (T) ConvertTo(valueToConvert, typeof(T), formatProvider);
            }
            catch (InvalidCastException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
            return true;
        }

        public static bool TryConvertTo(object valueToConvert, Type resultType, out object result)
        {
            return TryConvertTo(valueToConvert, resultType, CultureInfo.InvariantCulture, out result);
        }

        public static bool TryConvertTo(object valueToConvert, Type resultType, IFormatProvider formatProvider, out object result)
        {
            result = null;
            try
            {
                result = ConvertTo(valueToConvert, resultType, formatProvider);
            }
            catch (InvalidCastException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
            return true;
        }

        private static bool TypeConverterPossiblyExists(Type type)
        {
            lock (possibleTypeConverter)
            {
                if (possibleTypeConverter.ContainsKey(type.FullName))
                {
                    return true;
                }
            }
            return (type.GetCustomAttributes(typeof(TypeConverterAttribute), false).Length != 0);
        }

        internal static int TypeTableIndex(Type type)
        {
            switch (GetTypeCode(type))
            {
                case TypeCode.SByte:
                    return 6;

                case TypeCode.Byte:
                    return 7;

                case TypeCode.Int16:
                    return 0;

                case TypeCode.UInt16:
                    return 3;

                case TypeCode.Int32:
                    return 1;

                case TypeCode.UInt32:
                    return 4;

                case TypeCode.Int64:
                    return 2;

                case TypeCode.UInt64:
                    return 5;

                case TypeCode.Single:
                    return 8;

                case TypeCode.Double:
                    return 9;

                case TypeCode.Decimal:
                    return 10;
            }
            return -1;
        }

        private static IEnumerable TypicalEnumerable(object obj)
        {
            IEnumerable enumerableFromIEnumerableT = (IEnumerable) obj;
            try
            {
                if (enumerableFromIEnumerableT.GetEnumerator() == null)
                {
                    return GetEnumerableFromIEnumerableT(obj);
                }
                return enumerableFromIEnumerableT;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                enumerableFromIEnumerableT = GetEnumerableFromIEnumerableT(obj);
                if (enumerableFromIEnumerableT == null)
                {
                    throw new ExtendedTypeSystemException("ExceptionInGetEnumerator", exception, ExtendedTypeSystem.EnumerationException, new object[] { exception.Message });
                }
                return enumerableFromIEnumerableT;
            }
        }

        internal interface ConversionData
        {
            object Invoke(object valueToConvert, Type resultType, bool recurse, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable);

            object Converter { get; }

            ConversionRank Rank { get; }
        }

        internal class ConversionData<T> : LanguagePrimitives.ConversionData
        {
            private readonly LanguagePrimitives.PSConverter<T> converter;
            private readonly ConversionRank rank;

            public ConversionData(LanguagePrimitives.PSConverter<T> converter, ConversionRank rank)
            {
                this.converter = converter;
                this.rank = rank;
            }

            public object Invoke(object valueToConvert, Type resultType, bool recurse, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
            {
                return this.converter(valueToConvert, resultType, recurse, originalValueToConvert, formatProvider, backupTable);
            }

            public object Converter
            {
                get
                {
                    return this.converter;
                }
            }

            public ConversionRank Rank
            {
                get
                {
                    return this.rank;
                }
            }
        }

        private class ConversionTypePair
        {
            private Type from;
            private Type to;

            internal ConversionTypePair(Type from, Type to)
            {
                this.from = from;
                this.to = to;
            }

            public override bool Equals(object other)
            {
                LanguagePrimitives.ConversionTypePair pair = other as LanguagePrimitives.ConversionTypePair;
                if (pair == null)
                {
                    return false;
                }
                return ((this.from == pair.from) && (this.to == pair.to));
            }

            public override int GetHashCode()
            {
                return (this.from.GetHashCode() + (0x25 * this.to.GetHashCode()));
            }
        }

        private class ConvertCheckingForCustomConverter
        {
            internal LanguagePrimitives.PSConverter<object> fallbackConverter;
            internal LanguagePrimitives.PSConverter<object> tryfirstConverter;

            internal object Convert(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
            {
                object result = null;
                if (this.tryfirstConverter != null)
                {
                    try
                    {
                        return this.tryfirstConverter(valueToConvert, resultType, recursion, originalValueToConvert, formatProvider, backupTable);
                    }
                    catch (InvalidCastException)
                    {
                    }
                }
                if (LanguagePrimitives.IsCustomTypeConversion(originalValueToConvert ?? valueToConvert, resultType, formatProvider, out result, backupTable))
                {
                    LanguagePrimitives.typeConversion.WriteLine("Custom Type Conversion succeeded.", new object[0]);
                    return result;
                }
                if (this.fallbackConverter == null)
                {
                    throw new PSInvalidCastException("ConvertToFinalInvalidCastException", null, ExtendedTypeSystem.InvalidCastException, new object[] { valueToConvert.ToString(), LanguagePrimitives.ObjectToTypeNameString(valueToConvert), resultType.ToString() });
                }
                return this.fallbackConverter(valueToConvert, resultType, recursion, originalValueToConvert, formatProvider, backupTable);
            }
        }

        private class ConvertViaCast
        {
            internal MethodInfo cast;

            internal object Convert(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
            {
                object obj2;
                try
                {
                    obj2 = this.cast.Invoke(null, new object[] { valueToConvert });
                }
                catch (TargetInvocationException exception)
                {
                    Exception innerException = (exception.InnerException == null) ? exception : exception.InnerException;
                    LanguagePrimitives.typeConversion.WriteLine("Cast operator exception: \"{0}\".", new object[] { innerException.Message });
                    throw new PSInvalidCastException("InvalidCastTargetInvocationException" + this.cast.Name, innerException, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), innerException.Message });
                }
                catch (Exception exception3)
                {
                    CommandProcessorBase.CheckForSevereException(exception3);
                    LanguagePrimitives.typeConversion.WriteLine("Cast operator exception: \"{0}\".", new object[] { exception3.Message });
                    throw new PSInvalidCastException("InvalidCastException" + this.cast.Name, exception3, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), exception3.Message });
                }
                return obj2;
            }
        }

        private class ConvertViaConstructor
        {
            internal Func<object, object> TargetCtorLambda;

            internal object Convert(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
            {
                object obj3;
                try
                {
                    object obj2 = this.TargetCtorLambda(valueToConvert);
                    LanguagePrimitives.typeConversion.WriteLine("Constructor result: \"{0}\".", new object[] { obj2 });
                    obj3 = obj2;
                }
                catch (TargetInvocationException exception)
                {
                    Exception innerException = (exception.InnerException == null) ? exception : exception.InnerException;
                    LanguagePrimitives.typeConversion.WriteLine("Exception invoking Constructor: \"{0}\".", new object[] { innerException.Message });
                    throw new PSInvalidCastException("InvalidCastConstructorTargetInvocationException", innerException, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), innerException.Message });
                }
                catch (Exception exception3)
                {
                    CommandProcessorBase.CheckForSevereException(exception3);
                    LanguagePrimitives.typeConversion.WriteLine("Exception invoking Constructor: \"{0}\".", new object[] { exception3.Message });
                    throw new PSInvalidCastException("InvalidCastConstructorException", exception3, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), exception3.Message });
                }
                return obj3;
            }
        }

        private class ConvertViaIEnumerableConstructor
        {
            internal Type ElementType;
            internal bool IsScalar;
            internal Func<int, IList> ListCtorLambda;
            internal Func<IList, object> TargetCtorLambda;

            internal object Convert(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
            {
                IList arg = null;
                object obj5;
                try
                {
                    int num = this.IsScalar ? 1 : ((Array) valueToConvert).Length;
                    arg = this.ListCtorLambda(num);
                }
                catch (Exception exception)
                {
                    CommandProcessorBase.CheckForSevereException(exception);
                    LanguagePrimitives.ThrowInvalidCastException(valueToConvert, resultType);
                    return null;
                }
                if (this.IsScalar)
                {
                    arg.Add(valueToConvert);
                }
                else
                {
                    Array array = (Array) valueToConvert;
                    foreach (object obj2 in array)
                    {
                        object obj3 = PSObject.Base(obj2);
                        if (!this.ElementType.IsAssignableFrom(obj3.GetType()))
                        {
                            LanguagePrimitives.ThrowInvalidCastException(valueToConvert, resultType);
                            return null;
                        }
                        arg.Add(obj3);
                    }
                }
                try
                {
                    object obj4 = this.TargetCtorLambda(arg);
                    LanguagePrimitives.typeConversion.WriteLine("IEnumerable Constructor result: \"{0}\".", new object[] { obj4 });
                    obj5 = obj4;
                }
                catch (TargetInvocationException exception2)
                {
                    Exception innerException = (exception2.InnerException == null) ? exception2 : exception2.InnerException;
                    LanguagePrimitives.typeConversion.WriteLine("Exception invoking IEnumerable Constructor: \"{0}\".", new object[] { innerException.Message });
                    throw new PSInvalidCastException("InvalidCastConstructorTargetInvocationException", innerException, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), innerException.Message });
                }
                catch (Exception exception4)
                {
                    CommandProcessorBase.CheckForSevereException(exception4);
                    LanguagePrimitives.typeConversion.WriteLine("Exception invoking IEnumerable Constructor: \"{0}\".", new object[] { exception4.Message });
                    throw new PSInvalidCastException("InvalidCastConstructorException", exception4, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), exception4.Message });
                }
                return obj5;
            }
        }

        private class ConvertViaNoArgumentConstructor
        {
            private readonly Func<object> constructor;

            internal ConvertViaNoArgumentConstructor(ConstructorInfo constructor, Type type)
            {
                NewExpression expr = (constructor != null) ? Expression.New(constructor) : Expression.New(type);
                this.constructor = Expression.Lambda<Func<object>>(expr.Cast(typeof(object)), new ParameterExpression[0]).Compile();
            }

            internal object Convert(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
            {
                object obj4;
                try
                {
                    ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
                    object o = null;
                    if ((executionContextFromTLS != null) && ((executionContextFromTLS == null) || (executionContextFromTLS.LanguageMode != PSLanguageMode.FullLanguage)))
                    {
                        throw InterpreterError.NewInterpreterException(valueToConvert, typeof(RuntimeException), null, "HashtableToObjectConversionNotSupportedInDataSection", ParserStrings.HashtableToObjectConversionNotSupportedInDataSection, new object[] { resultType.ToString() });
                    }
                    o = this.constructor();
                    PSObject psObject = valueToConvert as PSObject;
                    if (psObject != null)
                    {
                        LanguagePrimitives.SetObjectProperties(o, psObject, resultType, new LanguagePrimitives.MemberNotFoundError(LanguagePrimitives.CreateMemberNotFoundError), new LanguagePrimitives.MemberSetValueError(LanguagePrimitives.CreateMemberSetValueError));
                    }
                    else
                    {
                        IDictionary properties = valueToConvert as IDictionary;
                        LanguagePrimitives.SetObjectProperties(o, properties, resultType, new LanguagePrimitives.MemberNotFoundError(LanguagePrimitives.CreateMemberNotFoundError), new LanguagePrimitives.MemberSetValueError(LanguagePrimitives.CreateMemberSetValueError), false);
                    }
                    LanguagePrimitives.typeConversion.WriteLine("Constructor result: \"{0}\".", new object[] { o });
                    obj4 = o;
                }
                catch (TargetInvocationException exception2)
                {
                    Exception innerException = (exception2.InnerException == null) ? exception2 : exception2.InnerException;
                    LanguagePrimitives.typeConversion.WriteLine("Exception invoking Constructor: \"{0}\".", new object[] { innerException.Message });
                    throw new PSInvalidCastException("InvalidCastConstructorTargetInvocationException", innerException, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), innerException.Message });
                }
                catch (InvalidOperationException exception4)
                {
                    Exception exception5 = (exception4.InnerException == null) ? exception4 : exception4.InnerException;
                    throw new PSInvalidCastException("ObjectCreationError", exception4, ExtendedTypeSystem.ObjectCreationError, new object[] { resultType.ToString(), exception5.Message });
                }
                catch (SetValueException exception6)
                {
                    Exception exception7 = (exception6.InnerException == null) ? exception6 : exception6.InnerException;
                    throw new PSInvalidCastException("ObjectCreationError", exception7, ExtendedTypeSystem.ObjectCreationError, new object[] { resultType.ToString(), exception7.Message });
                }
                catch (RuntimeException exception8)
                {
                    Exception exception9 = (exception8.InnerException == null) ? exception8 : exception8.InnerException;
                    LanguagePrimitives.typeConversion.WriteLine("Exception invoking Constructor: \"{0}\".", new object[] { exception9.Message });
                    throw new PSInvalidCastException("InvalidCastConstructorException", exception9, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), exception9.Message });
                }
                catch (Exception exception10)
                {
                    CommandProcessorBase.CheckForSevereException(exception10);
                    LanguagePrimitives.typeConversion.WriteLine("Exception invoking Constructor: \"{0}\".", new object[] { exception10.Message });
                    throw new PSInvalidCastException("InvalidCastConstructorException", exception10, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), exception10.Message });
                }
                return obj4;
            }
        }

        private class ConvertViaParseMethod
        {
            internal MethodInfo parse;

            internal object ConvertWithCulture(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
            {
                object obj3;
                try
                {
                    object obj2 = this.parse.Invoke(null, new object[] { valueToConvert, formatProvider });
                    LanguagePrimitives.typeConversion.WriteLine("Parse result: {0}", new object[] { obj2 });
                    obj3 = obj2;
                }
                catch (TargetInvocationException exception)
                {
                    Exception innerException = (exception.InnerException == null) ? exception : exception.InnerException;
                    LanguagePrimitives.typeConversion.WriteLine("Exception calling Parse method with CultureInfo: \"{0}\".", new object[] { innerException.Message });
                    throw new PSInvalidCastException("InvalidCastParseTargetInvocationWithFormatProvider", innerException, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), innerException.Message });
                }
                catch (Exception exception3)
                {
                    CommandProcessorBase.CheckForSevereException(exception3);
                    LanguagePrimitives.typeConversion.WriteLine("Exception calling Parse method with CultureInfo: \"{0}\".", new object[] { exception3.Message });
                    throw new PSInvalidCastException("InvalidCastParseExceptionWithFormatProvider", exception3, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), exception3.Message });
                }
                return obj3;
            }

            internal object ConvertWithoutCulture(object valueToConvert, Type resultType, bool recursion, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
            {
                object obj3;
                try
                {
                    object obj2 = this.parse.Invoke(null, new object[] { valueToConvert });
                    LanguagePrimitives.typeConversion.WriteLine("Parse result: \"{0}\".", new object[] { obj2 });
                    obj3 = obj2;
                }
                catch (TargetInvocationException exception)
                {
                    Exception innerException = (exception.InnerException == null) ? exception : exception.InnerException;
                    LanguagePrimitives.typeConversion.WriteLine("Exception calling Parse method: \"{0}\".", new object[] { innerException.Message });
                    throw new PSInvalidCastException("InvalidCastParseTargetInvocation", innerException, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), innerException.Message });
                }
                catch (Exception exception3)
                {
                    CommandProcessorBase.CheckForSevereException(exception3);
                    LanguagePrimitives.typeConversion.WriteLine("Exception calling Parse method: \"{0}\".", new object[] { exception3.Message });
                    throw new PSInvalidCastException("InvalidCastParseException", exception3, ExtendedTypeSystem.InvalidCastExceptionWithInnerException, new object[] { valueToConvert.ToString(), resultType.ToString(), exception3.Message });
                }
                return obj3;
            }
        }

        private class EnumerableTWrapper : IEnumerable
        {
            private object _enumerable;
            private Type _enumerableType;
            private DynamicMethod _getEnumerator;

            internal EnumerableTWrapper(object enumerable, Type enumerableType)
            {
                this._enumerable = enumerable;
                this._enumerableType = enumerableType;
                this.CreateGetEnumerator();
            }

            private void CreateGetEnumerator()
            {
                this._getEnumerator = new DynamicMethod("GetEnumerator", typeof(object), new Type[] { typeof(object) }, typeof(LanguagePrimitives).Module, true);
                ILGenerator iLGenerator = this._getEnumerator.GetILGenerator();
                iLGenerator.Emit(OpCodes.Ldarg_0);
                iLGenerator.Emit(OpCodes.Castclass, this._enumerableType);
                MethodInfo method = this._enumerableType.GetMethod("GetEnumerator", new Type[0]);
                iLGenerator.Emit(OpCodes.Callvirt, method);
                iLGenerator.Emit(OpCodes.Ret);
            }

            public IEnumerator GetEnumerator()
            {
                return (IEnumerator) this._getEnumerator.Invoke(null, new object[] { this._enumerable });
            }
        }

        internal class EnumMultipleTypeConverter : LanguagePrimitives.EnumSingleTypeConverter
        {
            public override object ConvertFrom(object sourceValue, Type destinationType, IFormatProvider formatProvider, bool ignoreCase)
            {
                return LanguagePrimitives.EnumSingleTypeConverter.BaseConvertFrom(sourceValue, destinationType, formatProvider, ignoreCase, true);
            }
        }

        internal class EnumSingleTypeConverter : PSTypeConverter
        {
            private static HybridDictionary enumTable = new HybridDictionary();
            private const int maxEnumTableSize = 100;

            protected static object BaseConvertFrom(object sourceValue, Type destinationType, IFormatProvider formatProvider, bool ignoreCase, bool multipleValues)
            {
                string[] strArray;
                WildcardPattern[] patternArray;
                StringComparison ordinalIgnoreCase;
                string str = sourceValue as string;
                if (str == null)
                {
                    throw new PSInvalidCastException("InvalidCastEnumFromTypeNotAString", null, ExtendedTypeSystem.InvalidCastException, new object[] { sourceValue, LanguagePrimitives.ObjectToTypeNameString(sourceValue), destinationType });
                }
                if (str.Length == 0)
                {
                    throw new PSInvalidCastException("InvalidCastEnumFromEmptyString", null, ExtendedTypeSystem.InvalidCastException, new object[] { sourceValue, LanguagePrimitives.ObjectToTypeNameString(sourceValue), destinationType });
                }
                str = str.Trim();
                if (str.Length == 0)
                {
                    throw new PSInvalidCastException("InvalidEnumCastFromEmptyStringAfterTrim", null, ExtendedTypeSystem.InvalidCastException, new object[] { sourceValue, LanguagePrimitives.ObjectToTypeNameString(sourceValue), destinationType });
                }
                if ((char.IsDigit(str[0]) || (str[0] == '+')) || (str[0] == '-'))
                {
                    Type underlyingType = Enum.GetUnderlyingType(destinationType);
                    try
                    {
                        object enumValue = Enum.ToObject(destinationType, Convert.ChangeType(str, underlyingType, formatProvider));
                        ThrowForUndefinedEnum("UndefinedInEnumSingleTypeConverter", enumValue, str, destinationType);
                        return enumValue;
                    }
                    catch (Exception exception)
                    {
                        CommandProcessorBase.CheckForSevereException(exception);
                    }
                }
                if (!multipleValues)
                {
                    if (str.Contains(","))
                    {
                        throw new PSInvalidCastException("InvalidCastEnumCommaAndNoFlags", null, ExtendedTypeSystem.InvalidCastExceptionEnumerationNoFlagAndComma, new object[] { sourceValue, destinationType });
                    }
                    strArray = new string[] { str };
                    patternArray = new WildcardPattern[1];
                    if (WildcardPattern.ContainsWildcardCharacters(str))
                    {
                        patternArray[0] = new WildcardPattern(str, ignoreCase ? WildcardOptions.IgnoreCase : WildcardOptions.None);
                    }
                    else
                    {
                        patternArray[0] = null;
                    }
                }
                else
                {
                    strArray = str.Split(new char[] { ',' });
                    patternArray = new WildcardPattern[strArray.Length];
                    for (int j = 0; j < strArray.Length; j++)
                    {
                        string str2 = strArray[j];
                        if (WildcardPattern.ContainsWildcardCharacters(str2))
                        {
                            patternArray[j] = new WildcardPattern(str2, ignoreCase ? WildcardOptions.IgnoreCase : WildcardOptions.None);
                        }
                        else
                        {
                            patternArray[j] = null;
                        }
                    }
                }
                EnumHashEntry enumHashEntry = GetEnumHashEntry(destinationType);
                string[] names = enumHashEntry.names;
                Array values = enumHashEntry.values;
                ulong num2 = 0L;
                if (ignoreCase)
                {
                    ordinalIgnoreCase = StringComparison.OrdinalIgnoreCase;
                }
                else
                {
                    ordinalIgnoreCase = StringComparison.Ordinal;
                }
                for (int i = 0; i < strArray.Length; i++)
                {
                    string strA = strArray[i];
                    WildcardPattern pattern = patternArray[i];
                    bool flag = false;
                    for (int k = 0; k < names.Length; k++)
                    {
                        string input = names[k];
                        if (pattern != null)
                        {
                            if (pattern.IsMatch(input))
                            {
                                goto Label_024A;
                            }
                            continue;
                        }
                        if (string.Compare(strA, input, ordinalIgnoreCase) != 0)
                        {
                            continue;
                        }
                    Label_024A:
                        if (!multipleValues && flag)
                        {
                            object obj3 = Enum.ToObject(destinationType, num2);
                            object obj4 = Enum.ToObject(destinationType, ((IConvertible) values.GetValue(i)).ToUInt64(null));
                            throw new PSInvalidCastException("InvalidCastEnumTwoStringsFoundAndNoFlags", null, ExtendedTypeSystem.InvalidCastExceptionEnumerationMoreThanOneValue, new object[] { sourceValue, destinationType, obj3, obj4 });
                        }
                        flag = true;
                        num2 |= ((IConvertible) values.GetValue(k)).ToUInt64(null);
                    }
                    if (!flag)
                    {
                        throw new PSInvalidCastException("InvalidCastEnumStringNotFound", null, ExtendedTypeSystem.InvalidCastExceptionEnumerationNoValue, new object[] { strA, destinationType, EnumValues(destinationType) });
                    }
                }
                return Enum.ToObject(destinationType, num2);
            }

            public override bool CanConvertFrom(object sourceValue, Type destinationType)
            {
                return (((sourceValue != null) && (sourceValue is string)) && destinationType.IsEnum);
            }

            public override bool CanConvertTo(object sourceValue, Type destinationType)
            {
                return false;
            }

            public override object ConvertFrom(object sourceValue, Type destinationType, IFormatProvider formatProvider, bool ignoreCase)
            {
                return BaseConvertFrom(sourceValue, destinationType, formatProvider, ignoreCase, false);
            }

            public override object ConvertTo(object sourceValue, Type destinationType, IFormatProvider formatProvider, bool ignoreCase)
            {
                throw PSTraceSource.NewNotSupportedException();
            }

            internal static string EnumValues(Type enumType)
            {
                string[] names = GetEnumHashEntry(enumType).names;
                string listSeparator = ExtendedTypeSystem.ListSeparator;
                StringBuilder builder = new StringBuilder();
                if (names.Length != 0)
                {
                    for (int i = 0; i < names.Length; i++)
                    {
                        builder.Append(names[i]);
                        builder.Append(listSeparator);
                    }
                    builder.Remove(builder.Length - listSeparator.Length, listSeparator.Length);
                }
                return builder.ToString();
            }

            private static EnumHashEntry GetEnumHashEntry(Type enumType)
            {
                lock (enumTable)
                {
                    EnumHashEntry entry = (EnumHashEntry) enumTable[enumType];
                    if (entry == null)
                    {
                        if (enumTable.Count == 100)
                        {
                            enumTable.Clear();
                        }
                        ulong allValues = 0L;
                        bool hasNegativeValue = false;
                        Array values = Enum.GetValues(enumType);
                        if (LanguagePrimitives.IsSignedInteger(Type.GetTypeCode(enumType)))
                        {
                            foreach (IConvertible convertible in values)
                            {
                                if (convertible.ToInt64(null) < 0L)
                                {
                                    hasNegativeValue = true;
                                    break;
                                }
                                allValues |= convertible.ToUInt64(null);
                            }
                        }
                        else
                        {
                            foreach (IConvertible convertible2 in values)
                            {
                                allValues |= convertible2.ToUInt64(null);
                            }
                        }
                        bool hasFlagsAttribute = enumType.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0;
                        entry = new EnumHashEntry(Enum.GetNames(enumType), values, allValues, hasNegativeValue, hasFlagsAttribute);
                        enumTable.Add(enumType, entry);
                    }
                    return entry;
                }
            }

            private static bool IsDefinedEnum(object enumValue, Type enumType)
            {
                if (enumValue == null)
                {
                    return false;
                }
                EnumHashEntry enumHashEntry = GetEnumHashEntry(enumType);
                if (enumHashEntry.hasNegativeValue)
                {
                    return true;
                }
                IConvertible convertible = (IConvertible) enumValue;
                if (LanguagePrimitives.IsSignedInteger(Type.GetTypeCode(enumValue.GetType())) && (convertible.ToInt64(null) < 0L))
                {
                    return false;
                }
                ulong num2 = convertible.ToUInt64(null);
                if (enumHashEntry.hasFlagsAttribute)
                {
                    return (((num2 | enumHashEntry.allValues) ^ enumHashEntry.allValues) == 0L);
                }
                return (Array.IndexOf(enumHashEntry.values, enumValue) >= 0);
            }

            internal static void ThrowForUndefinedEnum(string errorId, object enumValue, Type enumType)
            {
                ThrowForUndefinedEnum(errorId, enumValue, enumValue, enumType);
            }

            internal static void ThrowForUndefinedEnum(string errorId, object enumValue, object valueToUseToThrow, Type enumType)
            {
                if (!IsDefinedEnum(enumValue, enumType))
                {
                    LanguagePrimitives.typeConversion.WriteLine("Value {0} is not defined in the Enum {1}.", new object[] { valueToUseToThrow, enumType });
                    throw new PSInvalidCastException(errorId, null, ExtendedTypeSystem.InvalidCastExceptionEnumerationNoValue, new object[] { valueToUseToThrow, enumType, EnumValues(enumType) });
                }
            }

            private class EnumHashEntry
            {
                internal ulong allValues;
                internal bool hasFlagsAttribute;
                internal bool hasNegativeValue;
                internal string[] names;
                internal Array values;

                internal EnumHashEntry(string[] names, Array values, ulong allValues, bool hasNegativeValue, bool hasFlagsAttribute)
                {
                    this.names = names;
                    this.values = values;
                    this.allValues = allValues;
                    this.hasNegativeValue = hasNegativeValue;
                    this.hasFlagsAttribute = hasFlagsAttribute;
                }
            }
        }

        private delegate IEnumerable GetEnumerableDelegate(object obj);

        internal class InternalPSCustomObject
        {
        }

        internal class InternalPSObject : PSObject
        {
        }

        internal delegate void MemberNotFoundError(PSObject pso, DictionaryEntry property, Type resultType);

        internal delegate void MemberSetValueError(SetValueException e);

        internal class Null
        {
        }

        internal delegate T PSConverter<T>(object valueToConvert, Type resultType, bool recurse, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable);

        internal delegate object PSNullConverter(object nullOrAutomationNull);

        [Flags]
        private enum TypeCodeTraits
        {
            CimIntrinsicType = 8,
            Decimal = 0x10,
            Floating = 4,
            Integer = 3,
            None = 0,
            Numeric = 0x17,
            SignedInteger = 1,
            UnsignedInteger = 2
        }
    }
}

