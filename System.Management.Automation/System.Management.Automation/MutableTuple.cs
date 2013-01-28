namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Management.Automation.Language;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal abstract class MutableTuple
    {
        private Dictionary<string, int> _nameToIndexMap;
        private int _size;
        private static readonly Dictionary<Type, int> _sizeDict = new Dictionary<Type, int>();
        protected BitArray _valuesSet;
        private const int MaxSize = 0x80;

        protected MutableTuple()
        {
        }

        public static Expression Create(params Expression[] values)
        {
            return CreateNew(MakeTupleType(Array.ConvertAll<Expression, Type>(values, x => x.Type)), 0, values.Length, values);
        }

        internal static Expression CreateNew(Type tupleType, int start, int end, Expression[] values)
        {
            Expression[] expressionArray;
            int num = end - start;
            if (num > 0x80)
            {
                int num2 = 1;
                while (num > 0x80)
                {
                    num = ((num + 0x80) - 1) / 0x80;
                    num2 *= 0x80;
                }
                expressionArray = new Expression[PowerOfTwoRound(num)];
                for (int i = 0; i < num; i++)
                {
                    int num4 = start + (i * num2);
                    int num5 = Math.Min(end, start + ((i + 1) * num2));
                    PropertyInfo property = tupleType.GetProperty("Item" + string.Format(CultureInfo.InvariantCulture, "{0:D3}", new object[] { i }));
                    expressionArray[i] = CreateNew(property.PropertyType, num4, num5, values);
                }
                for (int j = num; j < expressionArray.Length; j++)
                {
                    expressionArray[j] = Expression.Constant(null, typeof(LanguagePrimitives.Null));
                }
            }
            else
            {
                expressionArray = new Expression[PowerOfTwoRound(num)];
                for (int k = 0; k < num; k++)
                {
                    expressionArray[k] = values[k + start];
                }
                for (int m = num; m < expressionArray.Length; m++)
                {
                    expressionArray[m] = Expression.Constant(null, typeof(LanguagePrimitives.Null));
                }
            }
            return Expression.New(tupleType.GetConstructor(Array.ConvertAll<Expression, Type>(expressionArray, x => x.Type)), expressionArray);
        }

        internal static IEnumerable<int> GetAccessPath(int size, int index)
        {
            int iteratorVariable0 = 0;
            int iteratorVariable1 = 0x7f;
            int iteratorVariable2 = 1;
            int iteratorVariable3 = size;
            while (iteratorVariable3 > 0x80)
            {
                iteratorVariable0++;
                iteratorVariable3 /= 0x80;
                iteratorVariable1 *= 0x80;
                iteratorVariable2 *= 0x80;
            }
            while (iteratorVariable0-- >= 0)
            {
                int iteratorVariable4 = (index & iteratorVariable1) / iteratorVariable2;
                yield return iteratorVariable4;
                iteratorVariable1 /= 0x80;
                iteratorVariable2 /= 0x80;
            }
        }

        public static IEnumerable<PropertyInfo> GetAccessPath(Type tupleType, int index)
        {
            return GetAccessProperties(tupleType, GetSize(tupleType), index);
        }

        internal static IEnumerable<PropertyInfo> GetAccessProperties(Type tupleType, int size, int index)
        {
            if ((index < 0) || (index >= size))
            {
                throw new ArgumentException("index");
            }
            foreach (int iteratorVariable0 in GetAccessPath(size, index))
            {
                PropertyInfo property = tupleType.GetProperty("Item" + string.Format(CultureInfo.InvariantCulture, "{0:D3}", new object[] { iteratorVariable0 }));
                yield return property;
                tupleType = property.PropertyType;
            }
        }

        internal object GetAutomaticVariable(AutomaticVariable auto)
        {
            return this.GetValue((int) auto);
        }

        private object GetNestedValue(int size, int index)
        {
            if (size < 0x80)
            {
                return this.GetValueImpl(index);
            }
            object valueImpl = this;
            foreach (int num in GetAccessPath(size, index))
            {
                valueImpl = ((MutableTuple) valueImpl).GetValueImpl(num);
            }
            return valueImpl;
        }

        public static int GetSize(Type tupleType)
        {
            int num = 0;
            lock (_sizeDict)
            {
                if (_sizeDict.TryGetValue(tupleType, out num))
                {
                    return num;
                }
            }
            Stack<Type> stack = new Stack<Type>(tupleType.GetGenericArguments());
            while (stack.Count != 0)
            {
                Type c = stack.Pop();
                if (typeof(MutableTuple).IsAssignableFrom(c))
                {
                    foreach (Type type2 in c.GetGenericArguments())
                    {
                        stack.Push(type2);
                    }
                }
                else if (!(c == typeof(LanguagePrimitives.Null)))
                {
                    num++;
                }
            }
            lock (_sizeDict)
            {
                _sizeDict[tupleType] = num;
            }
            return num;
        }

        private static Type GetTupleType(int size)
        {
            if (size > 0x80)
            {
                return null;
            }
            if (size <= 1)
            {
                return typeof(MutableTuple<>);
            }
            if (size <= 2)
            {
                return typeof(MutableTuple<,>);
            }
            if (size <= 4)
            {
                return typeof(MutableTuple<,,,>);
            }
            if (size <= 8)
            {
                return typeof(MutableTuple<,,,,,,,>);
            }
            if (size <= 0x10)
            {
                return typeof(MutableTuple<,,,,,,,,,,,,,,,>);
            }
            if (size <= 0x20)
            {
                return typeof(MutableTuple<,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,>);
            }
            if (size <= 0x40)
            {
                return typeof(MutableTuple<,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,>);
            }
            return typeof(MutableTuple<,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,>);
        }

        public static object[] GetTupleValues(MutableTuple tuple)
        {
            List<object> args = new List<object>();
            GetTupleValues(tuple, args);
            return args.ToArray();
        }

        private static void GetTupleValues(MutableTuple tuple, List<object> args)
        {
            Type[] genericArguments = tuple.GetType().GetGenericArguments();
            for (int i = 0; i < genericArguments.Length; i++)
            {
                if (typeof(MutableTuple).IsAssignableFrom(genericArguments[i]))
                {
                    GetTupleValues((MutableTuple) tuple.GetValue(i), args);
                }
                else if (genericArguments[i] != typeof(LanguagePrimitives.Null))
                {
                    args.Add(tuple.GetValue(i));
                }
            }
        }

        public object GetValue(int index)
        {
            return this.GetNestedValue(this._size, index);
        }

        protected abstract object GetValueImpl(int index);
        internal void GetVariableTable(Dictionary<string, PSVariable> result, bool includePrivate)
        {
            string[] strArray = (from keyValuePairs in this._nameToIndexMap
                orderby keyValuePairs.Value
                select keyValuePairs.Key).ToArray<string>();
            for (int i = 0; i < strArray.Length; i++)
            {
                string key = strArray[i];
                if (this.IsValueSet(i) && !result.ContainsKey(key))
                {
                    result.Add(key, new LocalVariable(key, this, i));
                    if (key.Equals("_", StringComparison.OrdinalIgnoreCase))
                    {
                        result.Add("PSItem", new LocalVariable("PSItem", this, i));
                    }
                }
            }
        }

        internal bool IsValueSet(int index)
        {
            if (this._size < 0x80)
            {
                return this._valuesSet[index];
            }
            MutableTuple valueImpl = this;
            int[] source = GetAccessPath(this._size, index).ToArray<int>();
            for (int i = 0; i < (source.Length - 1); i++)
            {
                valueImpl = (MutableTuple) valueImpl.GetValueImpl(source[i]);
            }
            return valueImpl._valuesSet[source.Last<int>()];
        }

        public static MutableTuple MakeTuple(Type tupleType, Dictionary<string, int> nameToIndexMap)
        {
            int size = GetSize(tupleType);
            BitArray bitArray = new BitArray(size);
            MutableTuple tuple = MakeTuple(tupleType, size, bitArray);
            tuple._nameToIndexMap = nameToIndexMap;
            return tuple;
        }

        private static MutableTuple MakeTuple(Type tupleType, int size, BitArray bitArray)
        {
            MutableTuple tuple = (MutableTuple) Activator.CreateInstance(tupleType);
            tuple._size = size;
            tuple._valuesSet = bitArray;
            if (size > 0x80)
            {
                while (size > 0x80)
                {
                    size = ((size + 0x80) - 1) / 0x80;
                }
                for (int i = 0; i < size; i++)
                {
                    PropertyInfo property = tupleType.GetProperty("Item" + string.Format(CultureInfo.InvariantCulture, "{0:D3}", new object[] { i }));
                    tuple.SetValueImpl(i, MakeTuple(property.PropertyType, null));
                }
            }
            return tuple;
        }

        public static Type MakeTupleType(params Type[] types)
        {
            return MakeTupleType(types, 0, types.Length);
        }

        private static Type MakeTupleType(Type[] types, int start, int end)
        {
            int size = end - start;
            Type tupleType = GetTupleType(size);
            if (tupleType != null)
            {
                Type[] typeArray = new Type[tupleType.GetGenericArguments().Length];
                int num2 = 0;
                for (int k = start; k < end; k++)
                {
                    typeArray[num2++] = types[k];
                }
                while (num2 < typeArray.Length)
                {
                    typeArray[num2++] = typeof(LanguagePrimitives.Null);
                }
                return tupleType.MakeGenericType(typeArray);
            }
            int num4 = 1;
            while (size > 0x80)
            {
                size = ((size + 0x80) - 1) / 0x80;
                num4 *= 0x80;
            }
            tupleType = GetTupleType(size);
            Type[] typeArguments = new Type[tupleType.GetGenericArguments().Length];
            for (int i = 0; i < size; i++)
            {
                int num6 = start + (i * num4);
                int num7 = Math.Min(end, start + ((i + 1) * num4));
                typeArguments[i] = MakeTupleType(types, num6, num7);
            }
            for (int j = size; j < typeArguments.Length; j++)
            {
                typeArguments[j] = typeof(LanguagePrimitives.Null);
            }
            return tupleType.MakeGenericType(typeArguments);
        }

        private static int PowerOfTwoRound(int value)
        {
            int num = 1;
            while (value > num)
            {
                num = num << 1;
            }
            return num;
        }

        internal void SetAutomaticVariable(AutomaticVariable auto, object value, System.Management.Automation.ExecutionContext context)
        {
            if (context._debuggingMode > 0)
            {
                context.Debugger.CheckVariableWrite(SpecialVariables.AutomaticVariables[(int) auto]);
            }
            this.SetValue((int) auto, value);
        }

        private void SetNestedValue(int size, int index, object value)
        {
            if (size < 0x80)
            {
                this.SetValueImpl(index, value);
            }
            else
            {
                MutableTuple valueImpl = this;
                int num = -1;
                foreach (int num2 in GetAccessPath(size, index))
                {
                    if (num != -1)
                    {
                        valueImpl = (MutableTuple) valueImpl.GetValueImpl(num);
                    }
                    num = num2;
                }
                valueImpl.SetValueImpl(num, value);
            }
        }

        internal void SetPreferenceVariable(PreferenceVariable pref, object value)
        {
            this.SetValue((int) pref, value);
        }

        public void SetValue(int index, object value)
        {
            this.SetNestedValue(this._size, index, value);
        }

        protected abstract void SetValueImpl(int index, object value);
        internal bool TryGetLocalVariable(string name, bool fromNewOrSet, out PSVariable result)
        {
            int num;
            name = VariableAnalysis.GetUnaliasedVariableName(name);
            if (this._nameToIndexMap.TryGetValue(name, out num) && (fromNewOrSet || this.IsValueSet(num)))
            {
                result = new LocalVariable(name, this, num);
                return true;
            }
            result = null;
            return false;
        }

        internal bool TrySetParameter(string name, object value)
        {
            int num;
            name = VariableAnalysis.GetUnaliasedVariableName(name);
            if (this._nameToIndexMap.TryGetValue(name, out num))
            {
                this.SetValue(num, value);
                return true;
            }
            return false;
        }

        internal PSVariable TrySetVariable(string name, object value)
        {
            int num;
            name = VariableAnalysis.GetUnaliasedVariableName(name);
            if (this._nameToIndexMap.TryGetValue(name, out num))
            {
                this.SetValue(num, value);
                return new LocalVariable(name, this, num);
            }
            return null;
        }

        public abstract int Capacity { get; }

        
    }
}

