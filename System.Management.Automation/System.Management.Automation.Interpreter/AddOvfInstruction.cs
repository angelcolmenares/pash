namespace System.Management.Automation.Interpreter
{
    using System;

    internal abstract class AddOvfInstruction : Instruction
    {
        private static Instruction _Double;
        private static Instruction _Int16;
        private static Instruction _Int32;
        private static Instruction _Int64;
        private static Instruction _Single;
        private static Instruction _UInt16;
        private static Instruction _UInt32;
        private static Instruction _UInt64;

        private AddOvfInstruction()
        {
        }

        public static Instruction Create(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Int16:
                    return (_Int16 ?? (_Int16 = new AddOvfInt16()));

                case TypeCode.UInt16:
                    return (_UInt16 ?? (_UInt16 = new AddOvfUInt16()));

                case TypeCode.Int32:
                    return (_Int32 ?? (_Int32 = new AddOvfInt32()));

                case TypeCode.UInt32:
                    return (_UInt32 ?? (_UInt32 = new AddOvfUInt32()));

                case TypeCode.Int64:
                    return (_Int64 ?? (_Int64 = new AddOvfInt64()));

                case TypeCode.UInt64:
                    return (_UInt64 ?? (_UInt64 = new AddOvfUInt64()));

                case TypeCode.Single:
                    return (_Single ?? (_Single = new AddOvfSingle()));

                case TypeCode.Double:
                    return (_Double ?? (_Double = new AddOvfDouble()));
            }
            throw Assert.Unreachable;
        }

        public override string ToString()
        {
            return "AddOvf()";
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

        internal sealed class AddOvfDouble : AddOvfInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                object obj2 = frame.Data[frame.StackIndex - 2];
                object obj3 = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = ((double) obj2) + ((double) obj3);
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class AddOvfInt16 : AddOvfInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                object obj2 = frame.Data[frame.StackIndex - 2];
                object obj3 = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = (short) (((short) obj2) + ((short) obj3));
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class AddOvfInt32 : AddOvfInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                object obj2 = frame.Data[frame.StackIndex - 2];
                object obj3 = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = ScriptingRuntimeHelpers.Int32ToObject(((int) obj2) + ((int) obj3));
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class AddOvfInt64 : AddOvfInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                object obj2 = frame.Data[frame.StackIndex - 2];
                object obj3 = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = ((long) obj2) + ((long) obj3);
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class AddOvfSingle : AddOvfInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                object obj2 = frame.Data[frame.StackIndex - 2];
                object obj3 = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = ((float) obj2) + ((float) obj3);
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class AddOvfUInt16 : AddOvfInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                object obj2 = frame.Data[frame.StackIndex - 2];
                object obj3 = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = (ushort) (((ushort) obj2) + ((ushort) obj3));
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class AddOvfUInt32 : AddOvfInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                object obj2 = frame.Data[frame.StackIndex - 2];
                object obj3 = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = ((int) obj2) + ((int) obj3);
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class AddOvfUInt64 : AddOvfInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                object obj2 = frame.Data[frame.StackIndex - 2];
                object obj3 = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = (ulong) (((short) obj2) + ((short) obj3));
                frame.StackIndex--;
                return 1;
            }
        }
    }
}

