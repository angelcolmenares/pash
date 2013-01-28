namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Reflection;

    internal sealed class FuncCallInstruction<TRet> : CallInstruction
    {
        private readonly Func<TRet> _target;

        public FuncCallInstruction(Func<TRet> target)
        {
            this._target = target;
        }

        public FuncCallInstruction(MethodInfo target)
        {
            this._target = (Func<TRet>) Delegate.CreateDelegate(typeof(Func<TRet>), target);
        }

        public override object Invoke()
        {
            return this._target();
        }

        public override int Run(InterpretedFrame frame)
        {
            frame.Data[frame.StackIndex] = this._target();
            frame.StackIndex -= -1;
            return 1;
        }

        public override int ArgumentCount
        {
            get
            {
                return 0;
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

