namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class DynamicSplatInstruction : Instruction
    {
        private readonly int _argumentCount;
        private readonly CallSite<Func<CallSite, ArgumentArray, object>> _site;

        internal DynamicSplatInstruction(int argumentCount, CallSite<Func<CallSite, ArgumentArray, object>> site)
        {
            this._site = site;
            this._argumentCount = argumentCount;
        }

        public override int Run(InterpretedFrame frame)
        {
            int index = frame.StackIndex - this._argumentCount;
            frame.Data[index] = this._site.Target(this._site, new ArgumentArray(frame.Data, index, this._argumentCount));
            frame.StackIndex = index + 1;
            return 1;
        }

        public override string ToString()
        {
            return ("DynamicSplatInstruction(" + this._site + ")");
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
                return 1;
            }
        }
    }
}

