namespace System.Management.Automation.Interpreter
{
    using System;

    internal sealed class NotInstruction : Instruction
    {
        public static readonly Instruction Instance = new NotInstruction();

        private NotInstruction()
        {
        }

        public override int Run(InterpretedFrame frame)
        {
            frame.Push(((bool) frame.Pop()) ? ScriptingRuntimeHelpers.False : ScriptingRuntimeHelpers.True);
            return 1;
        }

        public override int ConsumedStack
        {
            get
            {
                return 1;
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

