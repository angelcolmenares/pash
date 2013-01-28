namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Reflection;

    internal sealed class MethodInfoCallInstruction : CallInstruction
    {
        private readonly int _argumentCount;
        private readonly MethodInfo _target;

        internal MethodInfoCallInstruction(MethodInfo target, int argumentCount)
        {
            this._target = target;
            this._argumentCount = argumentCount;
        }

        private static object[] GetNonStaticArgs(object[] args)
        {
            object[] objArray = new object[args.Length - 1];
            for (int i = 0; i < objArray.Length; i++)
            {
                objArray[i] = args[i + 1];
            }
            return objArray;
        }

        public override object Invoke()
        {
            return this.InvokeWorker(new object[0]);
        }

        public override object Invoke(object arg0)
        {
            return this.InvokeWorker(new object[] { arg0 });
        }

        public override object Invoke(params object[] args)
        {
            return this.InvokeWorker(args);
        }

        public override object Invoke(object arg0, object arg1)
        {
            return this.InvokeWorker(new object[] { arg0, arg1 });
        }

        public override object InvokeInstance(object instance, params object[] args)
        {
            object obj2;
            if (this._target.IsStatic)
            {
                try
                {
                    return this._target.Invoke(null, args);
                }
                catch (TargetInvocationException exception)
                {
                    throw ExceptionHelpers.UpdateForRethrow(exception.InnerException);
                }
            }
            try
            {
                obj2 = this._target.Invoke(instance, args);
            }
            catch (TargetInvocationException exception2)
            {
                throw ExceptionHelpers.UpdateForRethrow(exception2.InnerException);
            }
            return obj2;
        }

        private object InvokeWorker(params object[] args)
        {
            object obj2;
            if (this._target.IsStatic)
            {
                try
                {
                    return this._target.Invoke(null, args);
                }
                catch (TargetInvocationException exception)
                {
                    throw ExceptionHelpers.UpdateForRethrow(exception.InnerException);
                }
            }
            try
            {
                obj2 = this._target.Invoke(args[0], GetNonStaticArgs(args));
            }
            catch (TargetInvocationException exception2)
            {
                throw ExceptionHelpers.UpdateForRethrow(exception2.InnerException);
            }
            return obj2;
        }

        public sealed override int Run(InterpretedFrame frame)
        {
            int index = frame.StackIndex - this._argumentCount;
            object[] args = new object[this._argumentCount];
            for (int i = 0; i < args.Length; i++)
            {
                args[i] = frame.Data[index + i];
            }
            object obj2 = this.Invoke(args);
            if (this._target.ReturnType != typeof(void))
            {
                frame.Data[index] = obj2;
                frame.StackIndex = index + 1;
            }
            else
            {
                frame.StackIndex = index;
            }
            return 1;
        }

        public override int ArgumentCount
        {
            get
            {
                return this._argumentCount;
            }
        }

        public override MethodInfo Info
        {
            get
            {
                return this._target;
            }
        }
    }
}

