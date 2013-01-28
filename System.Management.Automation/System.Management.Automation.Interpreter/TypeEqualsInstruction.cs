namespace System.Management.Automation.Interpreter
{
    using System;

    internal sealed class TypeEqualsInstruction : Instruction
    {
        public static readonly TypeEqualsInstruction Instance = new TypeEqualsInstruction();

        private TypeEqualsInstruction()
        {
        }

        public override int Run(InterpretedFrame frame)
        {
            object obj2 = frame.Pop();
            object obj3 = frame.Pop();
            frame.Push(ScriptingRuntimeHelpers.BooleanToObject((obj3 != null) && (obj3.GetType() == obj2)));
            return 1;
        }

        public override int ConsumedStack
        {
            get
            {
                return 2;
            }
        }

        public override string InstructionName
        {
            get
            {
                return "TypeEquals()";
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

