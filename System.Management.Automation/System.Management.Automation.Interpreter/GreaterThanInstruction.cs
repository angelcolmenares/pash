namespace System.Management.Automation.Interpreter
{
    using System;

    internal abstract class GreaterThanInstruction : Instruction
    {
        private static Instruction _Byte;
        private static Instruction _Char;
        private static Instruction _Double;
        private static Instruction _Int16;
        private static Instruction _Int32;
        private static Instruction _Int64;
        private static Instruction _SByte;
        private static Instruction _Single;
        private static Instruction _UInt16;
        private static Instruction _UInt32;
        private static Instruction _UInt64;

        private GreaterThanInstruction()
        {
        }

        public static Instruction Create(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Char:
                    return (_Char ?? (_Char = new GreaterThanChar()));

                case TypeCode.SByte:
                    return (_SByte ?? (_SByte = new GreaterThanSByte()));

                case TypeCode.Byte:
                    return (_Byte ?? (_Byte = new GreaterThanByte()));

                case TypeCode.Int16:
                    return (_Int16 ?? (_Int16 = new GreaterThanInt16()));

                case TypeCode.UInt16:
                    return (_UInt16 ?? (_UInt16 = new GreaterThanUInt16()));

                case TypeCode.Int32:
                    return (_Int32 ?? (_Int32 = new GreaterThanInt32()));

                case TypeCode.UInt32:
                    return (_UInt32 ?? (_UInt32 = new GreaterThanUInt32()));

                case TypeCode.Int64:
                    return (_Int64 ?? (_Int64 = new GreaterThanInt64()));

                case TypeCode.UInt64:
                    return (_UInt64 ?? (_UInt64 = new GreaterThanUInt64()));

                case TypeCode.Single:
                    return (_Single ?? (_Single = new GreaterThanSingle()));

                case TypeCode.Double:
                    return (_Double ?? (_Double = new GreaterThanDouble()));
            }
            throw Assert.Unreachable;
        }

        public override string ToString()
        {
            return "GreaterThan()";
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

        internal sealed class GreaterThanByte : GreaterThanInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                byte num = (byte) frame.Pop();
                frame.Push(((byte) frame.Pop()) > num);
                return 1;
            }
        }

        internal sealed class GreaterThanChar : GreaterThanInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                char ch = (char) frame.Pop();
                frame.Push(((char) frame.Pop()) > ch);
                return 1;
            }
        }

        internal sealed class GreaterThanDouble : GreaterThanInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                double num = (double) frame.Pop();
                frame.Push(((double) frame.Pop()) > num);
                return 1;
            }
        }

        internal sealed class GreaterThanInt16 : GreaterThanInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                short num = (short) frame.Pop();
                frame.Push(((short) frame.Pop()) > num);
                return 1;
            }
        }

        internal sealed class GreaterThanInt32 : GreaterThanInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                int num = (int) frame.Pop();
                frame.Push(((int) frame.Pop()) > num);
                return 1;
            }
        }

        internal sealed class GreaterThanInt64 : GreaterThanInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                long num = (long) frame.Pop();
                frame.Push(((long) frame.Pop()) > num);
                return 1;
            }
        }

        internal sealed class GreaterThanSByte : GreaterThanInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                sbyte num = (sbyte) frame.Pop();
                frame.Push(((sbyte) frame.Pop()) > num);
                return 1;
            }
        }

        internal sealed class GreaterThanSingle : GreaterThanInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                float num = (float) frame.Pop();
                frame.Push(((float) frame.Pop()) > num);
                return 1;
            }
        }

        internal sealed class GreaterThanUInt16 : GreaterThanInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                ushort num = (ushort) frame.Pop();
                frame.Push(((ushort) frame.Pop()) > num);
                return 1;
            }
        }

        internal sealed class GreaterThanUInt32 : GreaterThanInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                int num = (int) frame.Pop();
                frame.Push(((int) frame.Pop()) > num);
                return 1;
            }
        }

        internal sealed class GreaterThanUInt64 : GreaterThanInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                ulong num = (ulong) frame.Pop();
                frame.Push(((ulong) frame.Pop()) > num);
                return 1;
            }
        }
    }
}

