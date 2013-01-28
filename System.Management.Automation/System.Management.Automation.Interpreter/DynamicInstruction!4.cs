namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Runtime.CompilerServices;

    internal class DynamicInstruction<T0, T1, T2, TRet> : Instruction
    {
        private CallSite<Func<CallSite, T0, T1, T2, TRet>> _site;

        private DynamicInstruction(CallSite<Func<CallSite, T0, T1, T2, TRet>> site)
        {
            this._site = site;
        }

        public static Instruction Factory(CallSiteBinder binder)
        {
            return new DynamicInstruction<T0, T1, T2, TRet>(CallSite<Func<CallSite, T0, T1, T2, TRet>>.Create(binder));
        }

        public override int Run(InterpretedFrame frame)
        {
            frame.Data[frame.StackIndex - 3] = this._site.Target(this._site, (T0) frame.Data[frame.StackIndex - 3], (T1) frame.Data[frame.StackIndex - 2], (T2) frame.Data[frame.StackIndex - 1]);
            frame.StackIndex -= 2;
            return 1;
        }

        public override string ToString()
        {
            return ("Dynamic(" + this._site.Binder.ToString() + ")");
        }

        public override int ConsumedStack
        {
            get
            {
                return 3;
            }
        }

        public override int ProducedStack
        {
            get
            {
                return 1;
            }
        }
    }
}

