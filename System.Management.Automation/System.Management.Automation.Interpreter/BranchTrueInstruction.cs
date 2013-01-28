namespace System.Management.Automation.Interpreter
{
    using System;

    internal sealed class BranchTrueInstruction : OffsetInstruction
    {
        private static Instruction[] _cache;

        internal BranchTrueInstruction()
        {
        }

        public override int Run(InterpretedFrame frame)
        {
            if ((bool) frame.Pop())
            {
                return base._offset;
            }
            return 1;
        }

        public override Instruction[] Cache
        {
            get
            {
                if (_cache == null)
                {
                    _cache = new Instruction[0x20];
                }
                return _cache;
            }
        }

        public override int ConsumedStack
        {
            get
            {
                return 1;
            }
        }
    }
}

