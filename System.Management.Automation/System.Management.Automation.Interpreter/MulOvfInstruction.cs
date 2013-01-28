namespace System.Management.Automation.Interpreter
{
    using System;

    internal abstract class MulOvfInstruction : Instruction
    {
        private static Instruction _Double;
        private static Instruction _Int16;
        private static Instruction _Int32;
        private static Instruction _Int64;
        private static Instruction _Single;
        private static Instruction _UInt16;
        private static Instruction _UInt32;
        private static Instruction _UInt64;

        private MulOvfInstruction()
        {
        }

        public static Instruction Create(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Int16:
                    return (_Int16 ?? (_Int16 = new MulOvfInt16()));

                case TypeCode.UInt16:
                    return (_UInt16 ?? (_UInt16 = new MulOvfUInt16()));

                case TypeCode.Int32:
                    return (_Int32 ?? (_Int32 = new MulOvfInt32()));

                case TypeCode.UInt32:
                    return (_UInt32 ?? (_UInt32 = new MulOvfUInt32()));

                case TypeCode.Int64:
                    return (_Int64 ?? (_Int64 = new MulOvfInt64()));

                case TypeCode.UInt64:
                    return (_UInt64 ?? (_UInt64 = new MulOvfUInt64()));

                case TypeCode.Single:
                    return (_Single ?? (_Single = new MulOvfSingle()));

                case TypeCode.Double:
                    return (_Double ?? (_Double = new MulOvfDouble()));
            }
            throw Assert.Unreachable;
        }

        public override string ToString()
        {
            return "MulOvf()";
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

        internal sealed class MulOvfDouble : MulOvfInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                object obj2 = frame.Data[frame.StackIndex - 2];
                object obj3 = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = ((double) obj2) * ((double) obj3);
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class MulOvfInt16 : MulOvfInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                object obj2 = frame.Data[frame.StackIndex - 2];
                object obj3 = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = (short) (((short) obj2) * ((short) obj3));
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class MulOvfInt32 : MulOvfInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                object obj2 = frame.Data[frame.StackIndex - 2];
                object obj3 = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = ScriptingRuntimeHelpers.Int32ToObject(((int) obj2) * ((int) obj3));
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class MulOvfInt64 : MulOvfInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                object obj2 = frame.Data[frame.StackIndex - 2];
                object obj3 = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = ((long) obj2) * ((long) obj3);
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class MulOvfSingle : MulOvfInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                object obj2 = frame.Data[frame.StackIndex - 2];
                object obj3 = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = ((float) obj2) * ((float) obj3);
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class MulOvfUInt16 : MulOvfInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                object obj2 = frame.Data[frame.StackIndex - 2];
                object obj3 = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = (ushort) (((ushort) obj2) * ((ushort) obj3));
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class MulOvfUInt32 : MulOvfInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                object obj2 = frame.Data[frame.StackIndex - 2];
                object obj3 = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = ((int) obj2) * ((int) obj3);
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class MulOvfUInt64 : MulOvfInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                object obj2 = frame.Data[frame.StackIndex - 2];
                object obj3 = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = (ulong) (((short) obj2) * ((short) obj3));
                frame.StackIndex--;
                return 1;
            }
        }
    }
}

