namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Runtime.CompilerServices;

    internal class DynamicInstruction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet> : Instruction
    {
        private CallSite<Func<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet>> _site;

        private DynamicInstruction(CallSite<Func<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet>> site)
        {
            this._site = site;
        }

        public static Instruction Factory(CallSiteBinder binder)
        {
            return new DynamicInstruction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet>(CallSite<Func<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet>>.Create(binder));
        }

        public override int Run(InterpretedFrame frame)
        {
            frame.Data[frame.StackIndex - 12] = this._site.Target(this._site, (T0) frame.Data[frame.StackIndex - 12], (T1) frame.Data[frame.StackIndex - 11], (T2) frame.Data[frame.StackIndex - 10], (T3) frame.Data[frame.StackIndex - 9], (T4) frame.Data[frame.StackIndex - 8], (T5) frame.Data[frame.StackIndex - 7], (T6) frame.Data[frame.StackIndex - 6], (T7) frame.Data[frame.StackIndex - 5], (T8) frame.Data[frame.StackIndex - 4], (T9) frame.Data[frame.StackIndex - 3], (T10) frame.Data[frame.StackIndex - 2], (T11) frame.Data[frame.StackIndex - 1]);
            frame.StackIndex -= 11;
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
                return 12;
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

