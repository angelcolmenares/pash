namespace System.Management.Automation.Interpreter
{
    using System;

    internal abstract class LessThanInstruction : Instruction
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

        private LessThanInstruction()
        {
        }

        public static Instruction Create(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Char:
                    return (_Char ?? (_Char = new LessThanChar()));

                case TypeCode.SByte:
                    return (_SByte ?? (_SByte = new LessThanSByte()));

                case TypeCode.Byte:
                    return (_Byte ?? (_Byte = new LessThanByte()));

                case TypeCode.Int16:
                    return (_Int16 ?? (_Int16 = new LessThanInt16()));

                case TypeCode.UInt16:
                    return (_UInt16 ?? (_UInt16 = new LessThanUInt16()));

                case TypeCode.Int32:
                    return (_Int32 ?? (_Int32 = new LessThanInt32()));

                case TypeCode.UInt32:
                    return (_UInt32 ?? (_UInt32 = new LessThanUInt32()));

                case TypeCode.Int64:
                    return (_Int64 ?? (_Int64 = new LessThanInt64()));

                case TypeCode.UInt64:
                    return (_UInt64 ?? (_UInt64 = new LessThanUInt64()));

                case TypeCode.Single:
                    return (_Single ?? (_Single = new LessThanSingle()));

                case TypeCode.Double:
                    return (_Double ?? (_Double = new LessThanDouble()));
            }
            throw Assert.Unreachable;
        }

        public override string ToString()
        {
            return "LessThan()";
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

        internal sealed class LessThanByte : LessThanInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                byte num = (byte) frame.Pop();
                frame.Push(((byte) frame.Pop()) < num);
                return 1;
            }
        }

        internal sealed class LessThanChar : LessThanInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                char ch = (char) frame.Pop();
                frame.Push(((char) frame.Pop()) < ch);
                return 1;
            }
        }

        internal sealed class LessThanDouble : LessThanInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                double num = (double) frame.Pop();
                frame.Push(((double) frame.Pop()) < num);
                return 1;
            }
        }

        internal sealed class LessThanInt16 : LessThanInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                short num = (short) frame.Pop();
                frame.Push(((short) frame.Pop()) < num);
                return 1;
            }
        }

        internal sealed class LessThanInt32 : LessThanInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                int num = (int) frame.Pop();
                frame.Push(((int) frame.Pop()) < num);
                return 1;
            }
        }

        internal sealed class LessThanInt64 : LessThanInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                long num = (long) frame.Pop();
                frame.Push(((long) frame.Pop()) < num);
                return 1;
            }
        }

        internal sealed class LessThanSByte : LessThanInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                sbyte num = (sbyte) frame.Pop();
                frame.Push(((sbyte) frame.Pop()) < num);
                return 1;
            }
        }

        internal sealed class LessThanSingle : LessThanInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                float num = (float) frame.Pop();
                frame.Push(((float) frame.Pop()) < num);
                return 1;
            }
        }

        internal sealed class LessThanUInt16 : LessThanInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                ushort num = (ushort) frame.Pop();
                frame.Push(((ushort) frame.Pop()) < num);
                return 1;
            }
        }

        internal sealed class LessThanUInt32 : LessThanInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                int num = (int) frame.Pop();
                frame.Push(((int) frame.Pop()) < num);
                return 1;
            }
        }

        internal sealed class LessThanUInt64 : LessThanInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                ulong num = (ulong) frame.Pop();
                frame.Push(((ulong) frame.Pop()) < num);
                return 1;
            }
        }
    }
}

