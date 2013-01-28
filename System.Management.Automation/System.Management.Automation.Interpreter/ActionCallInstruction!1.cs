namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Reflection;

    internal sealed class ActionCallInstruction<T0> : CallInstruction
    {
        private readonly Action<T0> _target;

        public ActionCallInstruction(Action<T0> target)
        {
            this._target = target;
        }

        public ActionCallInstruction(MethodInfo target)
        {
            this._target = (Action<T0>) Delegate.CreateDelegate(typeof(Action<T0>), target);
        }

        public override object Invoke(object arg0)
        {
            this._target((arg0 != null) ? ((T0) arg0) : default(T0));
            return null;
        }

        public override int Run(InterpretedFrame frame)
        {
            this._target((T0) frame.Data[frame.StackIndex - 1]);
            frame.StackIndex--;
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

