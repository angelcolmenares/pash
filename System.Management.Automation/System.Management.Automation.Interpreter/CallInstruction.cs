namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;

    internal abstract class CallInstruction : Instruction
    {
        private static readonly Dictionary<MethodInfo, CallInstruction> _cache = new Dictionary<MethodInfo, CallInstruction>();
        private const int MaxArgs = 3;
        private const int MaxHelpers = 10;

        internal CallInstruction()
        {
        }

        public static void ArrayItemSetter1(Array array, int index0, object value)
        {
            array.SetValue(value, index0);
        }

        public static void ArrayItemSetter2(Array array, int index0, int index1, object value)
        {
            array.SetValue(value, index0, index1);
        }

        public static void ArrayItemSetter3(Array array, int index0, int index1, int index2, object value)
        {
            array.SetValue(value, index0, index1, index2);
        }

        public static MethodInfo CacheAction<T0>(Action<T0> method)
        {
            MethodInfo info = method.Method;
            lock (_cache)
            {
                _cache[info] = new ActionCallInstruction<T0>(method);
            }
            return info;
        }

        public static MethodInfo CacheAction<T0, T1>(Action<T0, T1> method)
        {
            MethodInfo info = method.Method;
            lock (_cache)
            {
                _cache[info] = new ActionCallInstruction<T0, T1>(method);
            }
            return info;
        }

        public static MethodInfo CacheAction<T0, T1, T2>(Action<T0, T1, T2> method)
        {
            MethodInfo info = method.Method;
            lock (_cache)
            {
                _cache[info] = new ActionCallInstruction<T0, T1, T2>(method);
            }
            return info;
        }

        public static MethodInfo CacheAction<T0, T1, T2, T3>(Action<T0, T1, T2, T3> method)
        {
            MethodInfo info = method.Method;
            lock (_cache)
            {
                _cache[info] = new ActionCallInstruction<T0, T1, T2, T3>(method);
            }
            return info;
        }

        public static MethodInfo CacheAction<T0, T1, T2, T3, T4>(Action<T0, T1, T2, T3, T4> method)
        {
            MethodInfo info = method.Method;
            lock (_cache)
            {
                _cache[info] = new ActionCallInstruction<T0, T1, T2, T3, T4>(method);
            }
            return info;
        }

        public static MethodInfo CacheAction<T0, T1, T2, T3, T4, T5>(Action<T0, T1, T2, T3, T4, T5> method)
        {
            MethodInfo info = method.Method;
            lock (_cache)
            {
                _cache[info] = new ActionCallInstruction<T0, T1, T2, T3, T4, T5>(method);
            }
            return info;
        }

        public static MethodInfo CacheAction<T0, T1, T2, T3, T4, T5, T6>(Action<T0, T1, T2, T3, T4, T5, T6> method)
        {
            MethodInfo info = method.Method;
            lock (_cache)
            {
                _cache[info] = new ActionCallInstruction<T0, T1, T2, T3, T4, T5, T6>(method);
            }
            return info;
        }

        public static MethodInfo CacheAction<T0, T1, T2, T3, T4, T5, T6, T7>(Action<T0, T1, T2, T3, T4, T5, T6, T7> method)
        {
            MethodInfo info = method.Method;
            lock (_cache)
            {
                _cache[info] = new ActionCallInstruction<T0, T1, T2, T3, T4, T5, T6, T7>(method);
            }
            return info;
        }

        public static MethodInfo CacheAction<T0, T1, T2, T3, T4, T5, T6, T7, T8>(Action<T0, T1, T2, T3, T4, T5, T6, T7, T8> method)
        {
            MethodInfo info = method.Method;
            lock (_cache)
            {
                _cache[info] = new ActionCallInstruction<T0, T1, T2, T3, T4, T5, T6, T7, T8>(method);
            }
            return info;
        }

        public static MethodInfo CacheAction(Action method)
        {
            MethodInfo info = method.Method;
            lock (_cache)
            {
                _cache[info] = new ActionCallInstruction(method);
            }
            return info;
        }

        public static MethodInfo CacheFunc<TRet>(Func<TRet> method)
        {
            MethodInfo info = method.Method;
            lock (_cache)
            {
                _cache[info] = new FuncCallInstruction<TRet>(method);
            }
            return info;
        }

        public static MethodInfo CacheFunc<T0, TRet>(Func<T0, TRet> method)
        {
            MethodInfo info = method.Method;
            lock (_cache)
            {
                _cache[info] = new FuncCallInstruction<T0, TRet>(method);
            }
            return info;
        }

        public static MethodInfo CacheFunc<T0, T1, TRet>(Func<T0, T1, TRet> method)
        {
            MethodInfo info = method.Method;
            lock (_cache)
            {
                _cache[info] = new FuncCallInstruction<T0, T1, TRet>(method);
            }
            return info;
        }

        public static MethodInfo CacheFunc<T0, T1, T2, TRet>(Func<T0, T1, T2, TRet> method)
        {
            MethodInfo info = method.Method;
            lock (_cache)
            {
                _cache[info] = new FuncCallInstruction<T0, T1, T2, TRet>(method);
            }
            return info;
        }

        public static MethodInfo CacheFunc<T0, T1, T2, T3, TRet>(Func<T0, T1, T2, T3, TRet> method)
        {
            MethodInfo info = method.Method;
            lock (_cache)
            {
                _cache[info] = new FuncCallInstruction<T0, T1, T2, T3, TRet>(method);
            }
            return info;
        }

        public static MethodInfo CacheFunc<T0, T1, T2, T3, T4, TRet>(Func<T0, T1, T2, T3, T4, TRet> method)
        {
            MethodInfo info = method.Method;
            lock (_cache)
            {
                _cache[info] = new FuncCallInstruction<T0, T1, T2, T3, T4, TRet>(method);
            }
            return info;
        }

        public static MethodInfo CacheFunc<T0, T1, T2, T3, T4, T5, TRet>(Func<T0, T1, T2, T3, T4, T5, TRet> method)
        {
            MethodInfo info = method.Method;
            lock (_cache)
            {
                _cache[info] = new FuncCallInstruction<T0, T1, T2, T3, T4, T5, TRet>(method);
            }
            return info;
        }

        public static MethodInfo CacheFunc<T0, T1, T2, T3, T4, T5, T6, TRet>(Func<T0, T1, T2, T3, T4, T5, T6, TRet> method)
        {
            MethodInfo info = method.Method;
            lock (_cache)
            {
                _cache[info] = new FuncCallInstruction<T0, T1, T2, T3, T4, T5, T6, TRet>(method);
            }
            return info;
        }

        public static MethodInfo CacheFunc<T0, T1, T2, T3, T4, T5, T6, T7, TRet>(Func<T0, T1, T2, T3, T4, T5, T6, T7, TRet> method)
        {
            MethodInfo info = method.Method;
            lock (_cache)
            {
                _cache[info] = new FuncCallInstruction<T0, T1, T2, T3, T4, T5, T6, T7, TRet>(method);
            }
            return info;
        }

        public static MethodInfo CacheFunc<T0, T1, T2, T3, T4, T5, T6, T7, T8, TRet>(Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, TRet> method)
        {
            MethodInfo info = method.Method;
            lock (_cache)
            {
                _cache[info] = new FuncCallInstruction<T0, T1, T2, T3, T4, T5, T6, T7, T8, TRet>(method);
            }
            return info;
        }

        public static CallInstruction Create(MethodInfo info)
        {
            return Create(info, info.GetParameters());
        }

        public static CallInstruction Create(MethodInfo info, ParameterInfo[] parameters)
        {
            CallInstruction instruction;
            int length = parameters.Length;
            if (!info.IsStatic)
            {
                length++;
            }
            if (((info.DeclaringType != null) && info.DeclaringType.IsArray) && ((info.Name == "Get") || (info.Name == "Set")))
            {
                return GetArrayAccessor(info, length);
            }
            if ((info is DynamicMethod) || (!info.IsStatic && info.DeclaringType.IsValueType))
            {
                return new MethodInfoCallInstruction(info, length);
            }
            if (length >= 10)
            {
                return new MethodInfoCallInstruction(info, length);
            }
            foreach (ParameterInfo info2 in parameters)
            {
                if (info2.ParameterType.IsByRef)
                {
                    return new MethodInfoCallInstruction(info, length);
                }
            }
            if (ShouldCache(info))
            {
                lock (_cache)
                {
                    if (_cache.TryGetValue(info, out instruction))
                    {
                        return instruction;
                    }
                }
            }
            try
            {
                if (length < 3)
                {
                    instruction = FastCreate(info, parameters);
                }
                else
                {
                    instruction = SlowCreate(info, parameters);
                }
            }
            catch (TargetInvocationException exception)
            {
                if (!(exception.InnerException is NotSupportedException))
                {
                    throw;
                }
                instruction = new MethodInfoCallInstruction(info, length);
            }
            catch (NotSupportedException)
            {
                instruction = new MethodInfoCallInstruction(info, length);
            }
            if (ShouldCache(info))
            {
                lock (_cache)
                {
                    _cache[info] = instruction;
                }
            }
            return instruction;
        }

        private static CallInstruction FastCreate(MethodInfo target, ParameterInfo[] pi)
        {
            Type type = TryGetParameterOrReturnType(target, pi, 0);
            if (type == null)
            {
                return new ActionCallInstruction(target);
            }
            if (!type.IsEnum)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Object:
                        if ((type != typeof(object)) && (IndexIsNotReturnType(0, target, pi) || type.IsValueType))
                        {
                            break;
                        }
                        return FastCreate<object>(target, pi);

                    case TypeCode.Boolean:
                        return FastCreate<bool>(target, pi);

                    case TypeCode.Char:
                        return FastCreate<char>(target, pi);

                    case TypeCode.SByte:
                        return FastCreate<sbyte>(target, pi);

                    case TypeCode.Byte:
                        return FastCreate<byte>(target, pi);

                    case TypeCode.Int16:
                        return FastCreate<short>(target, pi);

                    case TypeCode.UInt16:
                        return FastCreate<ushort>(target, pi);

                    case TypeCode.Int32:
                        return FastCreate<int>(target, pi);

                    case TypeCode.UInt32:
                        return FastCreate<int>(target, pi);

                    case TypeCode.Int64:
                        return FastCreate<long>(target, pi);

                    case TypeCode.UInt64:
                        return FastCreate<ulong>(target, pi);

                    case TypeCode.Single:
                        return FastCreate<float>(target, pi);

                    case TypeCode.Double:
                        return FastCreate<double>(target, pi);

                    case TypeCode.Decimal:
                        return FastCreate<decimal>(target, pi);

                    case TypeCode.DateTime:
                        return FastCreate<DateTime>(target, pi);

                    case TypeCode.String:
                        return FastCreate<string>(target, pi);
                }
            }
            return SlowCreate(target, pi);
        }

        private static CallInstruction FastCreate<T0>(MethodInfo target, ParameterInfo[] pi)
        {
            Type type = TryGetParameterOrReturnType(target, pi, 1);
            if (type == null)
            {
                if (target.ReturnType == typeof(void))
                {
                    return new ActionCallInstruction<T0>(target);
                }
                return new FuncCallInstruction<T0>(target);
            }
            if (!type.IsEnum)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Object:
                        if ((type != typeof(object)) && (IndexIsNotReturnType(1, target, pi) || type.IsValueType))
                        {
                            break;
                        }
                        return FastCreate<T0, object>(target, pi);

                    case TypeCode.Boolean:
                        return FastCreate<T0, bool>(target, pi);

                    case TypeCode.Char:
                        return FastCreate<T0, char>(target, pi);

                    case TypeCode.SByte:
                        return FastCreate<T0, sbyte>(target, pi);

                    case TypeCode.Byte:
                        return FastCreate<T0, byte>(target, pi);

                    case TypeCode.Int16:
                        return FastCreate<T0, short>(target, pi);

                    case TypeCode.UInt16:
                        return FastCreate<T0, ushort>(target, pi);

                    case TypeCode.Int32:
                        return FastCreate<T0, int>(target, pi);

                    case TypeCode.UInt32:
                        return FastCreate<T0, int>(target, pi);

                    case TypeCode.Int64:
                        return FastCreate<T0, long>(target, pi);

                    case TypeCode.UInt64:
                        return FastCreate<T0, ulong>(target, pi);

                    case TypeCode.Single:
                        return FastCreate<T0, float>(target, pi);

                    case TypeCode.Double:
                        return FastCreate<T0, double>(target, pi);

                    case TypeCode.Decimal:
                        return FastCreate<T0, decimal>(target, pi);

                    case TypeCode.DateTime:
                        return FastCreate<T0, DateTime>(target, pi);

                    case TypeCode.String:
                        return FastCreate<T0, string>(target, pi);
                }
            }
            return SlowCreate(target, pi);
        }

        private static CallInstruction FastCreate<T0, T1>(MethodInfo target, ParameterInfo[] pi)
        {
            Type type = TryGetParameterOrReturnType(target, pi, 2);
            if (type == null)
            {
                if (target.ReturnType == typeof(void))
                {
                    return new ActionCallInstruction<T0, T1>(target);
                }
                return new FuncCallInstruction<T0, T1>(target);
            }
            if (!type.IsEnum)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Object:
                        if (type.IsValueType)
                        {
                            break;
                        }
                        return new FuncCallInstruction<T0, T1, object>(target);

                    case TypeCode.Boolean:
                        return new FuncCallInstruction<T0, T1, bool>(target);

                    case TypeCode.Char:
                        return new FuncCallInstruction<T0, T1, char>(target);

                    case TypeCode.SByte:
                        return new FuncCallInstruction<T0, T1, sbyte>(target);

                    case TypeCode.Byte:
                        return new FuncCallInstruction<T0, T1, byte>(target);

                    case TypeCode.Int16:
                        return new FuncCallInstruction<T0, T1, short>(target);

                    case TypeCode.UInt16:
                        return new FuncCallInstruction<T0, T1, ushort>(target);

                    case TypeCode.Int32:
                        return new FuncCallInstruction<T0, T1, int>(target);

                    case TypeCode.UInt32:
                        return new FuncCallInstruction<T0, T1, int>(target);

                    case TypeCode.Int64:
                        return new FuncCallInstruction<T0, T1, long>(target);

                    case TypeCode.UInt64:
                        return new FuncCallInstruction<T0, T1, ulong>(target);

                    case TypeCode.Single:
                        return new FuncCallInstruction<T0, T1, float>(target);

                    case TypeCode.Double:
                        return new FuncCallInstruction<T0, T1, double>(target);

                    case TypeCode.Decimal:
                        return new FuncCallInstruction<T0, T1, decimal>(target);

                    case TypeCode.DateTime:
                        return new FuncCallInstruction<T0, T1, DateTime>(target);

                    case TypeCode.String:
                        return new FuncCallInstruction<T0, T1, string>(target);
                }
            }
            return SlowCreate(target, pi);
        }

        private static CallInstruction GetArrayAccessor(MethodInfo info, int argumentCount)
        {
            Type declaringType = info.DeclaringType;
            bool flag = info.Name == "Get";
            switch (declaringType.GetArrayRank())
            {
                case 1:
                    return Create(flag ? declaringType.GetMethod("GetValue", new Type[] { typeof(int) }) : new Action<Array, int, object>(CallInstruction.ArrayItemSetter1).Method);

                case 2:
                    return Create(flag ? declaringType.GetMethod("GetValue", new Type[] { typeof(int), typeof(int) }) : new Action<Array, int, int, object>(CallInstruction.ArrayItemSetter2).Method);

                case 3:
                    return Create(flag ? declaringType.GetMethod("GetValue", new Type[] { typeof(int), typeof(int), typeof(int) }) : new Action<Array, int, int, int, object>(CallInstruction.ArrayItemSetter3).Method);
            }
            return new MethodInfoCallInstruction(info, argumentCount);
        }

        private static Type GetHelperType(MethodInfo info, Type[] arrTypes)
        {
            if (info.ReturnType == typeof(void))
            {
                switch (arrTypes.Length)
                {
                    case 0:
                        return typeof(ActionCallInstruction);

                    case 1:
                        return typeof(ActionCallInstruction<>).MakeGenericType(arrTypes);

                    case 2:
                        return typeof(ActionCallInstruction<,>).MakeGenericType(arrTypes);

                    case 3:
                        return typeof(ActionCallInstruction<,,>).MakeGenericType(arrTypes);

                    case 4:
                        return typeof(ActionCallInstruction<,,,>).MakeGenericType(arrTypes);

                    case 5:
                        return typeof(ActionCallInstruction<,,,,>).MakeGenericType(arrTypes);

                    case 6:
                        return typeof(ActionCallInstruction<,,,,,>).MakeGenericType(arrTypes);

                    case 7:
                        return typeof(ActionCallInstruction<,,,,,,>).MakeGenericType(arrTypes);

                    case 8:
                        return typeof(ActionCallInstruction<,,,,,,,>).MakeGenericType(arrTypes);

                    case 9:
                        return typeof(ActionCallInstruction<,,,,,,,,>).MakeGenericType(arrTypes);
                }
                throw new InvalidOperationException();
            }
            switch (arrTypes.Length)
            {
                case 1:
                    return typeof(FuncCallInstruction<>).MakeGenericType(arrTypes);

                case 2:
                    return typeof(FuncCallInstruction<,>).MakeGenericType(arrTypes);

                case 3:
                    return typeof(FuncCallInstruction<,,>).MakeGenericType(arrTypes);

                case 4:
                    return typeof(FuncCallInstruction<,,,>).MakeGenericType(arrTypes);

                case 5:
                    return typeof(FuncCallInstruction<,,,,>).MakeGenericType(arrTypes);

                case 6:
                    return typeof(FuncCallInstruction<,,,,,>).MakeGenericType(arrTypes);

                case 7:
                    return typeof(FuncCallInstruction<,,,,,,>).MakeGenericType(arrTypes);

                case 8:
                    return typeof(FuncCallInstruction<,,,,,,,>).MakeGenericType(arrTypes);

                case 9:
                    return typeof(FuncCallInstruction<,,,,,,,,>).MakeGenericType(arrTypes);

                case 10:
                    return typeof(FuncCallInstruction<,,,,,,,,,>).MakeGenericType(arrTypes);
            }
            throw new InvalidOperationException();
        }

        private static bool IndexIsNotReturnType(int index, MethodInfo target, ParameterInfo[] pi)
        {
            return ((pi.Length != index) || ((pi.Length == index) && !target.IsStatic));
        }

        public virtual object Invoke()
        {
            throw new InvalidOperationException();
        }

        public virtual object Invoke(object arg0)
        {
            throw new InvalidOperationException();
        }

        public virtual object Invoke(params object[] args)
        {
            switch (args.Length)
            {
                case 0:
                    return this.Invoke();

                case 1:
                    return this.Invoke(args[0]);

                case 2:
                    return this.Invoke(args[0], args[1]);

                case 3:
                    return this.Invoke(args[0], args[1], args[2]);

                case 4:
                    return this.Invoke(args[0], args[1], args[2], args[3]);

                case 5:
                    return this.Invoke(args[0], args[1], args[2], args[3], args[4]);

                case 6:
                    return this.Invoke(args[0], args[1], args[2], args[3], args[4], args[5]);

                case 7:
                    return this.Invoke(args[0], args[1], args[2], args[3], args[4], args[5], args[6]);

                case 8:
                    return this.Invoke(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]);

                case 9:
                    return this.Invoke(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8]);
            }
            throw new InvalidOperationException();
        }

        public virtual object Invoke(object arg0, object arg1)
        {
            throw new InvalidOperationException();
        }

        public virtual object Invoke(object arg0, object arg1, object arg2)
        {
            throw new InvalidOperationException();
        }

        public virtual object Invoke(object arg0, object arg1, object arg2, object arg3)
        {
            throw new InvalidOperationException();
        }

        public virtual object Invoke(object arg0, object arg1, object arg2, object arg3, object arg4)
        {
            throw new InvalidOperationException();
        }

        public virtual object Invoke(object arg0, object arg1, object arg2, object arg3, object arg4, object arg5)
        {
            throw new InvalidOperationException();
        }

        public virtual object Invoke(object arg0, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6)
        {
            throw new InvalidOperationException();
        }

        public virtual object Invoke(object arg0, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7)
        {
            throw new InvalidOperationException();
        }

        public virtual object Invoke(object arg0, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8)
        {
            throw new InvalidOperationException();
        }

        public virtual object InvokeInstance(object instance, params object[] args)
        {
            switch (args.Length)
            {
                case 0:
                    return this.Invoke(instance);

                case 1:
                    return this.Invoke(instance, args[0]);

                case 2:
                    return this.Invoke(instance, args[0], args[1]);

                case 3:
                    return this.Invoke(instance, args[0], args[1], args[2]);

                case 4:
                    return this.Invoke(instance, args[0], args[1], args[2], args[3]);

                case 5:
                    return this.Invoke(instance, args[0], args[1], args[2], args[3], args[4]);

                case 6:
                    return this.Invoke(instance, args[0], args[1], args[2], args[3], args[4], args[5]);

                case 7:
                    return this.Invoke(instance, args[0], args[1], args[2], args[3], args[4], args[5], args[6]);

                case 8:
                    return this.Invoke(instance, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]);
            }
            throw new InvalidOperationException();
        }

        private static bool ShouldCache(MethodInfo info)
        {
            return !(info is DynamicMethod);
        }

        private static CallInstruction SlowCreate(MethodInfo info, ParameterInfo[] pis)
        {
            List<Type> list = new List<Type>();
            if (!info.IsStatic)
            {
                list.Add(info.DeclaringType);
            }
            foreach (ParameterInfo info2 in pis)
            {
                list.Add(info2.ParameterType);
            }
            if (info.ReturnType != typeof(void))
            {
                list.Add(info.ReturnType);
            }
            Type[] arrTypes = list.ToArray();
            return (CallInstruction) Activator.CreateInstance(GetHelperType(info, arrTypes), new object[] { info });
        }

        public override string ToString()
        {
            return ("Call(" + this.Info + ")");
        }

        private static Type TryGetParameterOrReturnType(MethodInfo target, ParameterInfo[] pi, int index)
        {
            if (!target.IsStatic)
            {
                index--;
                if (index < 0)
                {
                    return target.DeclaringType;
                }
            }
            if (index < pi.Length)
            {
                return pi[index].ParameterType;
            }
            if (!(target.ReturnType == typeof(void)) && (index <= pi.Length))
            {
                return target.ReturnType;
            }
            return null;
        }

        public abstract int ArgumentCount { get; }

        public sealed override int ConsumedStack
        {
            get
            {
                return this.ArgumentCount;
            }
        }

        public abstract MethodInfo Info { get; }

        public sealed override string InstructionName
        {
            get
            {
                return "Call";
            }
        }

        public sealed override int ProducedStack
        {
            get
            {
                if (!(this.Info.ReturnType == typeof(void)))
                {
                    return 1;
                }
                return 0;
            }
        }
    }
}

