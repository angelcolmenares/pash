namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Reflection;

    internal sealed class ActionCallInstruction : CallInstruction
    {
        private readonly Action _target;

        public ActionCallInstruction(Action target)
        {
            this._target = target;
        }

        public ActionCallInstruction(MethodInfo target)
        {
            this._target = (Action) Delegate.CreateDelegate(typeof(Action), target);
        }

        public override object Invoke()
        {
            this._target();
            return null;
        }

        public override int Run(InterpretedFrame frame)
        {
            this._target();
            frame.StackIndex = frame.StackIndex;
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

