namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Threading;

    internal class LightLambda
    {
        private readonly StrongBox<object>[] _closure;
        private int _compilationThreshold;
        private Delegate _compiled;
        private readonly LightDelegateCreator _delegateCreator;
        private readonly System.Management.Automation.Interpreter.Interpreter _interpreter;
        private static readonly CacheDict<Type, Func<LightLambda, Delegate>> _runCache = new CacheDict<Type, Func<LightLambda, Delegate>>(100);
        internal const int MaxParameters = 0x10;

        public event EventHandler<LightLambdaCompileEventArgs> Compile;

        internal LightLambda(LightDelegateCreator delegateCreator, StrongBox<object>[] closure, int compilationThreshold)
        {
            this._delegateCreator = delegateCreator;
            this._closure = closure;
            this._interpreter = delegateCreator.Interpreter;
            this._compilationThreshold = compilationThreshold;
        }

        private Delegate CreateCustomDelegate(Type delegateType)
        {
            MethodInfo method = delegateType.GetMethod("Invoke");
            ParameterInfo[] parameters = method.GetParameters();
            ParameterExpression[] expressionArray = new ParameterExpression[parameters.Length];
            Expression[] initializers = new Expression[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterExpression expression = Expression.Parameter(parameters[i].ParameterType, parameters[i].Name);
                expressionArray[i] = expression;
                initializers[i] = Expression.Convert(expression, typeof(object));
            }
            NewArrayExpression expression2 = Expression.NewArrayInit(typeof(object), initializers);
            Expression instance = Utils.Constant(this);
            MethodInfo info2 = typeof(LightLambda).GetMethod("Run");
            UnaryExpression body = Expression.Convert(Expression.Call(instance, info2, new Expression[] { expression2 }), method.ReturnType);
            return Expression.Lambda(delegateType, body, expressionArray).Compile();
        }

        private static Func<LightLambda, Delegate> GetRunDelegateCtor(Type delegateType)
        {
            lock (_runCache)
            {
                Func<LightLambda, Delegate> func;
                if (_runCache.TryGetValue(delegateType, out func))
                {
                    return func;
                }
                return MakeRunDelegateCtor(delegateType);
            }
        }

        internal Delegate MakeDelegate(Type delegateType)
        {
            Func<LightLambda, Delegate> runDelegateCtor = GetRunDelegateCtor(delegateType);
            if (runDelegateCtor != null)
            {
                return runDelegateCtor(this);
            }
            return this.CreateCustomDelegate(delegateType);
        }

        private InterpretedFrame MakeFrame()
        {
            return new InterpretedFrame(this._interpreter, this._closure);
        }

        internal static Delegate MakeRun0<TRet>(LightLambda lambda)
        {
            return new Func<TRet>(lambda.Run0<TRet>);
        }

        internal static Delegate MakeRun1<T0, TRet>(LightLambda lambda)
        {
            return new Func<T0, TRet>(lambda.Run1<T0, TRet>);
        }

        internal static Delegate MakeRun10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet>(LightLambda lambda)
        {
            return new Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet>(lambda.Run10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet>);
        }

        internal static Delegate MakeRun11<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet>(LightLambda lambda)
        {
            return new Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet>(lambda.Run11<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet>);
        }

        internal static Delegate MakeRun12<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet>(LightLambda lambda)
        {
            return new Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet>(lambda.Run12<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet>);
        }

        internal static Delegate MakeRun13<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TRet>(LightLambda lambda)
        {
            return new Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TRet>(lambda.Run13<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TRet>);
        }

        internal static Delegate MakeRun14<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TRet>(LightLambda lambda)
        {
            return new Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TRet>(lambda.Run14<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TRet>);
        }

        internal static Delegate MakeRun15<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TRet>(LightLambda lambda)
        {
            return new Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TRet>(lambda.Run15<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TRet>);
        }

        internal static Delegate MakeRun2<T0, T1, TRet>(LightLambda lambda)
        {
            return new Func<T0, T1, TRet>(lambda.Run2<T0, T1, TRet>);
        }

        internal static Delegate MakeRun3<T0, T1, T2, TRet>(LightLambda lambda)
        {
            return new Func<T0, T1, T2, TRet>(lambda.Run3<T0, T1, T2, TRet>);
        }

        internal static Delegate MakeRun4<T0, T1, T2, T3, TRet>(LightLambda lambda)
        {
            return new Func<T0, T1, T2, T3, TRet>(lambda.Run4<T0, T1, T2, T3, TRet>);
        }

        internal static Delegate MakeRun5<T0, T1, T2, T3, T4, TRet>(LightLambda lambda)
        {
            return new Func<T0, T1, T2, T3, T4, TRet>(lambda.Run5<T0, T1, T2, T3, T4, TRet>);
        }

        internal static Delegate MakeRun6<T0, T1, T2, T3, T4, T5, TRet>(LightLambda lambda)
        {
            return new Func<T0, T1, T2, T3, T4, T5, TRet>(lambda.Run6<T0, T1, T2, T3, T4, T5, TRet>);
        }

        internal static Delegate MakeRun7<T0, T1, T2, T3, T4, T5, T6, TRet>(LightLambda lambda)
        {
            return new Func<T0, T1, T2, T3, T4, T5, T6, TRet>(lambda.Run7<T0, T1, T2, T3, T4, T5, T6, TRet>);
        }

        internal static Delegate MakeRun8<T0, T1, T2, T3, T4, T5, T6, T7, TRet>(LightLambda lambda)
        {
            return new Func<T0, T1, T2, T3, T4, T5, T6, T7, TRet>(lambda.Run8<T0, T1, T2, T3, T4, T5, T6, T7, TRet>);
        }

        internal static Delegate MakeRun9<T0, T1, T2, T3, T4, T5, T6, T7, T8, TRet>(LightLambda lambda)
        {
            return new Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, TRet>(lambda.Run9<T0, T1, T2, T3, T4, T5, T6, T7, T8, TRet>);
        }

        private static Func<LightLambda, Delegate> MakeRunDelegateCtor(Type delegateType)
        {
            Type[] typeArray;
            MethodInfo info2;
            Func<LightLambda, Delegate> func4;
            MethodInfo info = delegateType.GetMethod("Invoke");
            ParameterInfo[] parameters = info.GetParameters();
            string name = "Run";
            if (parameters.Length >= 0x10)
            {
                return null;
            }
            if (info.ReturnType == typeof(void))
            {
                name = name + "Void";
                typeArray = new Type[parameters.Length];
            }
            else
            {
                typeArray = new Type[parameters.Length + 1];
                typeArray[typeArray.Length - 1] = info.ReturnType;
            }
            if (((info.ReturnType == typeof(void)) && (typeArray.Length == 2)) && (parameters[0].ParameterType.IsByRef && parameters[1].ParameterType.IsByRef))
            {
                info2 = typeof(LightLambda).GetMethod("RunVoidRef2", BindingFlags.NonPublic | BindingFlags.Instance);
                typeArray[0] = parameters[0].ParameterType.GetElementType();
                typeArray[1] = parameters[1].ParameterType.GetElementType();
            }
            else if ((info.ReturnType == typeof(void)) && (typeArray.Length == 0))
            {
                info2 = typeof(LightLambda).GetMethod("RunVoid0", BindingFlags.NonPublic | BindingFlags.Instance);
            }
            else
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    typeArray[i] = parameters[i].ParameterType;
                    if (typeArray[i].IsByRef)
                    {
                        return null;
                    }
                }
                if (DelegateHelpers.MakeDelegate(typeArray) == delegateType)
                {
                    Func<LightLambda, Delegate> func2;
                    name = "Make" + name + parameters.Length;
                    MethodInfo info3 = typeof(LightLambda).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(typeArray);
                    _runCache[delegateType] = func2 = (Func<LightLambda, Delegate>) Delegate.CreateDelegate(typeof(Func<LightLambda, Delegate>), info3);
                    return func2;
                }
                info2 = typeof(LightLambda).GetMethod(name + parameters.Length, BindingFlags.NonPublic | BindingFlags.Instance);
            }
            try
            {
                DynamicMethod method = new DynamicMethod("FastCtor", typeof(Delegate), new Type[] { typeof(LightLambda) }, typeof(LightLambda), true);
                ILGenerator iLGenerator = method.GetILGenerator();
                iLGenerator.Emit(OpCodes.Ldarg_0);
                iLGenerator.Emit(OpCodes.Ldftn, info2.IsGenericMethodDefinition ? info2.MakeGenericMethod(typeArray) : info2);
                iLGenerator.Emit(OpCodes.Newobj, delegateType.GetConstructor(new Type[] { typeof(object), typeof(IntPtr) }));
                iLGenerator.Emit(OpCodes.Ret);
                return (_runCache[delegateType] = (Func<LightLambda, Delegate>) method.CreateDelegate(typeof(Func<LightLambda, Delegate>)));
            }
            catch (SecurityException)
            {
            }
            MethodInfo targetMethod = info2.IsGenericMethodDefinition ? info2.MakeGenericMethod(typeArray) : info2;
            _runCache[delegateType] = func4 = lambda => Delegate.CreateDelegate(delegateType, lambda, targetMethod);
            return func4;
        }

        internal static Delegate MakeRunVoid0(LightLambda lambda)
        {
            return new Action(lambda.RunVoid0);
        }

        internal static Delegate MakeRunVoid1<T0>(LightLambda lambda)
        {
            return new Action<T0>(lambda.RunVoid1<T0>);
        }

        internal static Delegate MakeRunVoid10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(LightLambda lambda)
        {
            return new Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(lambda.RunVoid10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>);
        }

        internal static Delegate MakeRunVoid11<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(LightLambda lambda)
        {
            return new Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(lambda.RunVoid11<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>);
        }

        internal static Delegate MakeRunVoid12<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(LightLambda lambda)
        {
            return new Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(lambda.RunVoid12<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>);
        }

        internal static Delegate MakeRunVoid13<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(LightLambda lambda)
        {
            return new Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(lambda.RunVoid13<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>);
        }

        internal static Delegate MakeRunVoid14<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(LightLambda lambda)
        {
            return new Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(lambda.RunVoid14<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>);
        }

        internal static Delegate MakeRunVoid15<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(LightLambda lambda)
        {
            return new Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(lambda.RunVoid15<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>);
        }

        internal static Delegate MakeRunVoid2<T0, T1>(LightLambda lambda)
        {
            return new Action<T0, T1>(lambda.RunVoid2<T0, T1>);
        }

        internal static Delegate MakeRunVoid3<T0, T1, T2>(LightLambda lambda)
        {
            return new Action<T0, T1, T2>(lambda.RunVoid3<T0, T1, T2>);
        }

        internal static Delegate MakeRunVoid4<T0, T1, T2, T3>(LightLambda lambda)
        {
            return new Action<T0, T1, T2, T3>(lambda.RunVoid4<T0, T1, T2, T3>);
        }

        internal static Delegate MakeRunVoid5<T0, T1, T2, T3, T4>(LightLambda lambda)
        {
            return new Action<T0, T1, T2, T3, T4>(lambda.RunVoid5<T0, T1, T2, T3, T4>);
        }

        internal static Delegate MakeRunVoid6<T0, T1, T2, T3, T4, T5>(LightLambda lambda)
        {
            return new Action<T0, T1, T2, T3, T4, T5>(lambda.RunVoid6<T0, T1, T2, T3, T4, T5>);
        }

        internal static Delegate MakeRunVoid7<T0, T1, T2, T3, T4, T5, T6>(LightLambda lambda)
        {
            return new Action<T0, T1, T2, T3, T4, T5, T6>(lambda.RunVoid7<T0, T1, T2, T3, T4, T5, T6>);
        }

        internal static Delegate MakeRunVoid8<T0, T1, T2, T3, T4, T5, T6, T7>(LightLambda lambda)
        {
            return new Action<T0, T1, T2, T3, T4, T5, T6, T7>(lambda.RunVoid8<T0, T1, T2, T3, T4, T5, T6, T7>);
        }

        internal static Delegate MakeRunVoid9<T0, T1, T2, T3, T4, T5, T6, T7, T8>(LightLambda lambda)
        {
            return new Action<T0, T1, T2, T3, T4, T5, T6, T7, T8>(lambda.RunVoid9<T0, T1, T2, T3, T4, T5, T6, T7, T8>);
        }

        public object Run(params object[] arguments)
        {
            if ((this._compiled != null) || this.TryGetCompiled())
            {
                return this._compiled.DynamicInvoke(arguments);
            }
            InterpretedFrame frame = this.MakeFrame();
            for (int i = 0; i < arguments.Length; i++)
            {
                frame.Data[i] = arguments[i];
            }
            System.Management.Automation.Interpreter.ThreadLocal<InterpretedFrame>.StorageInfo currentFrame = frame.Enter();
            try
            {
                this._interpreter.Run(frame);
            }
            finally
            {
                frame.Leave(currentFrame);
            }
            return frame.Pop();
        }

        internal TRet Run0<TRet>()
        {
            if ((this._compiled != null) || this.TryGetCompiled())
            {
                return ((Func<TRet>) this._compiled)();
            }
            InterpretedFrame frame = this.MakeFrame();
            System.Management.Automation.Interpreter.ThreadLocal<InterpretedFrame>.StorageInfo currentFrame = frame.Enter();
            try
            {
                this._interpreter.Run(frame);
            }
            finally
            {
                frame.Leave(currentFrame);
            }
            return (TRet) frame.Pop();
        }

        internal TRet Run1<T0, TRet>(T0 arg0)
        {
            if ((this._compiled != null) || this.TryGetCompiled())
            {
                return ((Func<T0, TRet>) this._compiled)(arg0);
            }
            InterpretedFrame frame = this.MakeFrame();
            frame.Data[0] = arg0;
            System.Management.Automation.Interpreter.ThreadLocal<InterpretedFrame>.StorageInfo currentFrame = frame.Enter();
            try
            {
                this._interpreter.Run(frame);
            }
            finally
            {
                frame.Leave(currentFrame);
            }
            return (TRet) frame.Pop();
        }

        internal TRet Run10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            if ((this._compiled != null) || this.TryGetCompiled())
            {
                return ((Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet>) this._compiled)(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
            }
            InterpretedFrame frame = this.MakeFrame();
            frame.Data[0] = arg0;
            frame.Data[1] = arg1;
            frame.Data[2] = arg2;
            frame.Data[3] = arg3;
            frame.Data[4] = arg4;
            frame.Data[5] = arg5;
            frame.Data[6] = arg6;
            frame.Data[7] = arg7;
            frame.Data[8] = arg8;
            frame.Data[9] = arg9;
            System.Management.Automation.Interpreter.ThreadLocal<InterpretedFrame>.StorageInfo currentFrame = frame.Enter();
            try
            {
                this._interpreter.Run(frame);
            }
            finally
            {
                frame.Leave(currentFrame);
            }
            return (TRet) frame.Pop();
        }

        internal TRet Run11<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
        {
            if ((this._compiled != null) || this.TryGetCompiled())
            {
                return ((Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet>) this._compiled)(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
            }
            InterpretedFrame frame = this.MakeFrame();
            frame.Data[0] = arg0;
            frame.Data[1] = arg1;
            frame.Data[2] = arg2;
            frame.Data[3] = arg3;
            frame.Data[4] = arg4;
            frame.Data[5] = arg5;
            frame.Data[6] = arg6;
            frame.Data[7] = arg7;
            frame.Data[8] = arg8;
            frame.Data[9] = arg9;
            frame.Data[10] = arg10;
            System.Management.Automation.Interpreter.ThreadLocal<InterpretedFrame>.StorageInfo currentFrame = frame.Enter();
            try
            {
                this._interpreter.Run(frame);
            }
            finally
            {
                frame.Leave(currentFrame);
            }
            return (TRet) frame.Pop();
        }

        internal TRet Run12<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
        {
            if ((this._compiled != null) || this.TryGetCompiled())
            {
                return ((Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet>) this._compiled)(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
            }
            InterpretedFrame frame = this.MakeFrame();
            frame.Data[0] = arg0;
            frame.Data[1] = arg1;
            frame.Data[2] = arg2;
            frame.Data[3] = arg3;
            frame.Data[4] = arg4;
            frame.Data[5] = arg5;
            frame.Data[6] = arg6;
            frame.Data[7] = arg7;
            frame.Data[8] = arg8;
            frame.Data[9] = arg9;
            frame.Data[10] = arg10;
            frame.Data[11] = arg11;
            System.Management.Automation.Interpreter.ThreadLocal<InterpretedFrame>.StorageInfo currentFrame = frame.Enter();
            try
            {
                this._interpreter.Run(frame);
            }
            finally
            {
                frame.Leave(currentFrame);
            }
            return (TRet) frame.Pop();
        }

        internal TRet Run13<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TRet>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
        {
            if ((this._compiled != null) || this.TryGetCompiled())
            {
                return ((Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TRet>) this._compiled)(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
            }
            InterpretedFrame frame = this.MakeFrame();
            frame.Data[0] = arg0;
            frame.Data[1] = arg1;
            frame.Data[2] = arg2;
            frame.Data[3] = arg3;
            frame.Data[4] = arg4;
            frame.Data[5] = arg5;
            frame.Data[6] = arg6;
            frame.Data[7] = arg7;
            frame.Data[8] = arg8;
            frame.Data[9] = arg9;
            frame.Data[10] = arg10;
            frame.Data[11] = arg11;
            frame.Data[12] = arg12;
            System.Management.Automation.Interpreter.ThreadLocal<InterpretedFrame>.StorageInfo currentFrame = frame.Enter();
            try
            {
                this._interpreter.Run(frame);
            }
            finally
            {
                frame.Leave(currentFrame);
            }
            return (TRet) frame.Pop();
        }

        internal TRet Run14<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TRet>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
        {
            if ((this._compiled != null) || this.TryGetCompiled())
            {
                return ((Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TRet>) this._compiled)(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
            }
            InterpretedFrame frame = this.MakeFrame();
            frame.Data[0] = arg0;
            frame.Data[1] = arg1;
            frame.Data[2] = arg2;
            frame.Data[3] = arg3;
            frame.Data[4] = arg4;
            frame.Data[5] = arg5;
            frame.Data[6] = arg6;
            frame.Data[7] = arg7;
            frame.Data[8] = arg8;
            frame.Data[9] = arg9;
            frame.Data[10] = arg10;
            frame.Data[11] = arg11;
            frame.Data[12] = arg12;
            frame.Data[13] = arg13;
            System.Management.Automation.Interpreter.ThreadLocal<InterpretedFrame>.StorageInfo currentFrame = frame.Enter();
            try
            {
                this._interpreter.Run(frame);
            }
            finally
            {
                frame.Leave(currentFrame);
            }
            return (TRet) frame.Pop();
        }

        internal TRet Run15<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TRet>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
        {
            if ((this._compiled != null) || this.TryGetCompiled())
            {
                return ((Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TRet>) this._compiled)(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
            }
            InterpretedFrame frame = this.MakeFrame();
            frame.Data[0] = arg0;
            frame.Data[1] = arg1;
            frame.Data[2] = arg2;
            frame.Data[3] = arg3;
            frame.Data[4] = arg4;
            frame.Data[5] = arg5;
            frame.Data[6] = arg6;
            frame.Data[7] = arg7;
            frame.Data[8] = arg8;
            frame.Data[9] = arg9;
            frame.Data[10] = arg10;
            frame.Data[11] = arg11;
            frame.Data[12] = arg12;
            frame.Data[13] = arg13;
            frame.Data[14] = arg14;
            System.Management.Automation.Interpreter.ThreadLocal<InterpretedFrame>.StorageInfo currentFrame = frame.Enter();
            try
            {
                this._interpreter.Run(frame);
            }
            finally
            {
                frame.Leave(currentFrame);
            }
            return (TRet) frame.Pop();
        }

        internal TRet Run2<T0, T1, TRet>(T0 arg0, T1 arg1)
        {
            if ((this._compiled != null) || this.TryGetCompiled())
            {
                return ((Func<T0, T1, TRet>) this._compiled)(arg0, arg1);
            }
            InterpretedFrame frame = this.MakeFrame();
            frame.Data[0] = arg0;
            frame.Data[1] = arg1;
            System.Management.Automation.Interpreter.ThreadLocal<InterpretedFrame>.StorageInfo currentFrame = frame.Enter();
            try
            {
                this._interpreter.Run(frame);
            }
            finally
            {
                frame.Leave(currentFrame);
            }
            return (TRet) frame.Pop();
        }

        internal TRet Run3<T0, T1, T2, TRet>(T0 arg0, T1 arg1, T2 arg2)
        {
            if ((this._compiled != null) || this.TryGetCompiled())
            {
                return ((Func<T0, T1, T2, TRet>) this._compiled)(arg0, arg1, arg2);
            }
            InterpretedFrame frame = this.MakeFrame();
            frame.Data[0] = arg0;
            frame.Data[1] = arg1;
            frame.Data[2] = arg2;
            System.Management.Automation.Interpreter.ThreadLocal<InterpretedFrame>.StorageInfo currentFrame = frame.Enter();
            try
            {
                this._interpreter.Run(frame);
            }
            finally
            {
                frame.Leave(currentFrame);
            }
            return (TRet) frame.Pop();
        }

        internal TRet Run4<T0, T1, T2, T3, TRet>(T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            if ((this._compiled != null) || this.TryGetCompiled())
            {
                return ((Func<T0, T1, T2, T3, TRet>) this._compiled)(arg0, arg1, arg2, arg3);
            }
            InterpretedFrame frame = this.MakeFrame();
            frame.Data[0] = arg0;
            frame.Data[1] = arg1;
            frame.Data[2] = arg2;
            frame.Data[3] = arg3;
            System.Management.Automation.Interpreter.ThreadLocal<InterpretedFrame>.StorageInfo currentFrame = frame.Enter();
            try
            {
                this._interpreter.Run(frame);
            }
            finally
            {
                frame.Leave(currentFrame);
            }
            return (TRet) frame.Pop();
        }

        internal TRet Run5<T0, T1, T2, T3, T4, TRet>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            if ((this._compiled != null) || this.TryGetCompiled())
            {
                return ((Func<T0, T1, T2, T3, T4, TRet>) this._compiled)(arg0, arg1, arg2, arg3, arg4);
            }
            InterpretedFrame frame = this.MakeFrame();
            frame.Data[0] = arg0;
            frame.Data[1] = arg1;
            frame.Data[2] = arg2;
            frame.Data[3] = arg3;
            frame.Data[4] = arg4;
            System.Management.Automation.Interpreter.ThreadLocal<InterpretedFrame>.StorageInfo currentFrame = frame.Enter();
            try
            {
                this._interpreter.Run(frame);
            }
            finally
            {
                frame.Leave(currentFrame);
            }
            return (TRet) frame.Pop();
        }

        internal TRet Run6<T0, T1, T2, T3, T4, T5, TRet>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            if ((this._compiled != null) || this.TryGetCompiled())
            {
                return ((Func<T0, T1, T2, T3, T4, T5, TRet>) this._compiled)(arg0, arg1, arg2, arg3, arg4, arg5);
            }
            InterpretedFrame frame = this.MakeFrame();
            frame.Data[0] = arg0;
            frame.Data[1] = arg1;
            frame.Data[2] = arg2;
            frame.Data[3] = arg3;
            frame.Data[4] = arg4;
            frame.Data[5] = arg5;
            System.Management.Automation.Interpreter.ThreadLocal<InterpretedFrame>.StorageInfo currentFrame = frame.Enter();
            try
            {
                this._interpreter.Run(frame);
            }
            finally
            {
                frame.Leave(currentFrame);
            }
            return (TRet) frame.Pop();
        }

        internal TRet Run7<T0, T1, T2, T3, T4, T5, T6, TRet>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            if ((this._compiled != null) || this.TryGetCompiled())
            {
                return ((Func<T0, T1, T2, T3, T4, T5, T6, TRet>) this._compiled)(arg0, arg1, arg2, arg3, arg4, arg5, arg6);
            }
            InterpretedFrame frame = this.MakeFrame();
            frame.Data[0] = arg0;
            frame.Data[1] = arg1;
            frame.Data[2] = arg2;
            frame.Data[3] = arg3;
            frame.Data[4] = arg4;
            frame.Data[5] = arg5;
            frame.Data[6] = arg6;
            System.Management.Automation.Interpreter.ThreadLocal<InterpretedFrame>.StorageInfo currentFrame = frame.Enter();
            try
            {
                this._interpreter.Run(frame);
            }
            finally
            {
                frame.Leave(currentFrame);
            }
            return (TRet) frame.Pop();
        }

        internal TRet Run8<T0, T1, T2, T3, T4, T5, T6, T7, TRet>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            if ((this._compiled != null) || this.TryGetCompiled())
            {
                return ((Func<T0, T1, T2, T3, T4, T5, T6, T7, TRet>) this._compiled)(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
            }
            InterpretedFrame frame = this.MakeFrame();
            frame.Data[0] = arg0;
            frame.Data[1] = arg1;
            frame.Data[2] = arg2;
            frame.Data[3] = arg3;
            frame.Data[4] = arg4;
            frame.Data[5] = arg5;
            frame.Data[6] = arg6;
            frame.Data[7] = arg7;
            System.Management.Automation.Interpreter.ThreadLocal<InterpretedFrame>.StorageInfo currentFrame = frame.Enter();
            try
            {
                this._interpreter.Run(frame);
            }
            finally
            {
                frame.Leave(currentFrame);
            }
            return (TRet) frame.Pop();
        }

        internal TRet Run9<T0, T1, T2, T3, T4, T5, T6, T7, T8, TRet>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            if ((this._compiled != null) || this.TryGetCompiled())
            {
                return ((Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, TRet>) this._compiled)(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
            }
            InterpretedFrame frame = this.MakeFrame();
            frame.Data[0] = arg0;
            frame.Data[1] = arg1;
            frame.Data[2] = arg2;
            frame.Data[3] = arg3;
            frame.Data[4] = arg4;
            frame.Data[5] = arg5;
            frame.Data[6] = arg6;
            frame.Data[7] = arg7;
            frame.Data[8] = arg8;
            System.Management.Automation.Interpreter.ThreadLocal<InterpretedFrame>.StorageInfo currentFrame = frame.Enter();
            try
            {
                this._interpreter.Run(frame);
            }
            finally
            {
                frame.Leave(currentFrame);
            }
            return (TRet) frame.Pop();
        }

        internal void RunVoid0()
        {
            if ((this._compiled != null) || this.TryGetCompiled())
            {
                ((Action) this._compiled)();
            }
            else
            {
                InterpretedFrame frame = this.MakeFrame();
                System.Management.Automation.Interpreter.ThreadLocal<InterpretedFrame>.StorageInfo currentFrame = frame.Enter();
                try
                {
                    this._interpreter.Run(frame);
                }
                finally
                {
                    frame.Leave(currentFrame);
                }
            }
        }

        internal void RunVoid1<T0>(T0 arg0)
        {
            if ((this._compiled != null) || this.TryGetCompiled())
            {
                ((Action<T0>) this._compiled)(arg0);
            }
            else
            {
                InterpretedFrame frame = this.MakeFrame();
                frame.Data[0] = arg0;
                System.Management.Automation.Interpreter.ThreadLocal<InterpretedFrame>.StorageInfo currentFrame = frame.Enter();
                try
                {
                    this._interpreter.Run(frame);
                }
                finally
                {
                    frame.Leave(currentFrame);
                }
            }
        }

        internal void RunVoid10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            if ((this._compiled != null) || this.TryGetCompiled())
            {
                ((Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>) this._compiled)(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
            }
            else
            {
                InterpretedFrame frame = this.MakeFrame();
                frame.Data[0] = arg0;
                frame.Data[1] = arg1;
                frame.Data[2] = arg2;
                frame.Data[3] = arg3;
                frame.Data[4] = arg4;
                frame.Data[5] = arg5;
                frame.Data[6] = arg6;
                frame.Data[7] = arg7;
                frame.Data[8] = arg8;
                frame.Data[9] = arg9;
                System.Management.Automation.Interpreter.ThreadLocal<InterpretedFrame>.StorageInfo currentFrame = frame.Enter();
                try
                {
                    this._interpreter.Run(frame);
                }
                finally
                {
                    frame.Leave(currentFrame);
                }
            }
        }

        internal void RunVoid11<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
        {
            if ((this._compiled != null) || this.TryGetCompiled())
            {
                ((Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>) this._compiled)(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
            }
            else
            {
                InterpretedFrame frame = this.MakeFrame();
                frame.Data[0] = arg0;
                frame.Data[1] = arg1;
                frame.Data[2] = arg2;
                frame.Data[3] = arg3;
                frame.Data[4] = arg4;
                frame.Data[5] = arg5;
                frame.Data[6] = arg6;
                frame.Data[7] = arg7;
                frame.Data[8] = arg8;
                frame.Data[9] = arg9;
                frame.Data[10] = arg10;
                System.Management.Automation.Interpreter.ThreadLocal<InterpretedFrame>.StorageInfo currentFrame = frame.Enter();
                try
                {
                    this._interpreter.Run(frame);
                }
                finally
                {
                    frame.Leave(currentFrame);
                }
            }
        }

        internal void RunVoid12<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
        {
            if ((this._compiled != null) || this.TryGetCompiled())
            {
                ((Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>) this._compiled)(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
            }
            else
            {
                InterpretedFrame frame = this.MakeFrame();
                frame.Data[0] = arg0;
                frame.Data[1] = arg1;
                frame.Data[2] = arg2;
                frame.Data[3] = arg3;
                frame.Data[4] = arg4;
                frame.Data[5] = arg5;
                frame.Data[6] = arg6;
                frame.Data[7] = arg7;
                frame.Data[8] = arg8;
                frame.Data[9] = arg9;
                frame.Data[10] = arg10;
                frame.Data[11] = arg11;
                System.Management.Automation.Interpreter.ThreadLocal<InterpretedFrame>.StorageInfo currentFrame = frame.Enter();
                try
                {
                    this._interpreter.Run(frame);
                }
                finally
                {
                    frame.Leave(currentFrame);
                }
            }
        }

        internal void RunVoid13<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
        {
            if ((this._compiled != null) || this.TryGetCompiled())
            {
                ((Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>) this._compiled)(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
            }
            else
            {
                InterpretedFrame frame = this.MakeFrame();
                frame.Data[0] = arg0;
                frame.Data[1] = arg1;
                frame.Data[2] = arg2;
                frame.Data[3] = arg3;
                frame.Data[4] = arg4;
                frame.Data[5] = arg5;
                frame.Data[6] = arg6;
                frame.Data[7] = arg7;
                frame.Data[8] = arg8;
                frame.Data[9] = arg9;
                frame.Data[10] = arg10;
                frame.Data[11] = arg11;
                frame.Data[12] = arg12;
                System.Management.Automation.Interpreter.ThreadLocal<InterpretedFrame>.StorageInfo currentFrame = frame.Enter();
                try
                {
                    this._interpreter.Run(frame);
                }
                finally
                {
                    frame.Leave(currentFrame);
                }
            }
        }

        internal void RunVoid14<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
        {
            if ((this._compiled != null) || this.TryGetCompiled())
            {
                ((Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>) this._compiled)(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
            }
            else
            {
                InterpretedFrame frame = this.MakeFrame();
                frame.Data[0] = arg0;
                frame.Data[1] = arg1;
                frame.Data[2] = arg2;
                frame.Data[3] = arg3;
                frame.Data[4] = arg4;
                frame.Data[5] = arg5;
                frame.Data[6] = arg6;
                frame.Data[7] = arg7;
                frame.Data[8] = arg8;
                frame.Data[9] = arg9;
                frame.Data[10] = arg10;
                frame.Data[11] = arg11;
                frame.Data[12] = arg12;
                frame.Data[13] = arg13;
                System.Management.Automation.Interpreter.ThreadLocal<InterpretedFrame>.StorageInfo currentFrame = frame.Enter();
                try
                {
                    this._interpreter.Run(frame);
                }
                finally
                {
                    frame.Leave(currentFrame);
                }
            }
        }

        internal void RunVoid15<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
        {
            if ((this._compiled != null) || this.TryGetCompiled())
            {
                ((Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>) this._compiled)(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
            }
            else
            {
                InterpretedFrame frame = this.MakeFrame();
                frame.Data[0] = arg0;
                frame.Data[1] = arg1;
                frame.Data[2] = arg2;
                frame.Data[3] = arg3;
                frame.Data[4] = arg4;
                frame.Data[5] = arg5;
                frame.Data[6] = arg6;
                frame.Data[7] = arg7;
                frame.Data[8] = arg8;
                frame.Data[9] = arg9;
                frame.Data[10] = arg10;
                frame.Data[11] = arg11;
                frame.Data[12] = arg12;
                frame.Data[13] = arg13;
                frame.Data[14] = arg14;
                System.Management.Automation.Interpreter.ThreadLocal<InterpretedFrame>.StorageInfo currentFrame = frame.Enter();
                try
                {
                    this._interpreter.Run(frame);
                }
                finally
                {
                    frame.Leave(currentFrame);
                }
            }
        }

        internal void RunVoid2<T0, T1>(T0 arg0, T1 arg1)
        {
            if ((this._compiled != null) || this.TryGetCompiled())
            {
                ((Action<T0, T1>) this._compiled)(arg0, arg1);
            }
            else
            {
                InterpretedFrame frame = this.MakeFrame();
                frame.Data[0] = arg0;
                frame.Data[1] = arg1;
                System.Management.Automation.Interpreter.ThreadLocal<InterpretedFrame>.StorageInfo currentFrame = frame.Enter();
                try
                {
                    this._interpreter.Run(frame);
                }
                finally
                {
                    frame.Leave(currentFrame);
                }
            }
        }

        internal void RunVoid3<T0, T1, T2>(T0 arg0, T1 arg1, T2 arg2)
        {
            if ((this._compiled != null) || this.TryGetCompiled())
            {
                ((Action<T0, T1, T2>) this._compiled)(arg0, arg1, arg2);
            }
            else
            {
                InterpretedFrame frame = this.MakeFrame();
                frame.Data[0] = arg0;
                frame.Data[1] = arg1;
                frame.Data[2] = arg2;
                System.Management.Automation.Interpreter.ThreadLocal<InterpretedFrame>.StorageInfo currentFrame = frame.Enter();
                try
                {
                    this._interpreter.Run(frame);
                }
                finally
                {
                    frame.Leave(currentFrame);
                }
            }
        }

        internal void RunVoid4<T0, T1, T2, T3>(T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            if ((this._compiled != null) || this.TryGetCompiled())
            {
                ((Action<T0, T1, T2, T3>) this._compiled)(arg0, arg1, arg2, arg3);
            }
            else
            {
                InterpretedFrame frame = this.MakeFrame();
                frame.Data[0] = arg0;
                frame.Data[1] = arg1;
                frame.Data[2] = arg2;
                frame.Data[3] = arg3;
                System.Management.Automation.Interpreter.ThreadLocal<InterpretedFrame>.StorageInfo currentFrame = frame.Enter();
                try
                {
                    this._interpreter.Run(frame);
                }
                finally
                {
                    frame.Leave(currentFrame);
                }
            }
        }

        internal void RunVoid5<T0, T1, T2, T3, T4>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            if ((this._compiled != null) || this.TryGetCompiled())
            {
                ((Action<T0, T1, T2, T3, T4>) this._compiled)(arg0, arg1, arg2, arg3, arg4);
            }
            else
            {
                InterpretedFrame frame = this.MakeFrame();
                frame.Data[0] = arg0;
                frame.Data[1] = arg1;
                frame.Data[2] = arg2;
                frame.Data[3] = arg3;
                frame.Data[4] = arg4;
                System.Management.Automation.Interpreter.ThreadLocal<InterpretedFrame>.StorageInfo currentFrame = frame.Enter();
                try
                {
                    this._interpreter.Run(frame);
                }
                finally
                {
                    frame.Leave(currentFrame);
                }
            }
        }

        internal void RunVoid6<T0, T1, T2, T3, T4, T5>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            if ((this._compiled != null) || this.TryGetCompiled())
            {
                ((Action<T0, T1, T2, T3, T4, T5>) this._compiled)(arg0, arg1, arg2, arg3, arg4, arg5);
            }
            else
            {
                InterpretedFrame frame = this.MakeFrame();
                frame.Data[0] = arg0;
                frame.Data[1] = arg1;
                frame.Data[2] = arg2;
                frame.Data[3] = arg3;
                frame.Data[4] = arg4;
                frame.Data[5] = arg5;
                System.Management.Automation.Interpreter.ThreadLocal<InterpretedFrame>.StorageInfo currentFrame = frame.Enter();
                try
                {
                    this._interpreter.Run(frame);
                }
                finally
                {
                    frame.Leave(currentFrame);
                }
            }
        }

        internal void RunVoid7<T0, T1, T2, T3, T4, T5, T6>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            if ((this._compiled != null) || this.TryGetCompiled())
            {
                ((Action<T0, T1, T2, T3, T4, T5, T6>) this._compiled)(arg0, arg1, arg2, arg3, arg4, arg5, arg6);
            }
            else
            {
                InterpretedFrame frame = this.MakeFrame();
                frame.Data[0] = arg0;
                frame.Data[1] = arg1;
                frame.Data[2] = arg2;
                frame.Data[3] = arg3;
                frame.Data[4] = arg4;
                frame.Data[5] = arg5;
                frame.Data[6] = arg6;
                System.Management.Automation.Interpreter.ThreadLocal<InterpretedFrame>.StorageInfo currentFrame = frame.Enter();
                try
                {
                    this._interpreter.Run(frame);
                }
                finally
                {
                    frame.Leave(currentFrame);
                }
            }
        }

        internal void RunVoid8<T0, T1, T2, T3, T4, T5, T6, T7>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            if ((this._compiled != null) || this.TryGetCompiled())
            {
                ((Action<T0, T1, T2, T3, T4, T5, T6, T7>) this._compiled)(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
            }
            else
            {
                InterpretedFrame frame = this.MakeFrame();
                frame.Data[0] = arg0;
                frame.Data[1] = arg1;
                frame.Data[2] = arg2;
                frame.Data[3] = arg3;
                frame.Data[4] = arg4;
                frame.Data[5] = arg5;
                frame.Data[6] = arg6;
                frame.Data[7] = arg7;
                System.Management.Automation.Interpreter.ThreadLocal<InterpretedFrame>.StorageInfo currentFrame = frame.Enter();
                try
                {
                    this._interpreter.Run(frame);
                }
                finally
                {
                    frame.Leave(currentFrame);
                }
            }
        }

        internal void RunVoid9<T0, T1, T2, T3, T4, T5, T6, T7, T8>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            if ((this._compiled != null) || this.TryGetCompiled())
            {
                ((Action<T0, T1, T2, T3, T4, T5, T6, T7, T8>) this._compiled)(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
            }
            else
            {
                InterpretedFrame frame = this.MakeFrame();
                frame.Data[0] = arg0;
                frame.Data[1] = arg1;
                frame.Data[2] = arg2;
                frame.Data[3] = arg3;
                frame.Data[4] = arg4;
                frame.Data[5] = arg5;
                frame.Data[6] = arg6;
                frame.Data[7] = arg7;
                frame.Data[8] = arg8;
                System.Management.Automation.Interpreter.ThreadLocal<InterpretedFrame>.StorageInfo currentFrame = frame.Enter();
                try
                {
                    this._interpreter.Run(frame);
                }
                finally
                {
                    frame.Leave(currentFrame);
                }
            }
        }

        internal void RunVoidRef2<T0, T1>(ref T0 arg0, ref T1 arg1)
        {
            InterpretedFrame frame = this.MakeFrame();
            frame.Data[0] = (T0) arg0;
            frame.Data[1] = (T1) arg1;
            System.Management.Automation.Interpreter.ThreadLocal<InterpretedFrame>.StorageInfo currentFrame = frame.Enter();
            try
            {
                this._interpreter.Run(frame);
            }
            finally
            {
                frame.Leave(currentFrame);
                arg0 = (T0) frame.Data[0];
                arg1 = (T1) frame.Data[1];
            }
        }

        private bool TryGetCompiled()
        {
            if (this._delegateCreator.HasCompiled)
            {
                this._compiled = this._delegateCreator.CreateCompiledDelegate(this._closure);
                EventHandler<LightLambdaCompileEventArgs> compile = this.Compile;
                if ((compile != null) && this._delegateCreator.SameDelegateType)
                {
                    compile(this, new LightLambdaCompileEventArgs(this._compiled));
                }
                return true;
            }
            if (this._compilationThreshold-- == 0)
            {
                if (this._interpreter.CompileSynchronously)
                {
                    this._delegateCreator.Compile(null);
                    return this.TryGetCompiled();
                }
                ThreadPool.QueueUserWorkItem(new WaitCallback(this._delegateCreator.Compile), null);
            }
            return false;
        }
    }
}

