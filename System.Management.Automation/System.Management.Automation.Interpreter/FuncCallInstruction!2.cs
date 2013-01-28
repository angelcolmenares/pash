namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Reflection;

    internal sealed class FuncCallInstruction<T0, TRet> : CallInstruction
    {
        private readonly Func<T0, TRet> _target;

        public FuncCallInstruction(Func<T0, TRet> target)
        {
            this._target = target;
        }

        public FuncCallInstruction(MethodInfo target)
        {
            this._target = (Func<T0, TRet>) Delegate.CreateDelegate(typeof(Func<T0, TRet>), target);
        }

        public override object Invoke(object arg0)
        {
            return this._target((arg0 != null) ? ((T0) arg0) : default(T0));
        }

        public override int Run(InterpretedFrame frame)
        {
            frame.Data[frame.StackIndex - 1] = this._target((T0) frame.Data[frame.StackIndex - 1]);
            frame.StackIndex = frame.StackIndex;
            return 1;
        }

        public override int ArgumentCount
        {
            get
            {
                return 1;
            }
        }

        public override MethodInfo Info
        {
            get
            {
                return this._target.Method;
            }
        }
    }
}

