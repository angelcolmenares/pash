namespace System.Management.Automation.Interpreter
{
    using System;

    internal abstract class NumericConvertInstruction : Instruction
    {
        internal readonly TypeCode _from;
        internal readonly TypeCode _to;

        protected NumericConvertInstruction(TypeCode from, TypeCode to)
        {
            this._from = from;
            this._to = to;
        }

        public override string ToString()
        {
            return string.Concat(new object[] { this.InstructionName, "(", this._from, "->", this._to, ")" });
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

        internal sealed class Checked : NumericConvertInstruction
        {
            public Checked(TypeCode from, TypeCode to) : base(from, to)
            {
            }

            private object Convert(object obj)
            {
                switch (base._from)
                {
                    case TypeCode.Char:
                        return this.ConvertInt32((char) obj);

                    case TypeCode.SByte:
                        return this.ConvertInt32((sbyte) obj);

                    case TypeCode.Byte:
                        return this.ConvertInt32((byte) obj);

                    case TypeCode.Int16:
                        return this.ConvertInt32((short) obj);

                    case TypeCode.UInt16:
                        return this.ConvertInt32((ushort) obj);

                    case TypeCode.Int32:
                        return this.ConvertInt32((int) obj);

                    case TypeCode.UInt32:
                        return this.ConvertInt64((long) ((int) obj));

                    case TypeCode.Int64:
                        return this.ConvertInt64((long) obj);

                    case TypeCode.UInt64:
                        return this.ConvertUInt64((ulong) obj);

                    case TypeCode.Single:
                        return this.ConvertDouble((double) ((float) obj));

                    case TypeCode.Double:
                        return this.ConvertDouble((double) obj);
                }
                throw Assert.Unreachable;
            }

            private object ConvertDouble(double obj)
            {
                switch (base._to)
                {
                    case TypeCode.Char:
                        return (char) ((ushort) obj);

                    case TypeCode.SByte:
                        return (sbyte) obj;

                    case TypeCode.Byte:
                        return (byte) obj;

                    case TypeCode.Int16:
                        return (short) obj;

                    case TypeCode.UInt16:
                        return (ushort) obj;

                    case TypeCode.Int32:
                        return (int) obj;

                    case TypeCode.UInt32:
                        return (int) obj;

                    case TypeCode.Int64:
                        return (long) obj;

                    case TypeCode.UInt64:
                        return (ulong) obj;

                    case TypeCode.Single:
                        return (float) obj;

                    case TypeCode.Double:
                        return obj;
                }
                throw Assert.Unreachable;
            }

            private object ConvertInt32(int obj)
            {
                switch (base._to)
                {
                    case TypeCode.Char:
                        return (char) obj;

                    case TypeCode.SByte:
                        return (sbyte) obj;

                    case TypeCode.Byte:
                        return (byte) obj;

                    case TypeCode.Int16:
                        return (short) obj;

                    case TypeCode.UInt16:
                        return (ushort) obj;

                    case TypeCode.Int32:
                        return obj;

                    case TypeCode.UInt32:
                        return (int) obj;

                    case TypeCode.Int64:
                        return (long) obj;

                    case TypeCode.UInt64:
                        return (ulong) obj;

                    case TypeCode.Single:
                        return (float) obj;

                    case TypeCode.Double:
                        return (double) obj;
                }
                throw Assert.Unreachable;
            }

            private object ConvertInt64(long obj)
            {
                switch (base._to)
                {
                    case TypeCode.Char:
                        return (char) ((ushort) obj);

                    case TypeCode.SByte:
                        return (sbyte) obj;

                    case TypeCode.Byte:
                        return (byte) obj;

                    case TypeCode.Int16:
                        return (short) obj;

                    case TypeCode.UInt16:
                        return (ushort) obj;

                    case TypeCode.Int32:
                        return (int) obj;

                    case TypeCode.UInt32:
                        return (int) obj;

                    case TypeCode.Int64:
                        return obj;

                    case TypeCode.UInt64:
                        return (ulong) obj;

                    case TypeCode.Single:
                        return (float) obj;

                    case TypeCode.Double:
                        return (double) obj;
                }
                throw Assert.Unreachable;
            }

            private object ConvertUInt64(ulong obj)
            {
                switch (base._to)
                {
                    case TypeCode.Char:
                        return (char) obj;

                    case TypeCode.SByte:
                        return (sbyte) obj;

                    case TypeCode.Byte:
                        return (byte) obj;

                    case TypeCode.Int16:
                        return (short) obj;

                    case TypeCode.UInt16:
                        return (ushort) obj;

                    case TypeCode.Int32:
                        return (int) obj;

                    case TypeCode.UInt32:
                        return (int) obj;

                    case TypeCode.Int64:
                        return (long) obj;

                    case TypeCode.UInt64:
                        return obj;

                    case TypeCode.Single:
                        return (float) obj;

                    case TypeCode.Double:
                        return (double) obj;
                }
                throw Assert.Unreachable;
            }

            public override int Run(InterpretedFrame frame)
            {
                frame.Push(this.Convert(frame.Pop()));
                return 1;
            }

            public override string InstructionName
            {
                get
                {
                    return "CheckedConvert";
                }
            }
        }

        internal sealed class Unchecked : NumericConvertInstruction
        {
            public Unchecked(TypeCode from, TypeCode to) : base(from, to)
            {
            }

            private object Convert(object obj)
            {
                switch (base._from)
                {
                    case TypeCode.Char:
                        return this.ConvertInt32((char) obj);

                    case TypeCode.SByte:
                        return this.ConvertInt32((sbyte) obj);

                    case TypeCode.Byte:
                        return this.ConvertInt32((byte) obj);

                    case TypeCode.Int16:
                        return this.ConvertInt32((short) obj);

                    case TypeCode.UInt16:
                        return this.ConvertInt32((ushort) obj);

                    case TypeCode.Int32:
                        return this.ConvertInt32((int) obj);

                    case TypeCode.UInt32:
                        return this.ConvertInt64((long) ((int) obj));

                    case TypeCode.Int64:
                        return this.ConvertInt64((long) obj);

                    case TypeCode.UInt64:
                        return this.ConvertUInt64((ulong) obj);

                    case TypeCode.Single:
                        return this.ConvertDouble((double) ((float) obj));

                    case TypeCode.Double:
                        return this.ConvertDouble((double) obj);
                }
                throw Assert.Unreachable;
            }

            private object ConvertDouble(double obj)
            {
                switch (base._to)
                {
                    case TypeCode.Char:
                        return (char) ((ushort) obj);

                    case TypeCode.SByte:
                        return (sbyte) obj;

                    case TypeCode.Byte:
                        return (byte) obj;

                    case TypeCode.Int16:
                        return (short) obj;

                    case TypeCode.UInt16:
                        return (ushort) obj;

                    case TypeCode.Int32:
                        return (int) obj;

                    case TypeCode.UInt32:
                        return (int) obj;

                    case TypeCode.Int64:
                        return (long) obj;

                    case TypeCode.UInt64:
                        return (ulong) obj;

                    case TypeCode.Single:
                        return (float) obj;

                    case TypeCode.Double:
                        return obj;
                }
                throw Assert.Unreachable;
            }

            private object ConvertInt32(int obj)
            {
                switch (base._to)
                {
                    case TypeCode.Char:
                        return (char) obj;

                    case TypeCode.SByte:
                        return (sbyte) obj;

                    case TypeCode.Byte:
                        return (byte) obj;

                    case TypeCode.Int16:
                        return (short) obj;

                    case TypeCode.UInt16:
                        return (ushort) obj;

                    case TypeCode.Int32:
                        return obj;

                    case TypeCode.UInt32:
                        return (int) obj;

                    case TypeCode.Int64:
                        return (long) obj;

                    case TypeCode.UInt64:
                        return (ulong) obj;

                    case TypeCode.Single:
                        return (float) obj;

                    case TypeCode.Double:
                        return (double) obj;
                }
                throw Assert.Unreachable;
            }

            private object ConvertInt64(long obj)
            {
                switch (base._to)
                {
                    case TypeCode.Char:
                        return (char) ((ushort) obj);

                    case TypeCode.SByte:
                        return (sbyte) obj;

                    case TypeCode.Byte:
                        return (byte) obj;

                    case TypeCode.Int16:
                        return (short) obj;

                    case TypeCode.UInt16:
                        return (ushort) obj;

                    case TypeCode.Int32:
                        return (int) obj;

                    case TypeCode.UInt32:
                        return (int) obj;

                    case TypeCode.Int64:
                        return obj;

                    case TypeCode.UInt64:
                        return (ulong) obj;

                    case TypeCode.Single:
                        return (float) obj;

                    case TypeCode.Double:
                        return (double) obj;
                }
                throw Assert.Unreachable;
            }

            private object ConvertUInt64(ulong obj)
            {
                switch (base._to)
                {
                    case TypeCode.Char:
                        return (char) obj;

                    case TypeCode.SByte:
                        return (sbyte) obj;

                    case TypeCode.Byte:
                        return (byte) obj;

                    case TypeCode.Int16:
                        return (short) obj;

                    case TypeCode.UInt16:
                        return (ushort) obj;

                    case TypeCode.Int32:
                        return (int) obj;

                    case TypeCode.UInt32:
                        return (int) obj;

                    case TypeCode.Int64:
                        return (long) obj;

                    case TypeCode.UInt64:
                        return obj;

                    case TypeCode.Single:
                        return (float) obj;

                    case TypeCode.Double:
                        return (double) obj;
                }
                throw Assert.Unreachable;
            }

            public override int Run(InterpretedFrame frame)
            {
                frame.Push(this.Convert(frame.Pop()));
                return 1;
            }

            public override string InstructionName
            {
                get
                {
                    return "UncheckedConvert";
                }
            }
        }
    }
}

