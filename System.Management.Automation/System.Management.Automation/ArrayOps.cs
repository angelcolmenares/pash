namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;

    internal static class ArrayOps
    {
        internal static object GetMDArrayValue(Array array, int[] indexes, bool slicing)
        {
            if (array.Rank != indexes.Length)
            {
                ReportIndexingError(array, indexes, null);
            }
            for (int i = 0; i < indexes.Length; i++)
            {
                int upperBound = array.GetUpperBound(i);
                int lowerBound = array.GetLowerBound(i);
                if (indexes[i] < lowerBound)
                {
                    indexes[i] = (indexes[i] + upperBound) + 1;
                }
                if ((indexes[i] < lowerBound) || (indexes[i] > upperBound))
                {
                    ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
                    if ((executionContextFromTLS != null) && !executionContextFromTLS.IsStrictVersion(3))
                    {
                        if (!slicing)
                        {
                            return null;
                        }
                        return AutomationNull.Value;
                    }
                }
            }
            return array.GetValue(indexes);
        }

        internal static object GetMDArrayValueOrSlice(Array array, object indexes)
        {
            Exception reason = null;
            int[] numArray = null;
            try
            {
                numArray = (int[]) LanguagePrimitives.ConvertTo(indexes, typeof(int[]), NumberFormatInfo.InvariantInfo);
            }
            catch (InvalidCastException exception2)
            {
                reason = exception2;
            }
            if (numArray != null)
            {
                if (numArray.Length != array.Rank)
                {
                    ReportIndexingError(array, indexes, null);
                }
                return GetMDArrayValue(array, numArray, false);
            }
            List<int[]> list = new List<int[]>();
            IEnumerator enumerator = LanguagePrimitives.GetEnumerator(indexes);
            while (EnumerableOps.MoveNext(null, enumerator))
            {
                object valueToConvert = EnumerableOps.Current(enumerator);
                try
                {
                    numArray = LanguagePrimitives.ConvertTo<int[]>(valueToConvert);
                }
                catch (InvalidCastException)
                {
                    numArray = null;
                }
                if ((numArray == null) || (numArray.Length != array.Rank))
                {
                    if (reason != null)
                    {
                        ReportIndexingError(array, indexes, reason);
                    }
                    ReportIndexingError(array, valueToConvert, null);
                }
                reason = null;
                list.Add(numArray);
            }
            object[] sourceArray = new object[list.Count];
            int length = 0;
            foreach (int[] numArray2 in list)
            {
                object obj3 = GetMDArrayValue(array, numArray2, true);
                if (obj3 != AutomationNull.Value)
                {
                    sourceArray[length++] = obj3;
                }
            }
            if (length != list.Count)
            {
                object[] destinationArray = new object[length];
                Array.Copy(sourceArray, destinationArray, length);
                return destinationArray;
            }
            return sourceArray;
        }

        internal static object GetNonIndexable(object target, object[] indices)
        {
            if (indices.Length == 1)
            {
                object second = indices[0];
                if ((second != null) && (LanguagePrimitives.Equals(0, second) || LanguagePrimitives.Equals(-1, second)))
                {
                    return target;
                }
            }
            ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
            if ((executionContextFromTLS != null) && executionContextFromTLS.IsStrictVersion(2))
            {
                throw InterpreterError.NewInterpreterException(target, typeof(RuntimeException), null, "CannotIndex", ParserStrings.CannotIndex, new object[] { target.GetType() });
            }
            return AutomationNull.Value;
        }

        private static string IndexStringMessage(object index)
        {
            string str = PSObject.ToString(null, index, ",", null, null, true, true);
            if (str.Length > 20)
            {
                str = str.Substring(0, 20) + " ...";
            }
            return str;
        }

        internal static T[] Multiply<T>(T[] array, int times)
        {
            if (times == 1)
            {
                return array;
            }
            if ((times == 0) || (array.Length == 0))
            {
                return new T[0];
            }
            ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
            if (((executionContextFromTLS != null) && (executionContextFromTLS.LanguageMode == PSLanguageMode.RestrictedLanguage)) && ((array.Length * times) > 0x400L))
            {
                throw InterpreterError.NewInterpreterException(times, typeof(RuntimeException), null, "ArrayMultiplyToolongInDataSection", ParserStrings.ArrayMultiplyToolongInDataSection, new object[] { 0x400 });
            }
            long valueToConvert = array.Length * times;
            int num2 = -1;
            try
            {
                num2 = (int) valueToConvert;
            }
            catch (OverflowException)
            {
                LanguagePrimitives.ThrowInvalidCastException(valueToConvert, typeof(int));
            }
            T[] destinationArray = new T[num2];
            int length = array.Length;
            Array.Copy(array, 0, destinationArray, 0, length);
            times = times >> 1;
            while (times != 0)
            {
                Array.Copy(destinationArray, 0, destinationArray, length, length);
                length *= 2;
                times = times >> 1;
            }
            if (destinationArray.Length != length)
            {
                Array.Copy(destinationArray, 0, destinationArray, length, destinationArray.Length - length);
            }
            return destinationArray;
        }

        private static void ReportIndexingError(Array array, object index, Exception reason)
        {
            string str = IndexStringMessage(index);
            if (reason == null)
            {
                throw InterpreterError.NewInterpreterException(index, typeof(RuntimeException), null, "NeedMultidimensionalIndex", ParserStrings.NeedMultidimensionalIndex, new object[] { array.Rank, str });
            }
            throw InterpreterError.NewInterpreterExceptionWithInnerException(index, typeof(RuntimeException), null, "NeedMultidimensionalIndex", ParserStrings.NeedMultidimensionalIndex, reason, new object[] { array.Rank, str });
        }

        internal static object SetMDArrayValue(Array array, int[] indexes, object value)
        {
            if (array.Rank != indexes.Length)
            {
                ReportIndexingError(array, indexes, null);
            }
            for (int i = 0; i < indexes.Length; i++)
            {
                int upperBound = array.GetUpperBound(i);
                int lowerBound = array.GetLowerBound(i);
                if (indexes[i] < lowerBound)
                {
                    indexes[i] = (indexes[i] + upperBound) + 1;
                }
            }
            array.SetValue(value, indexes);
            return value;
        }

        internal static object[] SlicingIndex(object target, object[] indexes, Func<object, object, object> indexer)
        {
            object[] sourceArray = new object[indexes.Length];
            int length = 0;
            foreach (object obj2 in indexes)
            {
                object obj3 = indexer(target, obj2);
                if (obj3 != AutomationNull.Value)
                {
                    sourceArray[length++] = obj3;
                }
            }
            if (length != indexes.Length)
            {
                object[] destinationArray = new object[length];
                Array.Copy(sourceArray, destinationArray, length);
                return destinationArray;
            }
            return sourceArray;
        }
    }
}

