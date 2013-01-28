namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Collections.Generic;

    internal abstract class Instruction
    {
        public const int UnknownInstrIndex = 0x7fffffff;

        protected Instruction()
        {
        }

        public virtual object GetDebugCookie(LightCompiler compiler)
        {
            return null;
        }

        public abstract int Run(InterpretedFrame frame);
        public virtual string ToDebugString(int instructionIndex, object cookie, Func<int, int> labelIndexer, IList<object> objects)
        {
            return this.ToString();
        }

        public override string ToString()
        {
            return (this.InstructionName + "()");
        }

        public virtual int ConsumedContinuations
        {
            get
            {
                return 0;
            }
        }

        public virtual int ConsumedStack
        {
            get
            {
                return 0;
            }
        }

        public int ContinuationsBalance
        {
            get
            {
                return (this.ProducedContinuations - this.ConsumedContinuations);
            }
        }

        public virtual string InstructionName
        {
            get
            {
                return base.GetType().Name.Replace("Instruction", "");
            }
        }

        public virtual int ProducedContinuations
        {
            get
            {
                return 0;
            }
        }

        public virtual int ProducedStack
        {
            get
            {
                return 0;
            }
        }

        public int StackBalance
        {
            get
            {
                return (this.ProducedStack - this.ConsumedStack);
            }
        }
    }
}

