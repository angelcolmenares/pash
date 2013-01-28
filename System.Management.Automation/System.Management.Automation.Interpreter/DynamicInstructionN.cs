namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal sealed class DynamicInstructionN : Instruction
    {
        private readonly int _argumentCount;
        private readonly bool _isVoid;
        private readonly CallSite _site;
        private readonly CallInstruction _target;
        private readonly object _targetDelegate;

        public DynamicInstructionN(Type delegateType, CallSite site)
        {
            MethodInfo method = delegateType.GetMethod("Invoke");
            ParameterInfo[] parameters = method.GetParameters();
            this._target = CallInstruction.Create(method, parameters);
            this._site = site;
            this._argumentCount = parameters.Length - 1;
            this._targetDelegate = site.GetType().GetField("Target").GetValue(site);
        }

        public DynamicInstructionN(Type delegateType, CallSite site, bool isVoid) : this(delegateType, site)
        {
            this._isVoid = isVoid;
        }

        internal static Instruction CreateUntypedInstruction(CallSiteBinder binder, int argCount)
        {
            switch (argCount)
            {
                case 0:
                    return DynamicInstruction<object>.Factory(binder);

                case 1:
                    return DynamicInstruction<object, object>.Factory(binder);

                case 2:
                    return DynamicInstruction<object, object, object>.Factory(binder);

                case 3:
                    return DynamicInstruction<object, object, object, object>.Factory(binder);

                case 4:
                    return DynamicInstruction<object, object, object, object, object>.Factory(binder);

                case 5:
                    return DynamicInstruction<object, object, object, object, object, object>.Factory(binder);

                case 6:
                    return DynamicInstruction<object, object, object, object, object, object, object>.Factory(binder);

                case 7:
                    return DynamicInstruction<object, object, object, object, object, object, object, object>.Factory(binder);

                case 8:
                    return DynamicInstruction<object, object, object, object, object, object, object, object, object>.Factory(binder);

                case 9:
                    return DynamicInstruction<object, object, object, object, object, object, object, object, object, object>.Factory(binder);

                case 10:
                    return DynamicInstruction<object, object, object, object, object, object, object, object, object, object, object>.Factory(binder);

                case 11:
                    return DynamicInstruction<object, object, object, object, object, object, object, object, object, object, object, object>.Factory(binder);

                case 12:
                    return DynamicInstruction<object, object, object, object, object, object, object, object, object, object, object, object, object>.Factory(binder);

                case 13:
                    return DynamicInstruction<object, object, object, object, object, object, object, object, object, object, object, object, object, object>.Factory(binder);

                case 14:
                    return DynamicInstruction<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>.Factory(binder);

                case 15:
                    return DynamicInstruction<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>.Factory(binder);
            }
            return null;
        }

        internal static Type GetDynamicInstructionType(Type delegateType)
        {
            Type type;
            Type[] genericArguments = delegateType.GetGenericArguments();
            if (genericArguments.Length == 0)
            {
                return null;
            }
            Type[] typeArguments = genericArguments.Skip<Type>(1).ToArray<Type>();
            switch (typeArguments.Length)
            {
                case 1:
                    type = typeof(DynamicInstruction<>);
                    break;

                case 2:
                    type = typeof(DynamicInstruction<,>);
                    break;

                case 3:
                    type = typeof(DynamicInstruction<,,>);
                    break;

                case 4:
                    type = typeof(DynamicInstruction<,,,>);
                    break;

                case 5:
                    type = typeof(DynamicInstruction<,,,,>);
                    break;

                case 6:
                    type = typeof(DynamicInstruction<,,,,,>);
                    break;

                case 7:
                    type = typeof(DynamicInstruction<,,,,,,>);
                    break;

                case 8:
                    type = typeof(DynamicInstruction<,,,,,,,>);
                    break;

                case 9:
                    type = typeof(DynamicInstruction<,,,,,,,,>);
                    break;

                case 10:
                    type = typeof(DynamicInstruction<,,,,,,,,,>);
                    break;

                case 11:
                    type = typeof(DynamicInstruction<,,,,,,,,,,>);
                    break;

                case 12:
                    type = typeof(DynamicInstruction<,,,,,,,,,,,>);
                    break;

                case 13:
                    type = typeof(DynamicInstruction<,,,,,,,,,,,,>);
                    break;

                case 14:
                    type = typeof(DynamicInstruction<,,,,,,,,,,,,,>);
                    break;

                case 15:
                    type = typeof(DynamicInstruction<,,,,,,,,,,,,,,>);
                    break;

                case 0x10:
                    type = typeof(DynamicInstruction<,,,,,,,,,,,,,,,>);
                    break;

                default:
                    throw Assert.Unreachable;
            }
            return type.MakeGenericType(typeArguments);
        }

        public override int Run(InterpretedFrame frame)
        {
            int index = frame.StackIndex - this._argumentCount;
            object[] args = new object[1 + this._argumentCount];
            args[0] = this._site;
            for (int i = 0; i < this._argumentCount; i++)
            {
                args[1 + i] = frame.Data[index + i];
            }
            object obj2 = this._target.InvokeInstance(this._targetDelegate, args);
            if (this._isVoid)
            {
                frame.StackIndex = index;
            }
            else
            {
                frame.Data[index] = obj2;
                frame.StackIndex = index + 1;
            }
            return 1;
        }

        public override string ToString()
        {
            return ("DynamicInstructionN(" + this._site + ")");
        }

        public override int ConsumedStack
        {
            get
            {
                return this._argumentCount;
            }
        }

        public override int ProducedStack
        {
            get
            {
                if (!this._isVoid)
                {
                    return 1;
                }
                return 0;
            }
        }
    }
}

