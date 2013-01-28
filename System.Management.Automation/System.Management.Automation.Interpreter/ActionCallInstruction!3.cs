namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Reflection;

    internal sealed class ActionCallInstruction<T0, T1, T2> : CallInstruction
    {
        private readonly Action<T0, T1, T2> _target;

        public ActionCallInstruction(Action<T0, T1, T2> target)
        {
            this._target = target;
        }

        public ActionCallInstruction(MethodInfo target)
        {
            this._target = (Action<T0, T1, T2>) Delegate.CreateDelegate(typeof(Action<T0, T1, T2>), target);
        }

        public override object Invoke(object arg0, object arg1, object arg2)
        {
            this._target((arg0 != null) ? ((T0) arg0) : default(T0), (arg1 != null) ? ((T1) arg1) : default(T1), (arg2 != null) ? ((T2) arg2) : default(T2));
            return null;
        }

        public override int Run(InterpretedFrame frame)
        {
            this._target((T0) frame.Data[frame.StackIndex - 3], (T1) frame.Data[frame.StackIndex - 2], (T2) frame.Data[frame.StackIndex - 1]);
            frame.StackIndex -= 3;
            return 1;
        }

        public override int ArgumentCount
        {
            get
            {
                return 3;
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

