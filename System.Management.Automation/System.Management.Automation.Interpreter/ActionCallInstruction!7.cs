namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Reflection;

    internal sealed class ActionCallInstruction<T0, T1, T2, T3, T4, T5, T6> : CallInstruction
    {
        private readonly Action<T0, T1, T2, T3, T4, T5, T6> _target;

        public ActionCallInstruction(Action<T0, T1, T2, T3, T4, T5, T6> target)
        {
            this._target = target;
        }

        public ActionCallInstruction(MethodInfo target)
        {
            this._target = (Action<T0, T1, T2, T3, T4, T5, T6>) Delegate.CreateDelegate(typeof(Action<T0, T1, T2, T3, T4, T5, T6>), target);
        }

        public override object Invoke(object arg0, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6)
        {
            this._target((arg0 != null) ? ((T0) arg0) : default(T0), (arg1 != null) ? ((T1) arg1) : default(T1), (arg2 != null) ? ((T2) arg2) : default(T2), (arg3 != null) ? ((T3) arg3) : default(T3), (arg4 != null) ? ((T4) arg4) : default(T4), (arg5 != null) ? ((T5) arg5) : default(T5), (arg6 != null) ? ((T6) arg6) : default(T6));
            return null;
        }

        public override int Run(InterpretedFrame frame)
        {
            this._target((T0) frame.Data[frame.StackIndex - 7], (T1) frame.Data[frame.StackIndex - 6], (T2) frame.Data[frame.StackIndex - 5], (T3) frame.Data[frame.StackIndex - 4], (T4) frame.Data[frame.StackIndex - 3], (T5) frame.Data[frame.StackIndex - 2], (T6) frame.Data[frame.StackIndex - 1]);
            frame.StackIndex -= 7;
            return 1;
        }

        public override int ArgumentCount
        {
            get
            {
                return 7;
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

