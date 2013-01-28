namespace System.Management.Automation.Interpreter
{
    using System;

    internal abstract class NotEqualInstruction : Instruction
    {
        private static Instruction _Boolean;
        private static Instruction _Byte;
        private static Instruction _Char;
        private static Instruction _Double;
        private static Instruction _Int16;
        private static Instruction _Int32;
        private static Instruction _Int64;
        private static Instruction _Reference;
        private static Instruction _SByte;
        private static Instruction _Single;
        private static Instruction _UInt16;
        private static Instruction _UInt32;
        private static Instruction _UInt64;

        private NotEqualInstruction()
        {
        }

        public static Instruction Create(Type type)
        {
            switch (Type.GetTypeCode(type.IsEnum ? Enum.GetUnderlyingType(type) : type))
            {
                case TypeCode.Object:
                    if (type.IsValueType)
                    {
                        throw new NotImplementedException();
                    }
                    return (_Reference ?? (_Reference = new NotEqualReference()));

                case TypeCode.Boolean:
                    return (_Boolean ?? (_Boolean = new NotEqualBoolean()));

                case TypeCode.Char:
                    return (_Char ?? (_Char = new NotEqualChar()));

                case TypeCode.SByte:
                    return (_SByte ?? (_SByte = new NotEqualSByte()));

                case TypeCode.Byte:
                    return (_Byte ?? (_Byte = new NotEqualByte()));

                case TypeCode.Int16:
                    return (_Int16 ?? (_Int16 = new NotEqualInt16()));

                case TypeCode.UInt16:
                    return (_UInt16 ?? (_UInt16 = new NotEqualInt16()));

                case TypeCode.Int32:
                    return (_Int32 ?? (_Int32 = new NotEqualInt32()));

                case TypeCode.UInt32:
                    return (_UInt32 ?? (_UInt32 = new NotEqualInt32()));

                case TypeCode.Int64:
                    return (_Int64 ?? (_Int64 = new NotEqualInt64()));

                case TypeCode.UInt64:
                    return (_UInt64 ?? (_UInt64 = new NotEqualInt64()));

                case TypeCode.Single:
                    return (_Single ?? (_Single = new NotEqualSingle()));

                case TypeCode.Double:
                    return (_Double ?? (_Double = new NotEqualDouble()));
            }
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "NotEqual()";
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

        internal sealed class NotEqualBoolean : NotEqualInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                frame.Push(((bool) frame.Pop()) != ((bool) frame.Pop()));
                return 1;
            }
        }

        internal sealed class NotEqualByte : NotEqualInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                frame.Push(((byte) frame.Pop()) != ((byte) frame.Pop()));
                return 1;
            }
        }

        internal sealed class NotEqualChar : NotEqualInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                frame.Push(((char) frame.Pop()) != ((char) frame.Pop()));
                return 1;
            }
        }

        internal sealed class NotEqualDouble : NotEqualInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                frame.Push(!(((double) frame.Pop()) == ((double) frame.Pop())));
                return 1;
            }
        }

        internal sealed class NotEqualInt16 : NotEqualInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                frame.Push(((short) frame.Pop()) != ((short) frame.Pop()));
                return 1;
            }
        }

        internal sealed class NotEqualInt32 : NotEqualInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                frame.Push(((int) frame.Pop()) != ((int) frame.Pop()));
                return 1;
            }
        }

        internal sealed class NotEqualInt64 : NotEqualInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                frame.Push(((long) frame.Pop()) != ((long) frame.Pop()));
                return 1;
            }
        }

        internal sealed class NotEqualReference : NotEqualInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                frame.Push(frame.Pop() != frame.Pop());
                return 1;
            }
        }

        internal sealed class NotEqualSByte : NotEqualInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                frame.Push(((sbyte) frame.Pop()) != ((sbyte) frame.Pop()));
                return 1;
            }
        }

        internal sealed class NotEqualSingle : NotEqualInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                frame.Push(!(((float) frame.Pop()) == ((float) frame.Pop())));
                return 1;
            }
        }

        internal sealed class NotEqualUInt16 : NotEqualInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                frame.Push(((ushort) frame.Pop()) != ((ushort) frame.Pop()));
                return 1;
            }
        }

        internal sealed class NotEqualUInt32 : NotEqualInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                frame.Push(((int) frame.Pop()) != ((int) frame.Pop()));
                return 1;
            }
        }

        internal sealed class NotEqualUInt64 : NotEqualInstruction
        {
            public override int Run(InterpretedFrame frame)
            {
                frame.Push(((ulong) frame.Pop()) != ((ulong) frame.Pop()));
                return 1;
            }
        }
    }
}

