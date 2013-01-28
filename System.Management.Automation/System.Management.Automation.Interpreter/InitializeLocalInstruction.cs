namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal abstract class InitializeLocalInstruction : LocalAccessInstruction
    {
        internal InitializeLocalInstruction(int index) : base(index)
        {
        }

        internal sealed class ImmutableBox : InitializeLocalInstruction
        {
            private readonly object _defaultValue;

            internal ImmutableBox(int index, object defaultValue) : base(index)
            {
                this._defaultValue = defaultValue;
            }

            public override int Run(InterpretedFrame frame)
            {
                frame.Data[base._index] = new StrongBox<object>(this._defaultValue);
                return 1;
            }

            public override string InstructionName
            {
                get
                {
                    return "InitImmutableBox";
                }
            }
        }

        internal sealed class ImmutableValue : InitializeLocalInstruction, IBoxableInstruction
        {
            private readonly object _defaultValue;

            internal ImmutableValue(int index, object defaultValue) : base(index)
            {
                this._defaultValue = defaultValue;
            }

            public Instruction BoxIfIndexMatches(int index)
            {
                if (index != base._index)
                {
                    return null;
                }
                return new InitializeLocalInstruction.ImmutableBox(index, this._defaultValue);
            }

            public override int Run(InterpretedFrame frame)
            {
                frame.Data[base._index] = this._defaultValue;
                return 1;
            }

            public override string InstructionName
            {
                get
                {
                    return "InitImmutableValue";
                }
            }
        }

        internal sealed class MutableBox : InitializeLocalInstruction
        {
            private readonly Type _type;

            internal MutableBox(int index, Type type) : base(index)
            {
                this._type = type;
            }

            public override int Run(InterpretedFrame frame)
            {
                frame.Data[base._index] = new StrongBox<object>(Activator.CreateInstance(this._type));
                return 1;
            }

            public override string InstructionName
            {
                get
                {
                    return "InitMutableBox";
                }
            }
        }

        internal sealed class MutableValue : InitializeLocalInstruction, IBoxableInstruction
        {
            private readonly Type _type;

            internal MutableValue(int index, Type type) : base(index)
            {
                this._type = type;
            }

            public Instruction BoxIfIndexMatches(int index)
            {
                if (index != base._index)
                {
                    return null;
                }
                return new InitializeLocalInstruction.MutableBox(index, this._type);
            }

            public override int Run(InterpretedFrame frame)
            {
                try
                {
                    frame.Data[base._index] = Activator.CreateInstance(this._type);
                }
                catch (TargetInvocationException exception)
                {
                    ExceptionHelpers.UpdateForRethrow(exception.InnerException);
                    throw exception.InnerException;
                }
                return 1;
            }

            public override string InstructionName
            {
                get
                {
                    return "InitMutableValue";
                }
            }
        }

        internal sealed class Parameter : InitializeLocalInstruction, IBoxableInstruction
        {
            internal Parameter(int index) : base(index)
            {
            }

            public Instruction BoxIfIndexMatches(int index)
            {
                if (index == base._index)
                {
                    return InstructionList.ParameterBox(index);
                }
                return null;
            }

            public override int Run(InterpretedFrame frame)
            {
                return 1;
            }

            public override string InstructionName
            {
                get
                {
                    return "InitParameter";
                }
            }
        }

        internal sealed class ParameterBox : InitializeLocalInstruction
        {
            public ParameterBox(int index) : base(index)
            {
            }

            public override int Run(InterpretedFrame frame)
            {
                frame.Data[base._index] = new StrongBox<object>(frame.Data[base._index]);
                return 1;
            }
        }

        internal sealed class Reference : InitializeLocalInstruction, IBoxableInstruction
        {
            internal Reference(int index) : base(index)
            {
            }

            public Instruction BoxIfIndexMatches(int index)
            {
                if (index != base._index)
                {
                    return null;
                }
                return InstructionList.InitImmutableRefBox(index);
            }

            public override int Run(InterpretedFrame frame)
            {
                frame.Data[base._index] = null;
                return 1;
            }

            public override string InstructionName
            {
                get
                {
                    return "InitRef";
                }
            }
        }
    }
}

