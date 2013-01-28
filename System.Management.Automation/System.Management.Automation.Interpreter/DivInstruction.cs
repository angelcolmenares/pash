namespace System.Management.Automation.Interpreter
{
    using System;

    internal abstract class DivInstruction : Instruction
    {
        private static Instruction _Double;
        private static Instruction _Int16;
        private static Instruction _Int32;
        private static Instruction _Int64;
        private static Instruction _Single;
        private static Instruction _UInt16;
        private static Instruction _UInt32;
        private static Instruction _UInt64;

        private DivInstruction()
        {
        }

        public static Instruction Create(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Int16:
                    return (_Int16 ?? (_Int16 = new DivInt16()));

                case TypeCode.UInt16:
                    return (_UInt16 ?? (_UInt16 = new DivUInt16()));

                case TypeCode.Int32:
                    return (_Int32 ?? (_Int32 = new DivInt32()));

                case TypeCode.UInt32:
                    return (_UInt32 ?? (_UInt32 = new DivUInt32()));

                case TypeCode.Int64:
                    return (_Int64 ?? (_Int64 = new DivInt64()));

                case TypeCode.UInt64:
                    return (_UInt64 ?? (_UInt64 = new DivUInt64()));

                case TypeCode.Single:
                    return (_Single ?? (_Single = new DivSingle()));

                case TypeCode.Double:
                    return (_Double ?? (_Double = new DivDouble()));
            }
            throw Assert.Unreachable;
        }

        public override string ToString()
        {
            return "Div()";
        }

        public override int ConsumedStack
        {
            get
            {
                return 2;
            }
        }

        public override int ProducedStack
        {
            get
            {
                return 1;
            }
        }

        internal sealed class DivDouble : DivInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                object obj2 = frame.Data[frame.StackIndex - 2];
                object obj3 = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = ((double) obj2) / ((double) obj3);
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class DivInt16 : DivInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                object obj2 = frame.Data[frame.StackIndex - 2];
                object obj3 = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = (short) (((short) obj2) / ((short) obj3));
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class DivInt32 : DivInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                object obj2 = frame.Data[frame.StackIndex - 2];
                object obj3 = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = ScriptingRuntimeHelpers.Int32ToObject(((int) obj2) / ((int) obj3));
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class DivInt64 : DivInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                object obj2 = frame.Data[frame.StackIndex - 2];
                object obj3 = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = ((long) obj2) / ((long) obj3);
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class DivSingle : DivInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                object obj2 = frame.Data[frame.StackIndex - 2];
                object obj3 = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = ((float) obj2) / ((float) obj3);
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class DivUInt16 : DivInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                object obj2 = frame.Data[frame.StackIndex - 2];
                object obj3 = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = (ushort) (((ushort) obj2) / ((ushort) obj3));
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class DivUInt32 : DivInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                object obj2 = frame.Data[frame.StackIndex - 2];
                object obj3 = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = ((int) obj2) / ((int) obj3);
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class DivUInt64 : DivInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                object obj2 = frame.Data[frame.StackIndex - 2];
                object obj3 = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = (ulong) (((short) obj2) / ((short) obj3));
                frame.StackIndex--;
                return 1;
            }
        }
    }
}

